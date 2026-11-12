using UnityEngine;

public class SkyHazards : MonoBehaviour
{
    public Transform player;
    public float calcFromMeters = 30f;
    public float codeFromMeters = 45f;

    GameManager gm;
    Camera cam;
    Transform root;
    float calcTimer, codeTimer;
    bool wasRunning;

    void Start()
    {
        gm = GameManager.Instance;
        if (gm != null && player == null) player = gm.player;
        cam = (gm != null && gm.cam != null) ? gm.cam : Camera.main;
        root = new GameObject("Obstaculos").transform;
    }

    void ClearAll()
    {
        for (int i = root.childCount - 1; i >= 0; i--) Destroy(root.GetChild(i).gameObject);
    }

    void Update()
    {
        if (gm == null || cam == null || player == null || root == null) return;

        bool running = !gm.InMenu && !gm.Dead;
        if (!running)
        {
            if (gm.InMenu) ClearAll();
            wasRunning = false;
            return;
        }
        if (!wasRunning)
        {
            ClearAll();
            calcTimer = 6f;
            codeTimer = 8f;
            wasRunning = true;
        }

        float h = gm.Height;
        float d = Difficulty.At(h);

        if (h >= calcFromMeters)
        {
            calcTimer -= Time.deltaTime;
            if (calcTimer <= 0f)
            {
                SpawnCalculator(d);
                calcTimer = Random.Range(Mathf.Lerp(6f, 2.5f, d), Mathf.Lerp(10f, 5f, d));
            }
        }

        if (h >= codeFromMeters)
        {
            codeTimer -= Time.deltaTime;
            if (codeTimer <= 0f)
            {
                float dir = Random.value < 0.5f ? 1f : -1f;
                float yOff = (player.position.y + Random.Range(-0.3f, 1.4f)) - cam.transform.position.y;
                SideObstacle.Create(root, cam, yOff, dir, Mathf.Lerp(5f, 9f, d));
                codeTimer = Random.Range(Mathf.Lerp(9f, 4f, d), Mathf.Lerp(14f, 7f, d));
            }
        }
    }

    void SpawnCalculator(float d)
    {
        float halfW = cam.orthographicSize * cam.aspect;
        float cx = cam.transform.position.x;
        float x = Mathf.Clamp(player.position.x + Random.Range(-3.5f, 3.5f), cx - halfW + 0.8f, cx + halfW - 0.8f);

        var go = new GameObject("Calculadora");
        go.transform.SetParent(root, false);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = DecorSprites.Calculator;
        sr.sortingOrder = 4;

        var bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(0.6f, 0.85f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var hz = go.AddComponent<Hazard>();
        hz.reason = "Te cayo una calculadora";

        go.AddComponent<FallingObstacle>().Init(cam, x, Mathf.Lerp(5.5f, 8.5f, d));
    }
}

public static class CodeText
{
    static Font font;

    public static TextMesh New(Transform parent, string text, Color color, float charSize, int order)
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go = new GameObject("Texto");
        go.transform.SetParent(parent, false);
        var tm = go.AddComponent<TextMesh>();
        tm.font = font;
        tm.text = text;
        tm.fontSize = 64;
        tm.characterSize = charSize;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = font.material;
        mr.sortingOrder = order;
        return tm;
    }
}

public class FallingObstacle : MonoBehaviour
{
    Camera cam;
    float x, speed, warn = 1f, spin;
    SpriteRenderer sr;
    Collider2D col;
    Transform mark;

    public void Init(Camera c, float px, float fallSpeed)
    {
        cam = c; x = px; speed = fallSpeed;
        spin = Random.Range(-70f, 70f);
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        sr.enabled = false;
        col.enabled = false;
        mark = CodeText.New(transform.parent, "!", new Color(1f, 0.2f, 0.2f), 0.22f, 6).transform;
    }

