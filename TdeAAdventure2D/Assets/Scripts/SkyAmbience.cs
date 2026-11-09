using System.Collections.Generic;
using UnityEngine;

// Ambiente segun la altura:
//   Nivel 2 (50 m)   -> los ladrillos se vuelven cielo azul y aparecen nubes que flotan.
//   Nivel 3 (100 m)  -> empieza a anochecer: azul -> atardecer -> noche con estrellas y luna.
// Cambia los numeros de abajo para mover estos momentos.
public class SkyAmbience : MonoBehaviour
{
    public float cloudStart = 50f;    // empieza el cielo y las nubes (nivel 2)
    public float cloudFull = 75f;     // cielo completo
    public float nightStart = 100f;   // empieza a anochecer (nivel 3)
    public float nightFull = 220f;    // noche completa

    // Orden de dibujo: el fondo de ladrillos es -20/-10, plataformas y personaje van arriba
    public int skyOrder = -9;
    public int cloudOrder = -8;

    static readonly Color DaySky = new Color(0.42f, 0.72f, 1f);
    static readonly Color DuskSky = new Color(0.80f, 0.42f, 0.50f);
    static readonly Color NightSky = new Color(0.03f, 0.05f, 0.17f);
    static readonly Color DayCloud = Color.white;
    static readonly Color DuskCloud = new Color(1f, 0.72f, 0.66f);
    static readonly Color NightCloud = new Color(0.36f, 0.42f, 0.66f);

    class Item
    {
        public Transform t;
        public SpriteRenderer sr;
        public float u, v, speed, par, baseA, phase;
    }

    Camera cam;
    GameManager gm;
    SpriteRenderer sky, moon;
    readonly List<Item> clouds = new List<Item>();
    readonly List<Item> stars = new List<Item>();
    float dayT, nightT;

    // Se agrega solo al GameManager al empezar (no hay que arrastrarlo a mano)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoAttach()
    {
        GameManager g = GameManager.Instance;
        if (g != null && g.GetComponent<SkyAmbience>() == null)
            g.gameObject.AddComponent<SkyAmbience>();
    }

    SpriteRenderer NewSprite(string n, Sprite s, Transform parent, int order)
    {
        var go = new GameObject(n);
        go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.sortingOrder = order;
        return sr;
    }

    void Start()
    {
        gm = GameManager.Instance;
        cam = gm != null && gm.cam != null ? gm.cam : Camera.main;
        if (cam == null) { enabled = false; return; }

        var root = new GameObject("Cielo").transform;

        // Cielo y estrellas viajan pegados a la camara
        sky = NewSprite("CieloFondo", SkyArt.Gradient(), cam.transform, skyOrder);
        sky.transform.localPosition = new Vector3(0f, 0f, 10f);

        Sprite dot = SkyArt.Dot();
        moon = NewSprite("Luna", dot, cam.transform, cloudOrder);
        moon.transform.localPosition = new Vector3(0f, 0f, 10f);
        moon.color = new Color(1f, 0.96f, 0.8f, 0f);

        var rnd = new System.Random(7);
        for (int i = 0; i < 50; i++)
        {
            var it = new Item();
            it.sr = NewSprite("Estrella", dot, cam.transform, cloudOrder);
            it.t = it.sr.transform;
            it.u = (float)rnd.NextDouble() - 0.5f;
            it.v = (float)rnd.NextDouble() - 0.5f;
            it.baseA = 0.5f + (float)rnd.NextDouble() * 0.5f;
            it.phase = (float)rnd.NextDouble() * 6.28f;
            it.speed = 1f + (float)rnd.NextDouble() * 2f;
            float sz = 0.08f + (float)rnd.NextDouble() * 0.14f;
            it.t.localScale = new Vector3(sz, sz, 1f);
            it.sr.color = new Color(1f, 1f, 1f, 0f);
            stars.Add(it);
        }

        Sprite[] shapes = { SkyArt.Cloud(1), SkyArt.Cloud(2), SkyArt.Cloud(3) };
        for (int i = 0; i < 14; i++)
        {
            var it = new Item();
            it.sr = NewSprite("Nube", shapes[i % 3], root, cloudOrder);
            it.t = it.sr.transform;
            it.u = (float)rnd.NextDouble();
            it.v = (float)rnd.NextDouble();
            it.par = 0.15f + (float)rnd.NextDouble() * 0.4f;      // las lejanas se mueven menos
            it.speed = 0.4f + it.par * 1.2f;                       // y van mas despacio de lado
            it.baseA = 0.85f + it.par * 0.3f;
            float sc = 1.1f + it.par * 2f;
            it.t.localScale = new Vector3(sc, sc, 1f);
            it.sr.color = new Color(1f, 1f, 1f, 0f);
            clouds.Add(it);
        }
    }

