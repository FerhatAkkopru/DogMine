using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Uyuyan Köpekler - 2. adım
// Dokun: kaz · Basılı tut (ya da sağ tık): mama kabı · Rakama dokun: çevresini aç
// 3 can · ekranda sayaç · oyun bitince dokunarak yeniden başla
public class Board : MonoBehaviour
{
    [Header("Bahçe boyutu")]
    [SerializeField] int cols = 8;
    [SerializeField] int rows = 10;
    [SerializeField] int dogCount = 12;

    [Header("Oynanış")]
    [SerializeField] int startHearts = 3;
    [SerializeField] float longPressTime = 0.35f;

    // Renkler
    readonly Color grassA = new Color(0.54f, 0.80f, 0.38f);
    readonly Color grassB = new Color(0.49f, 0.75f, 0.33f);
    readonly Color soilA = new Color(0.94f, 0.84f, 0.65f);
    readonly Color soilB = new Color(0.91f, 0.79f, 0.58f);
    readonly Color dogColor = new Color(0.64f, 0.38f, 0.23f);
    readonly Color bowlColor = new Color(0.88f, 0.30f, 0.24f);
    readonly Color wrongColor = new Color(0.45f, 0.45f, 0.45f);
    readonly Color[] numberColors =
    {
        Color.clear,
        new Color(0.18f, 0.44f, 0.86f), // 1
        new Color(0.17f, 0.54f, 0.24f), // 2
        new Color(0.84f, 0.25f, 0.17f), // 3
        new Color(0.42f, 0.25f, 0.71f), // 4
        new Color(0.63f, 0.18f, 0.18f), // 5
        new Color(0.06f, 0.52f, 0.52f), // 6
        new Color(0.17f, 0.15f, 0.12f), // 7
        new Color(0.42f, 0.39f, 0.33f)  // 8
    };

    // Oyun durumu
    bool[,] dog, open, flag;
    int[,] near;
    bool started, over;
    int opened, flags, woken, hearts;
    float overAt;

    // Dokunma takibi
    bool pressing, longDone;
    float pressTime;
    Vector2 pressPos;

    // Görseller
    SpriteRenderer[,] tiles;
    TextMesh[,] labels;
    TextMesh hud, hint, message;
    SpriteRenderer panel;
    Camera cam;
    Sprite square;
    Font font;

    void Start()
    {
        cam = Camera.main;

        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        square = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        NewGame();
    }

    // ---------- Kurulum ----------

    void NewGame()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        dog = new bool[cols, rows];
        open = new bool[cols, rows];
        flag = new bool[cols, rows];
        near = new int[cols, rows];
        tiles = new SpriteRenderer[cols, rows];
        labels = new TextMesh[cols, rows];
        started = over = pressing = false;
        opened = flags = woken = 0;
        hearts = startHearts;

        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
            {
                var tile = new GameObject($"Kare {c},{r}");
                tile.transform.SetParent(transform);
                tile.transform.position = CellCenter(c, r);
                tile.transform.localScale = Vector3.one * 0.95f;
                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                tiles[c, r] = sr;
                labels[c, r] = MakeText("Rakam", tile.transform, Vector3.zero, 0.1f, Color.white, 1, false);
                Paint(c, r);
            }

        float top = rows / 2f;
        hud = MakeText("Sayaç", transform, new Vector3(0, top + 1.2f, 0), 0.09f, Color.white, 1, true);
        hint = MakeText("İpucu", transform, new Vector3(0, top + 0.55f, 0), 0.055f, new Color(1f, 0.85f, 0.4f), 1, true);

        var panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(transform);
        panelObj.transform.localScale = new Vector3(cols + 0.6f, 3.2f, 1);
        panel = panelObj.AddComponent<SpriteRenderer>();
        panel.sprite = square;
        panel.color = new Color(0.08f, 0.12f, 0.07f, 0.85f);
        panel.sortingOrder = 5;
        panel.enabled = false;
        message = MakeText("Mesaj", transform, Vector3.zero, 0.08f, Color.white, 6, true);

