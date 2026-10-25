using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UyuyanKopekler.Anim;
using UyuyanKopekler.Art;
using UyuyanKopekler.Audio;
using UyuyanKopekler.Core;
using UyuyanKopekler.UI;

namespace UyuyanKopekler.Game
{
    // Bahçenin dünyadaki görüntüsü. Modeli okur, kareleri çizer ve hamlelerin animasyonlarını
    // oynatır: açılan kareler dalga halinde, uyanan köpek, kedi sahnesi, kaybedince dört bir
    // yana kaçışan köpekler. Kuralları bilmez; neyin değiştiğini MoveResult'tan öğrenir.
    public sealed class BoardView : MonoBehaviour
    {
        const float FramePadding = 0.32f;
        const float ScreenMargin = 0.35f;
        const float WaveStep = 0.028f;
        const float MaxWaveDelay = 0.55f;
        const int BigDigCells = 6;

        const float RunnerScale = 1.35f;
        const float RunnerSpeed = 7.5f;
        const float CatScale = 1.45f;
        const float CatWalkSpeed = 4.2f;
        const float CatRunSpeed = 10f;
        const int ActorOrder = 40;

        IGameArt art;
        ClipLibrary clips;
        ISoundPlayer sound;
        Transform boardRoot;
        SpriteRenderer frame;
        SpriteRenderer frameShadow;
        TextMesh bark;
        readonly List<TileView> pool = new List<TileView>();
        readonly List<GameObject> actors = new List<GameObject>();
        TileView[,] tiles;
        BoardModel board;
        TileView pressed;
        Coroutine shake;
        Coroutine barkRoutine;

        public SpriteRenderer Backdrop { get; private set; }

        // Açık karelerde rakam yerine pati izi gösterilsin mi (Ayarlar).
        public bool UsePawHints { get; set; } = true;

        // Kameranın sığdırması gereken alan (çerçeve ve kenar payı dahil).
        public Vector2 WorldSize => board == null
            ? new Vector2(10f, 12f)
            : new Vector2(board.Columns + 2f * (FramePadding + ScreenMargin), board.Rows + 2f * (FramePadding + ScreenMargin));

        public static BoardView Create(ClipLibrary clips, ISoundPlayer sound, Transform parent)
        {
            var go = new GameObject("Bahçe");
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<BoardView>();
            view.clips = clips;
            view.art = clips.Art;
            view.sound = sound;
            var art = view.art;

            view.Backdrop = new GameObject("Kâğıt").AddComponent<SpriteRenderer>();
            view.Backdrop.transform.SetParent(go.transform, false);
            view.Backdrop.sprite = art.Paper;
            view.Backdrop.drawMode = SpriteDrawMode.Tiled;
            view.Backdrop.sortingOrder = -100;

            view.boardRoot = new GameObject("Kareler").transform;
            view.boardRoot.SetParent(go.transform, false);

            view.frameShadow = view.CreateFrame("Çerçeve gölgesi", Palette.Shadow, -11);
            view.frameShadow.transform.localPosition = new Vector3(0.06f, -0.13f, 0f);
            view.frame = view.CreateFrame("Çerçeve", Color.white, -10);

            var barkGo = new GameObject("Hav");
            barkGo.transform.SetParent(view.boardRoot, false);
            view.bark = barkGo.AddComponent<TextMesh>();
            view.bark.font = art.DisplayFont;
            view.bark.fontSize = 64;
            view.bark.characterSize = 0.13f;
            view.bark.anchor = TextAnchor.MiddleCenter;
            view.bark.fontStyle = FontStyle.Normal;
            view.bark.text = "Hav!";
            var barkRenderer = barkGo.GetComponent<MeshRenderer>();
            barkRenderer.sharedMaterial = art.DisplayFont.material;
            barkRenderer.sortingOrder = ActorOrder + 50;
            barkGo.SetActive(false);
            return view;
        }

        SpriteRenderer CreateFrame(string name, Color color, int order)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(boardRoot, false);
            sr.sprite = art.BoardBed;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        public void SetVisible(bool visible)
        {
            if (!visible) ClearActors();
            gameObject.SetActive(visible);
        }

        // ---------- Kurulum ----------

        public void Build(BoardModel model)
        {
            board = model;
            pressed = null;
            StopAllCoroutines();
            shake = null;
            barkRoutine = null;
            bark.gameObject.SetActive(false);
            StopShake();
            ClearActors();

            int needed = model.Columns * model.Rows;
            while (pool.Count < needed) pool.Add(TileView.Create(boardRoot, clips));
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

        public Vector3 CellWorldPosition(Cell cell) => boardRoot.TransformPoint(CellPosition(cell));

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
            switch (board.GetState(cell))
            {
                case CellState.Hidden:
                    tile.ShowGrass(IsLight(cell), bowl: false);
                    break;
                case CellState.Flagged:
                    tile.ShowGrass(IsLight(cell), bowl: true);
                    break;
                default:
                    if (board.HasDog(cell)) tile.ShowAwakeDog(wakeUp: false);
                    else tile.ShowSoil(board.GetNearbyDogs(cell), UsePawHints);
                    break;
            }
        }

