using UnityEngine;
using UnityEngine.SceneManagement;

// Menu de inicio: logo, fondo, datos del jugador logueado, ranking y tienda.
// Partes: MainMenu.cs (inicio), MainMenuRanking.cs, MainMenuShop.cs, MainMenuStyles.cs (estilos).
public partial class MainMenu : MonoBehaviour
{
    enum Page { Home, Ranking, Shop }

    static readonly Color Lime = new Color(0.60f, 0.78f, 0.12f);
    static readonly Color Gold = new Color(1f, 0.85f, 0.25f);
    static readonly Color Silver = new Color(0.82f, 0.86f, 0.9f);
    static readonly Color Bronze = new Color(0.85f, 0.55f, 0.30f);
    static readonly Color Mine = new Color(0.55f, 0.95f, 1f);
    static readonly Color DarkText = new Color(0.02f, 0.18f, 0.10f);

    Texture2D bg, logo;
    Page page = Page.Home;
    bool wasInMenu;
    Vector2 scroll;

    static Texture2D FindImage(string word)
    {
        Texture2D[] all = Resources.LoadAll<Texture2D>("Menu");
        foreach (Texture2D t in all)
            if (t != null && t.name.ToLowerInvariant().Contains(word)) return t;
        return null;
    }

    void Awake()
    {
        bg = FindImage("fondo");
        logo = FindImage("logo");
        if (bg != null) bg.wrapMode = TextureWrapMode.Clamp;
        if (logo != null) logo.wrapMode = TextureWrapMode.Clamp;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        bool inMenu = gm != null && gm.InMenu;
        if (inMenu && !wasInMenu) page = Page.Home;
        wasInMenu = inMenu;
    }

    void OnGUI()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || !gm.InMenu) return;

        EnsureStyles();
        GUI.skin.settings.cursorColor = Color.white;

        float w = Screen.width, h = Screen.height, s = h / 720f;
        DrawBackground(w, h);
        if (page == Page.Home) DrawHome(w, h, s);
        else if (page == Page.Ranking) DrawRanking(w, h, s);
        else DrawShop(w, h, s);
    }

    void DrawBackground(float w, float h)
    {
        if (bg != null)
        {
            float imgAspect = (float)bg.width / bg.height;
            float scrAspect = w / h;
            float visW = Mathf.Min(1f, scrAspect / imgAspect);
            float visH = Mathf.Min(1f, imgAspect / scrAspect);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.unscaledTime * 0.04f, 1f));
            Rect uv = new Rect((1f - visW) * 0.5f, (1f - visH) * t, visW, visH);
            GUI.DrawTextureWithTexCoords(new Rect(0f, 0f, w, h), bg, uv);
        }
        else
        {
            GUI.DrawTexture(new Rect(0f, 0f, w, h), tBase);
        }
        GUI.DrawTexture(new Rect(0f, 0f, w, h), tDim);
    }

    void DrawHome(float w, float h, float s)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            GameManager.Instance.StartRun();
            e.Use();
            return;
        }

        float top = 24f * s;
        if (logo != null)
        {
            float lw = Mathf.Min(w * 0.72f, 900f * s);
            float lh = lw * logo.height / logo.width;
            float maxH = h * 0.38f;
            if (lh > maxH) { lw *= maxH / lh; lh = maxH; }
            GUI.DrawTexture(new Rect((w - lw) * 0.5f, top, lw, lh), logo, ScaleMode.ScaleToFit, true);
            top += lh + 10f * s;
        }
        else
        {
            Label(new Rect(0f, top, w, 90f * s), "T de A AVENTURE", 64, Color.white);
            top += 100f * s;
        }

        float pw = Mathf.Min(w * 0.9f, 520f * s);
        float px = (w - pw) * 0.5f;
        float pad = 24f * s;
        GUI.DrawTexture(new Rect(px, top, pw, 300f * s), tPanel);

        JugadorDto j = SessionManager.JugadorActual;
        Label(new Rect(px, top + 10f * s, pw, 36f * s), "Jugador: " + (j != null ? j.nombreUsuario : ""), 26, Color.white);
        Label(new Rect(px, top + 46f * s, pw, 30f * s),
            "Monedas: " + (j != null ? j.monedas : 0) + "     Mejor: " + (j != null ? j.puntuacionMaxima : 0), 22, Gold);

        sPrimary.fontSize = Mathf.RoundToInt(30f * s);
        if (GUI.Button(new Rect(px + pad, top + 86f * s, pw - 2f * pad, 52f * s), "JUGAR", sPrimary))
            GameManager.Instance.StartRun();

        sSecondary.fontSize = Mathf.RoundToInt(22f * s);
        if (GUI.Button(new Rect(px + pad, top + 148f * s, pw - 2f * pad, 38f * s), "RANKING", sSecondary)) AbrirRanking();
        if (GUI.Button(new Rect(px + pad, top + 194f * s, pw - 2f * pad, 38f * s), "TIENDA", sSecondary)) AbrirTienda();
        if (GUI.Button(new Rect(px + pad, top + 240f * s, pw - 2f * pad, 38f * s), "CERRAR SESIÓN", sSecondary)) CerrarSesion();

        Label(new Rect(0f, h - 56f * s, w, 30f * s), "A / D: mover      Espacio: saltar", 20, Color.white);
    }

    void CerrarSesion()
    {
        SessionManager.CerrarSesion();
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameManager.EscenaLogin);
    }
}