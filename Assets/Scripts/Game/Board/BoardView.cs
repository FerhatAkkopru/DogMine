using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UyuyanKopekler.Art;
using UyuyanKopekler.Core;
using UyuyanKopekler.UI;

namespace UyuyanKopekler.Game
{
    // Bahçenin dünyadaki görüntüsü. Modeli okur, kareleri çizer ve hamlelerin animasyonlarını
    // oynatır. Kuralları bilmez; neyin değiştiğini MoveResult'tan öğrenir.
    public sealed class BoardView : MonoBehaviour
    {
        const float FramePadding = 0.32f;
        const float ScreenMargin = 0.35f;
        const float WaveStep = 0.028f;
        const float MaxWaveDelay = 0.55f;

        IGameArt art;
        Transform boardRoot;
        SpriteRenderer frame;
        SpriteRenderer frameShadow;
        TextMesh bark;
        readonly List<TileView> pool = new List<TileView>();
        TileView[,] tiles;
        BoardModel board;
        TileView pressed;
        Coroutine shake;
        Coroutine barkRoutine;

        public SpriteRenderer Backdrop { get; private set; }

        // Kameranın sığdırması gereken alan (çerçeve ve kenar payı dahil).
        public Vector2 WorldSize => board == null
            ? new Vector2(10f, 12f)
            : new Vector2(board.Columns + 2f * (FramePadding + ScreenMargin), board.Rows + 2f * (FramePadding + ScreenMargin));

        public static BoardView Create(IGameArt art, Transform parent)
        {
            var go = new GameObject("Bahçe");
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<BoardView>();
            view.art = art;

            view.Backdrop = new GameObject("Çimen").AddComponent<SpriteRenderer>();
            view.Backdrop.transform.SetParent(go.transform, false);
            view.Backdrop.sprite = art.GardenBackground;
            view.Backdrop.sortingOrder = -100;

            view.boardRoot = new GameObject("Kareler").transform;
            view.boardRoot.SetParent(go.transform, false);

            view.frameShadow = view.CreateFrame("Çerçeve gölgesi", Palette.Shadow, -11);
            view.frameShadow.transform.localPosition = new Vector3(0f, -0.14f, 0f);
            view.frame = view.CreateFrame("Çerçeve", Palette.BoardFrame, -10);

            var barkGo = new GameObject("Hav");
            barkGo.transform.SetParent(view.boardRoot, false);
            view.bark = barkGo.AddComponent<TextMesh>();
            view.bark.font = art.Font;
            view.bark.fontSize = 64;
            view.bark.characterSize = 0.13f;
            view.bark.anchor = TextAnchor.MiddleCenter;
            view.bark.fontStyle = FontStyle.Bold;
            view.bark.text = "Hav!";
            var barkRenderer = barkGo.GetComponent<MeshRenderer>();
            barkRenderer.sharedMaterial = art.Font.material;
            barkRenderer.sortingOrder = 20;
            barkGo.SetActive(false);
            return view;
        }

        SpriteRenderer CreateFrame(string name, Color color, int order)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(boardRoot, false);
            sr.sprite = art.BoardFrame;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        // ---------- Kurulum ----------

        public void Build(BoardModel model)
        {
            board = model;
            pressed = null;
            StopShake();

            int needed = model.Columns * model.Rows;
            while (pool.Count < needed) pool.Add(TileView.Create(boardRoot, art));
            for (int i = 0; i < pool.Count; i++) pool[i].gameObject.SetActive(i < needed);

            tiles = new TileView[model.Columns, model.Rows];
            int index = 0;
            foreach (var cell in model.AllCells())
            {
                var tile = pool[index++];
                tile.Place(cell, CellPosition(cell));
                tiles[cell.X, cell.Y] = tile;
                Refresh(cell);
            }

            var frameSize = new Vector2(model.Columns + 2f * FramePadding, model.Rows + 2f * FramePadding);
            frame.size = frameSize;
            frameShadow.size = frameSize;
        }

        public Vector3 CellPosition(Cell cell) =>
            new Vector3(cell.X - board.Columns / 2f + 0.5f, cell.Y - board.Rows / 2f + 0.5f, 0f);

        public bool TryGetCell(Vector2 screenPosition, Camera camera, out Cell cell)
        {
            cell = default;
            if (board == null || !gameObject.activeInHierarchy) return false;

            Vector3 world = camera.ScreenToWorldPoint(screenPosition);
            Vector3 local = boardRoot.InverseTransformPoint(world);
            cell = new Cell(Mathf.FloorToInt(local.x + board.Columns / 2f), Mathf.FloorToInt(local.y + board.Rows / 2f));
            return board.Contains(cell);
        }

