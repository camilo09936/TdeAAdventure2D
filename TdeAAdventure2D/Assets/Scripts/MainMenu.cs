using System.Collections.Generic;
using UnityEngine;

// Menu de inicio: logo, fondo, nombre del jugador y ranking.
// Las imagenes se leen de Assets/Resources/Menu: cualquier imagen con "logo" en el nombre
// es el logo, y la que tenga "fondo" es el fondo. Si faltan, el menu funciona igual.
// Partes: MainMenu.cs (inicio), MainMenuRanking.cs (ranking), MainMenuStyles.cs (estilos).
public partial class MainMenu : MonoBehaviour
{
    enum Page { Home, Ranking }

    static readonly Color Lime = new Color(0.60f, 0.78f, 0.12f);
    static readonly Color Gold = new Color(1f, 0.85f, 0.25f);
    static readonly Color Silver = new Color(0.82f, 0.86f, 0.9f);
    static readonly Color Bronze = new Color(0.85f, 0.55f, 0.30f);
    static readonly Color Mine = new Color(0.55f, 0.95f, 1f);
    static readonly Color DarkText = new Color(0.02f, 0.18f, 0.10f);

    Texture2D bg, logo;
    string nameInput = "", message = "";
    Page page = Page.Home;
    bool wasInMenu, focusDone;
    int tab;
    Vector2 scroll;
    List<PlayerRecord> bestList = new List<PlayerRecord>();
    List<PlayerRecord> totalList = new List<PlayerRecord>();

    // Se agrega solo al GameManager al empezar (no hay que arrastrarlo a mano)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoAttach()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.GetComponent<MainMenu>() == null)
            gm.gameObject.AddComponent<MainMenu>();
    }

    // Busca en Resources/Menu una imagen cuyo nombre contenga la palabra (sin importar mayusculas)
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
        nameInput = ScoreBoard.LastName;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        bool inMenu = gm != null && gm.InMenu;
        if (inMenu && !wasInMenu)
        {
            Refresh();
            page = Page.Home;
            message = "";
            focusDone = false;
        }
        wasInMenu = inMenu;
    }

    void Refresh()
    {
        bestList = ScoreBoard.ByBest();
        totalList = ScoreBoard.ByTotal();
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
        else DrawRanking(w, h, s);
    }

    void DrawBackground(float w, float h)
    {
        if (bg != null)
        {
            // La imagen es alta: se ve un tramo y la vista sube despacio por el edificio y vuelve a bajar
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
            TryStart();
            e.Use();
            return;
        }

        float top = 24f * s;
        if (logo != null)
        {
            // El logo se ajusta para que quepa siempre: maximo 72% del ancho y 38% del alto
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
        GUI.DrawTexture(new Rect(px, top, pw, 250f * s), tPanel);
        Label(new Rect(px, top + 14f * s, pw, 34f * s), "Escribe tu nombre para jugar", 24, Color.white);

        GUI.SetNextControlName("nombre");
        sField.fontSize = Mathf.RoundToInt(30f * s);
        nameInput = GUI.TextField(new Rect(px + pad, top + 56f * s, pw - 2f * pad, 52f * s), nameInput, ScoreBoard.MaxName, sField);
        if (!focusDone)
        {
            GUI.FocusControl("nombre");
            focusDone = true;
        }

        if (message.Length > 0)
            Label(new Rect(px, top + 112f * s, pw, 28f * s), message, 20, new Color(1f, 0.55f, 0.5f));

        sPrimary.fontSize = Mathf.RoundToInt(30f * s);
        if (GUI.Button(new Rect(px + pad, top + 142f * s, pw - 2f * pad, 52f * s), "JUGAR", sPrimary))
            TryStart();

        sSecondary.fontSize = Mathf.RoundToInt(22f * s);
        if (GUI.Button(new Rect(px + pad, top + 202f * s, pw - 2f * pad, 36f * s), "RANKING", sSecondary))
        {
            Refresh();
            scroll = Vector2.zero;
            page = Page.Ranking;
        }

        if (bestList.Count > 0)
            Label(new Rect(0f, h - 92f * s, w, 30f * s), "Mejor puntaje: " + bestList[0].name + " - " + bestList[0].best, 22, Gold);
        Label(new Rect(0f, h - 56f * s, w, 30f * s), "A / D: mover      Espacio: saltar", 20, Color.white);
    }

    void TryStart()
    {
        string n = ScoreBoard.Clean(nameInput);
        if (n.Length == 0)
        {
            message = "Escribe un nombre para empezar";
            return;
        }
        message = "";
        GUIUtility.keyboardControl = 0;
        GameManager.Instance.StartRun(n);
    }
}