        FitCamera();
        UpdateHud();
    }

    TextMesh MakeText(string name, Transform parent, Vector3 localPos, float size, Color color, int order, bool worldParent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, !worldParent ? false : true);
        go.transform.localPosition = localPos;
        var tm = go.AddComponent<TextMesh>();
        tm.font = font;
        tm.fontSize = 64;
        tm.characterSize = size;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = color;
        tm.text = "";
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = font.material;
        mr.sortingOrder = order;
        return tm;
    }

    void FitCamera()
    {
        float halfW = cols / 2f + 0.5f;
        float halfH = rows / 2f + 2f; // üstte sayaç için yer
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(halfH, halfW / cam.aspect);
        cam.transform.position = new Vector3(0, 0.6f, -10);
    }

    // ---------- Girdi ----------

    void Update()
    {
        var p = Pointer.current;
        if (p == null) return;

        if (over)
        {
            if (p.press.wasPressedThisFrame && Time.time - overAt > 0.6f) NewGame();
            return;
        }

        // Bilgisayarda sağ tık = mama kabı
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (TryCell(Mouse.current.position.ReadValue(), out int mc, out int mr)) ToggleBowl(mc, mr);
            return;
        }

        if (p.press.wasPressedThisFrame)
        {
            pressing = true;
            longDone = false;
            pressTime = Time.time;
            pressPos = p.position.ReadValue();
        }

        if (pressing && p.press.isPressed && !longDone)
        {
            if ((p.position.ReadValue() - pressPos).magnitude > 30f) pressing = false; // parmak kaydıysa iptal
            else if (Time.time - pressTime > longPressTime)
            {
                longDone = true;
                if (TryCell(pressPos, out int lc, out int lr)) ToggleBowl(lc, lr);
            }
        }

        if (pressing && p.press.wasReleasedThisFrame)
        {
            pressing = false;
            if (!longDone && TryCell(pressPos, out int tc, out int tr)) Tap(tc, tr);
        }
    }

    bool TryCell(Vector2 screen, out int c, out int r)
    {
        Vector2 w = cam.ScreenToWorldPoint(screen);
        c = Mathf.FloorToInt(w.x + cols / 2f);
        r = Mathf.FloorToInt(w.y + rows / 2f);
        return Inside(c, r);
    }

    // ---------- Oynanış ----------

    void Tap(int c, int r)
    {
        hint.text = "";
        if (flag[c, r]) return;
        if (open[c, r]) { Chord(c, r); return; }

        if (!started)
        {
            PlaceDogs(c, r);
            started = true;
        }

        if (dog[c, r]) Wake(c, r);
        else OpenArea(c, r);
        AfterMove();
    }

    void ToggleBowl(int c, int r)
    {
        if (!started || open[c, r]) return;
        flag[c, r] = !flag[c, r];
        flags += flag[c, r] ? 1 : -1;
        hint.text = "";
        Paint(c, r);
        UpdateHud();
    }

    void Chord(int c, int r)
    {
        // Rakam kadar köpek işaretlenmişse kalan komşuları aç.
        int n = near[c, r];
        if (n == 0 || dog[c, r]) return;

        int marked = 0;
        ForNeighbors(c, r, (cc, rr) => { if (flag[cc, rr] || (open[cc, rr] && dog[cc, rr])) marked++; });
        if (marked != n) return;

        var targets = new List<Vector2Int>();
        ForNeighbors(c, r, (cc, rr) => { if (!open[cc, rr] && !flag[cc, rr]) targets.Add(new Vector2Int(cc, rr)); });
        foreach (var t in targets)
        {
            if (over) return;
            if (dog[t.x, t.y]) Wake(t.x, t.y);
            else OpenArea(t.x, t.y);
        }
        AfterMove();
    }

    void Wake(int c, int r)
    {
        open[c, r] = true;
        woken++;
        hearts--;
        Paint(c, r);
        UpdateHud();
        if (hearts <= 0) Lose();
        else hint.text = "Hav! Bir köpek uyandı.";
    }

    void AfterMove()
    {
        if (over) return;
        UpdateHud();

        if (opened == cols * rows - dogCount) { Win(); return; }

        // Kazılacak kare kalmadıysa ama oyun bitmediyse: bir kap boş karede.
        bool anyLeft = false;
        for (int c = 0; c < cols && !anyLeft; c++)
            for (int r = 0; r < rows && !anyLeft; r++)
                if (!open[c, r] && !dog[c, r] && !flag[c, r]) anyLeft = true;
        if (!anyLeft) hint.text = "Bir mama kabı boş bir karede. Birini kaldırıp kaz.";
    }

    void PlaceDogs(int safeC, int safeR)
    {
        var candidates = new List<Vector2Int>();
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
                if (Mathf.Abs(c - safeC) > 1 || Mathf.Abs(r - safeR) > 1)
                    candidates.Add(new Vector2Int(c, r));

        for (int i = 0; i < dogCount && candidates.Count > 0; i++)
        {
            int k = Random.Range(0, candidates.Count);
            dog[candidates[k].x, candidates[k].y] = true;
            candidates.RemoveAt(k);
        }

        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
            {
                int n = 0;
                ForNeighbors(c, r, (cc, rr) => { if (dog[cc, rr]) n++; });
                near[c, r] = n;
            }
    }

    void OpenArea(int startC, int startR)
    {
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startC, startR));

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            if (open[p.x, p.y] || dog[p.x, p.y] || flag[p.x, p.y]) continue;

            open[p.x, p.y] = true;
            opened++;
            Paint(p.x, p.y);

            if (near[p.x, p.y] == 0)
                ForNeighbors(p.x, p.y, (cc, rr) => { if (!open[cc, rr]) queue.Enqueue(new Vector2Int(cc, rr)); });
        }
    }

    // ---------- Bitiş ----------

    void Win()
    {
        over = true;
        overAt = Time.time;
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
                if (dog[c, r] && !open[c, r] && !flag[c, r]) { flag[c, r] = true; Paint(c, r); }

        string line = hearts == startHearts ? "Hiçbir köpek uyanmadı!" : $"{startHearts - hearts} köpek söylendi ama tekrar uyudu.";
        ShowMessage($"Bahçe tamam!\n{line}\n\nYeniden oynamak için dokun");
    }

    void Lose()
    {
        over = true;
        overAt = Time.time;
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
            {
                if (dog[c, r] && !open[c, r] && !flag[c, r]) { open[c, r] = true; Paint(c, r); }
                else if (flag[c, r] && !dog[c, r]) { tiles[c, r].color = wrongColor; labels[c, r].text = "X"; }
            }
        ShowMessage("Herkes uyandı!\nÜç köpek uyanınca bahçe ayaklandı.\n\nTekrar denemek için dokun");
    }

    void ShowMessage(string text)
    {
        panel.enabled = true;
        message.text = text;
        hint.text = "";
        UpdateHud();
    }

    // ---------- Çizim ----------

    void Paint(int c, int r)
    {
        var sr = tiles[c, r];
        var tm = labels[c, r];
        bool alt = (c + r) % 2 == 1;

        if (open[c, r] && dog[c, r]) { sr.color = dogColor; tm.text = "!"; tm.color = Color.white; return; }
        if (open[c, r])
        {
            sr.color = alt ? soilB : soilA;
            int n = near[c, r];
            tm.text = n > 0 ? n.ToString() : "";
            tm.color = numberColors[n];
            return;
        }
        if (flag[c, r]) { sr.color = bowlColor; tm.text = "zz"; tm.color = Color.white; return; }

        sr.color = alt ? grassB : grassA;
        tm.text = "";
    }

    void UpdateHud()
    {
        int left = Mathf.Max(0, dogCount - flags - woken);
        hud.text = $"Köpek: {left}      Can: {Mathf.Max(0, hearts)}/{startHearts}";
    }

    // ---------- Yardımcılar ----------

    void ForNeighbors(int c, int r, System.Action<int, int> action)
    {
        for (int dc = -1; dc <= 1; dc++)
            for (int dr = -1; dr <= 1; dr++)
            {
                if (dc == 0 && dr == 0) continue;
                int cc = c + dc, rr = r + dr;
                if (Inside(cc, rr)) action(cc, rr);
            }
    }

    Vector3 CellCenter(int c, int r) => new Vector3(c - cols / 2f + 0.5f, r - rows / 2f + 0.5f, 0);
    bool Inside(int c, int r) => c >= 0 && c < cols && r >= 0 && r < rows;
}