        // Karenin görüntüsünü modeldeki durumuna göre çizer.
        public void Refresh(Cell cell)
        {
            var tile = tiles[cell.X, cell.Y];
            bool alternate = (cell.X + cell.Y) % 2 == 1;
            switch (board.GetState(cell))
            {
                case CellState.Hidden:
                    tile.ShowGrass(alternate, bowl: false);
                    break;
                case CellState.Flagged:
                    tile.ShowGrass(alternate, bowl: true);
                    break;
                default:
                    if (board.HasDog(cell)) tile.ShowWokenDog();
                    else tile.ShowSoil(alternate, board.GetNearbyDogs(cell));
                    break;
            }
        }

        // ---------- Animasyonlar ----------

        public void PlayMove(MoveResult move)
        {
            foreach (var revealed in move.Reveal.Revealed)
            {
                Refresh(revealed.Cell);
                tiles[revealed.Cell.X, revealed.Cell.Y].Pop(Mathf.Min(revealed.Depth * WaveStep, MaxWaveDelay));
            }

            foreach (var dog in move.Reveal.WokenDogs)
            {
                Refresh(dog);
                tiles[dog.X, dog.Y].PopIcon(0f, overshoot: 2f);
                Bark(dog);
            }
            if (move.WokeDog) Shake();

            for (int i = 0; i < move.AutoFlagged.Count; i++)
            {
                var cell = move.AutoFlagged[i];
                Refresh(cell);
                tiles[cell.X, cell.Y].PopIcon(0.2f + i * 0.06f);
            }
        }

        public void PlayFlag(Cell cell)
        {
            Refresh(cell);
            if (board.GetState(cell) == CellState.Flagged) tiles[cell.X, cell.Y].PopIcon(0f);
        }

        // Kazanınca: dokunulan yerden yayılan sevinç dalgası.
        public void PlayWin(Cell focus)
        {
            foreach (var cell in board.AllCells())
                tiles[cell.X, cell.Y].Bump(0.3f + cell.StepsTo(focus) * 0.04f);
        }

        // Kaybedince: kalan köpekler sırayla uyanır, yanlış mama kapları işaretlenir.
        public void PlayLoss(Cell focus)
        {
            foreach (var cell in board.AllCells())
            {
                var state = board.GetState(cell);
                var tile = tiles[cell.X, cell.Y];
                if (board.HasDog(cell) && state == CellState.Hidden)
                {
                    tile.ShowWokenDog();
                    tile.PopIcon(0.1f + cell.StepsTo(focus) * 0.06f);
                }
                else if (!board.HasDog(cell) && state == CellState.Flagged)
                {
                    tile.ShowWrongBowl();
                }
            }
            Shake();
        }

        // Parmak basılıyken çimli kare hafifçe çöker ve aydınlanır.
        public void SetPressed(Cell? cell)
        {
            if (pressed != null) pressed.SetPressed(false);
            pressed = null;
            if (cell == null || board == null || !board.Contains(cell.Value)) return;
            if (board.GetState(cell.Value) != CellState.Hidden) return;
            pressed = tiles[cell.Value.X, cell.Value.Y];
            pressed.SetPressed(true);
        }

        void Shake()
        {
            StopShake();
            shake = StartCoroutine(Tween.Run(0.35f, t =>
            {
                float strength = 0.12f * (1f - t);
                boardRoot.localPosition = new Vector3(Mathf.Sin(t * 60f) * strength, Mathf.Cos(t * 47f) * strength * 0.5f, 0f);
            }));
        }

        void StopShake()
        {
            if (shake != null) StopCoroutine(shake);
            shake = null;
            boardRoot.localPosition = Vector3.zero;
        }

        // Uyanan köpeğin üstünde yükselip kaybolan "Hav!" yazısı.
        void Bark(Cell cell)
        {
            if (barkRoutine != null) StopCoroutine(barkRoutine);
            barkRoutine = StartCoroutine(BarkRoutine(CellPosition(cell)));
        }

        IEnumerator BarkRoutine(Vector3 at)
        {
            bark.gameObject.SetActive(true);
            yield return Tween.Run(0.9f, t =>
            {
                bark.transform.localPosition = at + new Vector3(0f, 0.55f + t * 0.8f, 0f);
                float s = Mathf.LerpUnclamped(0.6f, 1f, Tween.OutBack(Mathf.Clamp01(t * 3f)));
                bark.transform.localScale = new Vector3(s, s, 1f);
                bark.color = Palette.WithAlpha(Palette.Cream, 1f - t * t);
            });
            bark.gameObject.SetActive(false);
        }
    }
}