        // Dama tahtası: komşu çim kareleri iki tonda.
        static bool IsLight(Cell cell) => (cell.X + cell.Y) % 2 == 1;

        // ---------- Hamle animasyonları ----------

        public void PlayMove(MoveResult move)
        {
            PlayReveal(move.Reveal);

            foreach (var dog in move.Reveal.WokenDogs)
            {
                tiles[dog.X, dog.Y].ShowAwakeDog(wakeUp: true);
                Bark(dog, 0.25f);
            }
            if (move.WokeDog) Shake();
            // Kazanınca otomatik konan mama kapları çizilmez: PlayWin uyuyan köpekleri gösterir.
        }

        void PlayReveal(RevealResult reveal)
        {
            foreach (var revealed in reveal.Revealed)
            {
                Refresh(revealed.Cell);
                tiles[revealed.Cell.X, revealed.Cell.Y].Pop(Mathf.Min(revealed.Depth * WaveStep, MaxWaveDelay));
            }
            if (reveal.Revealed.Count > 0) sound.Play(reveal.Revealed.Count >= BigDigCells ? Sfx.BigDig : Sfx.Dig);
        }

        public void PlayFlag(Cell cell)
        {
            Refresh(cell);
            bool placed = board.GetState(cell) == CellState.Flagged;
            if (placed) tiles[cell.X, cell.Y].PopIcon(0f);
            sound.Play(placed ? Sfx.Bowl : Sfx.BowlOff);
        }

        // Kazanınca: dokunulan yerden yayılan sevinç dalgası; hiç uyanmayan köpekler
        // kıvrılmış uyurken ortaya çıkar.
        public void PlayWin(Cell focus)
        {
            foreach (var cell in board.AllCells())
            {
                var tile = tiles[cell.X, cell.Y];
                float delay = 0.3f + cell.StepsTo(focus) * 0.04f;
                if (board.HasDog(cell) && board.GetState(cell) != CellState.Revealed)
                {
                    tile.ShowSleepingDog();
                    tile.PopIcon(delay);
                }
                tile.Bump(delay);
            }
        }

        // Kaybedince: bahçe ayaklanır, bütün köpekler dört bir yana kaçışır;
        // yanlış konmuş mama kapları işaretlenir.
        public void PlayLoss(Cell focus)
        {
            foreach (var cell in board.AllCells())
            {
                var state = board.GetState(cell);
                if (board.HasDog(cell))
                    StartCoroutine(RunAway(cell, 0.15f + cell.StepsTo(focus) * 0.06f));
                else if (state == CellState.Flagged)
                    tiles[cell.X, cell.Y].ShowWrongBowl(IsLight(cell));
            }
            Shake();
            sound.Play(Sfx.Whoosh);
        }

        IEnumerator RunAway(Cell cell, float delay)
        {
            yield return new WaitForSeconds(delay);
            var tile = tiles[cell.X, cell.Y];
            tile.ShowEmptyHole();
            tile.Pop(0f);

            // Bahçenin ortasından dışarı doğru, biraz rastgele bir yön
            var from = CellPosition(cell);
            Vector2 away = (Vector2)from;
            if (away.sqrMagnitude < 0.01f) away = UnityEngine.Random.insideUnitCircle;
            float angle = Mathf.Atan2(away.y, away.x) + UnityEngine.Random.Range(-0.7f, 0.7f);
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var runner = SpawnActor("Kaçan köpek", RunnerScale, from);
            runner.renderer.flipX = direction.x < 0f;
            runner.animator.Play(clips.Dog(tile.Coat, Clips.Run), Clips.Run.Fps * UnityEngine.Random.Range(0.9f, 1.15f), true);
            StartCoroutine(Puff(from + new Vector3(-direction.x * 0.3f, -0.3f, 0f)));
            if (UnityEngine.Random.value < 0.35f) sound.Play(Sfx.Bark);

            float speed = RunnerSpeed * UnityEngine.Random.Range(0.85f, 1.2f);
            float limit = Mathf.Max(board.Columns, board.Rows) + 6f;
            while (runner.go != null && runner.go.transform.localPosition.magnitude < limit)
            {
                var p = runner.go.transform.localPosition + (Vector3)(direction * speed * Time.deltaTime);
                runner.go.transform.localPosition = p;
                runner.renderer.sortingOrder = ActorOrder + Mathf.RoundToInt(-p.y * 4f);
                yield return null;
            }
            RemoveActor(runner.go);
        }

        // ---------- Kedi sahnesi ----------

        // Kedi kenardan yürüyerek gelir, kareye oturur ve miyavlar; çevredeki köpekler
        // uyanıp ona havlar, kedi ürküp kaçar, köpekler mama kaplarıyla yeniden uyur.
        public void PlayCat(Cell target, MoveResult move, Action onDone) => StartCoroutine(CatScene(target, move, onDone));