    static float Ramp(float h, float a, float b)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(a, b, h));
    }

    // dia -> atardecer -> noche
    static Color ThreeStage(Color day, Color dusk, Color night, float t)
    {
        return t < 0.5f ? Color.Lerp(day, dusk, t * 2f) : Color.Lerp(dusk, night, (t - 0.5f) * 2f);
    }

    void Update()
    {
        if (cam == null) return;
        float h = (gm == null || gm.InMenu) ? 0f : gm.Height;
        float rate = Time.unscaledDeltaTime * 0.8f;
        dayT = Mathf.MoveTowards(dayT, Ramp(h, cloudStart, cloudFull), rate);
        nightT = Mathf.MoveTowards(nightT, Mathf.Clamp01(Mathf.InverseLerp(nightStart, nightFull, h)), rate);
    }

    void LateUpdate()
    {
        if (cam == null) return;
        float viewH = 2f * cam.orthographicSize;
        float viewW = viewH * cam.aspect;

        // Cielo: cubre toda la pantalla; azul de dia, atardecer, y azul muy oscuro de noche
        sky.transform.localScale = new Vector3(viewW * 1.3f / 0.03125f, viewH * 1.3f, 1f);
        Color c = ThreeStage(DaySky, DuskSky, NightSky, nightT);
        c.a = Mathf.Lerp(0.88f, 0.95f, nightT) * dayT;
        sky.color = c;

        // Nubes: flotan de lado y suben con parallax
        float spanX = viewW + 10f, spanY = viewH + 6f;
        Vector3 cp = cam.transform.position;
        Color cc = ThreeStage(DayCloud, DuskCloud, NightCloud, nightT);
        foreach (Item it in clouds)
        {
            it.u += it.speed * Time.deltaTime / spanX;
            it.u -= Mathf.Floor(it.u);
            float yRaw = it.v * spanY - cp.y * it.par;
            float y = yRaw - Mathf.Floor(yRaw / spanY) * spanY;
            it.t.position = new Vector3(cp.x + it.u * spanX - spanX * 0.5f, cp.y + y - spanY * 0.5f, 0f);
            cc.a = Mathf.Clamp01(dayT * it.baseA * Mathf.Lerp(1f, 0.65f, nightT));
            it.sr.color = cc;
        }

        // Estrellas y luna: salen cuando ya esta bastante oscuro
        float starT = Mathf.Clamp01((nightT - 0.4f) * 2f);
        float tw = Time.unscaledTime;
        foreach (Item it in stars)
        {
            it.t.localPosition = new Vector3(it.u * viewW, it.v * viewH, 10f);
            float a = starT * it.baseA * (0.65f + 0.35f * Mathf.Sin(tw * it.speed + it.phase));
            it.sr.color = new Color(1f, 1f, 1f, a);
        }
        moon.transform.localPosition = new Vector3(viewW * 0.3f, viewH * 0.33f, 10f);
        moon.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
        moon.color = new Color(1f, 0.96f, 0.8f, starT * 0.95f);
    }
}