    void Update()
    {
        float camY = cam.transform.position.y;
        float top = camY + cam.orthographicSize;

        if (warn > 0f)
        {
            warn -= Time.deltaTime;
            if (mark != null)
            {
                mark.position = new Vector3(x, top - 0.8f, 0f);
                mark.gameObject.SetActive(Mathf.Repeat(Time.time * 6f, 1f) < 0.6f);
            }
            if (warn <= 0f)
            {
                if (mark != null) Destroy(mark.gameObject);
                transform.position = new Vector3(x, top + 1f, 0f);
                sr.enabled = true;
                col.enabled = true;
            }
            return;
        }

        transform.position += Vector3.down * speed * Time.deltaTime;
        transform.Rotate(0f, 0f, spin * Time.deltaTime);
        if (transform.position.y < camY - cam.orthographicSize - 2f) Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (mark != null) Destroy(mark.gameObject);
    }
}

public class SideObstacle : MonoBehaviour
{
    static readonly string[] Words =
    {
        "0101", "1100", "01001", "{ }", "</>", "C#", "Java", "Python", "if(){}",
        "while(1)", "int x;", "SQL", "HTML", "JS", "404", "NULL", "git"
    };

    Camera cam;
    float dir, speed, yOff, width, warn = 1f;
    bool pushed;
    Transform mark;

    public static void Create(Transform root, Camera cam, float yOff, float dir, float speed)
    {
        string word = Words[Random.Range(0, Words.Length)];
        var go = new GameObject("Codigo_" + word);
        go.transform.SetParent(root, false);

        var tm = CodeText.New(go.transform, word, new Color(0.35f, 1f, 0.5f), 0.1f, 5);
        Bounds b = tm.GetComponent<MeshRenderer>().bounds;
        float w = Mathf.Max(0.9f, b.size.x + 0.5f);
        float h = Mathf.Max(0.9f, b.size.y + 0.25f);

        var border = new GameObject("Borde");
        border.transform.SetParent(go.transform, false);
        var bsr = border.AddComponent<SpriteRenderer>();
        bsr.sprite = DecorSprites.Block;
        bsr.color = new Color(0.25f, 0.9f, 0.45f);
        bsr.sortingOrder = 3;
        border.transform.localScale = new Vector3(w + 0.12f, h + 0.12f, 1f);

        var body = new GameObject("Bloque");
        body.transform.SetParent(go.transform, false);
        var sr = body.AddComponent<SpriteRenderer>();
        sr.sprite = DecorSprites.Block;
        sr.color = new Color(0.05f, 0.08f, 0.16f, 0.96f);
        sr.sortingOrder = 4;
        body.transform.localScale = new Vector3(w, h, 1f);

        var bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(w, h * 0.9f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        go.AddComponent<SideObstacle>().Init(cam, yOff, dir, speed, w);
    }

    void Init(Camera c, float y, float d, float s, float w)
    {
        cam = c; yOff = y; dir = d; speed = s; width = w;
        mark = CodeText.New(transform.parent, dir > 0f ? ">>>" : "<<<", new Color(1f, 0.2f, 0.2f), 0.2f, 6).transform;
        Place(0f);
    }

    float HalfW { get { return cam.orthographicSize * cam.aspect; } }

    void Place(float _)
    {
        float sx = cam.transform.position.x - dir * (HalfW + width * 0.5f + 0.6f);
        transform.position = new Vector3(sx, cam.transform.position.y + yOff, 0f);
    }

    void Update()
    {
        if (warn > 0f)
        {
            warn -= Time.deltaTime;
            float camY = cam.transform.position.y;
            if (mark != null)
            {
                mark.position = new Vector3(cam.transform.position.x - dir * (HalfW - 0.9f), camY + yOff, 0f);
                mark.gameObject.SetActive(Mathf.Repeat(Time.time * 6f, 1f) < 0.6f);
            }
            transform.position = new Vector3(transform.position.x, camY + yOff, 0f);
            if (warn <= 0f && mark != null) Destroy(mark.gameObject);
            return;
        }

        transform.position += Vector3.right * dir * speed * Time.deltaTime;
        if (Mathf.Abs(transform.position.x - cam.transform.position.x) > HalfW + width + 2f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (pushed || warn > 0f) return;
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;
        pushed = true;
        pc.Push(dir, 11f);
        AudioManager.Play("hit", 0.8f);
    }

    void OnDestroy()
    {
        if (mark != null) Destroy(mark.gameObject);
    }
}