        IEnumerator CatScene(Cell target, MoveResult move, Action onDone)
        {
            var dest = CellPosition(target) + new Vector3(0f, 0.1f, 0f);
            bool fromLeft = target.X < board.Columns / 2f;
            float edge = board.Columns / 2f + 2.5f;
            var start = new Vector3(fromLeft ? -edge : edge, dest.y, 0f);

            var cat = SpawnActor("Kedi", CatScale, start);
            cat.renderer.flipX = !fromLeft;
            cat.animator.Play(clips.Cat(Clips.CatWalk), Clips.CatWalk.Fps, true);
            sound.Play(Sfx.CatDrop);
            yield return MoveActor(cat, dest, CatWalkSpeed);

            cat.animator.Play(clips.Cat(Clips.CatSit), Clips.CatSit.Fps, true);
            sound.Play(Sfx.Meow);
            yield return new WaitForSeconds(0.5f);

            // Çevredeki köpekler uyanıp kediye havlar; kedinin oturduğu kare güvenliyse açılır.
            foreach (var dog in move.CatFoundDogs)
            {
                var tile = tiles[dog.X, dog.Y];
                tile.ShowBarkingDog(IsLight(dog), faceLeft: CellPosition(dog).x > dest.x);
                tile.PopIcon(0f, overshoot: 1.5f);
            }
            if (move.CatFoundDogs.Count > 0)
            {
                sound.Play(Sfx.Bark);
                Bark(move.CatFoundDogs[0], 0f);
                Shake();
            }
            PlayReveal(move.Reveal);
            yield return new WaitForSeconds(0.35f);

            if (move.CatFoundDogs.Count > 0)
            {
                cat.animator.Play(clips.Cat(Clips.CatStartle), Clips.CatStartle.Fps, false);
                cat.renderer.flipX = fromLeft; // köpeklerden uzağa döner
                yield return new WaitForSeconds(0.3f);
            }

            cat.renderer.flipX = fromLeft;
            cat.animator.Play(clips.Cat(Clips.CatRun), Clips.CatRun.Fps, true);
            sound.Play(Sfx.Whoosh);
            yield return MoveActor(cat, new Vector3(fromLeft ? -edge - 2f : edge + 2f, dest.y + 0.4f, 0f), CatRunSpeed);
            RemoveActor(cat.go);

            // Köpekler yatışır: üstlerinde artık mama kabı var.
            foreach (var dog in move.CatFoundDogs)
            {
                Refresh(dog);
                tiles[dog.X, dog.Y].PopIcon(0f);
            }
            if (move.CatFoundDogs.Count > 0) sound.Play(Sfx.Bowl);
            onDone?.Invoke();
        }

        // ---------- Oyuncular (kedi, kaçan köpekler) ----------

        struct Actor
        {
            public GameObject go;
            public SpriteRenderer renderer;
            public SpriteAnimator animator;
        }

        Actor SpawnActor(string name, float scale, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(boardRoot, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = ActorOrder;
            actors.Add(go);
            return new Actor { go = go, renderer = renderer, animator = SpriteAnimator.For(renderer) };
        }

        IEnumerator MoveActor(Actor actor, Vector3 to, float speed)
        {
            while (actor.go != null)
            {
                var p = actor.go.transform.localPosition;
                var next = Vector3.MoveTowards(p, to, speed * Time.deltaTime);
                actor.go.transform.localPosition = next;
                actor.renderer.sortingOrder = ActorOrder + Mathf.RoundToInt(-next.y * 4f);
                if ((next - to).sqrMagnitude < 0.0001f) yield break;
                yield return null;
            }
        }

        IEnumerator Puff(Vector3 at)
        {
            var puff = SpawnActor("Toz", 0.9f, at);
            bool done = false;
            puff.animator.Play(clips.Puff, 10f, false, () => done = true);
            while (!done && puff.go != null) yield return null;
            RemoveActor(puff.go);
        }

        void RemoveActor(GameObject go)
        {
            if (go == null) return;
            actors.Remove(go);
            Destroy(go);
        }

        void ClearActors()
        {
            foreach (var go in actors)
                if (go != null) Destroy(go);
            actors.Clear();
        }

        // ---------- Küçük efektler ----------

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

        // Köpeğin üstünde yükselip kaybolan "Hav!" yazısı (ve sesi).
        void Bark(Cell cell, float delay)
        {
            if (barkRoutine != null) StopCoroutine(barkRoutine);
            barkRoutine = StartCoroutine(BarkRoutine(CellPosition(cell), delay));
        }

        IEnumerator BarkRoutine(Vector3 at, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            sound.Play(Sfx.Bark);
            bark.gameObject.SetActive(true);
            yield return Tween.Run(0.9f, t =>
            {
                bark.transform.localPosition = at + new Vector3(0f, 0.7f + t * 0.8f, 0f);
                float s = Mathf.LerpUnclamped(0.6f, 1f, Tween.OutBack(Mathf.Clamp01(t * 3f)));
                bark.transform.localScale = new Vector3(s, s, 1f);
                bark.color = Palette.WithAlpha(Palette.CoralDark, 1f - t * t);
            });
            bark.gameObject.SetActive(false);
        }
    }
}
