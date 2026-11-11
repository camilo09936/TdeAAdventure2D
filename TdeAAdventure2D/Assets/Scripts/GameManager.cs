using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
public partial class GameManager : MonoBehaviour
{
    public const string EscenaLogin = "Login";
    public static GameManager Instance;

    public Transform player;
    public Camera cam;
    public LevelGenerator generator;

    public float Height { get; private set; }
    public int Bonus { get; private set; }
    public int Coins { get; private set; }
    public float RunTime { get; private set; }
    public bool Dead { get; private set; }
    public bool InMenu { get; private set; }
    public int Score { get { return Mathf.FloorToInt(Height); } }
    public int Level { get { return Difficulty.Level(Height); } }

    public static int Wallet { get { return SessionManager.EstaAutenticado ? SessionManager.JugadorActual.monedas : 0; } }

    Vector3 startPos, camStartPos;
    float startY, hintTimer = 8f, popTimer, bannerTimer;
    int lastLevel = 1;
    bool newBest, guardando;
    string popText = "", deathReason = "", errorGuardando = "";
    //public static bool Spend(int amount) { return ScoreBoard.Spend(amount); }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!SessionManager.EstaAutenticado)
        {
            InMenu = true;
            SceneManager.LoadScene(EscenaLogin);
            return;
        }

        startPos = player.position;
        camStartPos = cam.transform.position;
        startY = startPos.y;

        if (GetComponent<MainMenu>() == null) gameObject.AddComponent<MainMenu>();
        if (GetComponent<SkyAmbience>() == null) gameObject.AddComponent<SkyAmbience>();
        if (player.GetComponent<PlayerVisual>() == null) player.gameObject.AddComponent<PlayerVisual>();
        StartCoroutine(SkinLoader.Aplicar(player.GetComponent<PlayerVisual>()));
        EnterMenu();
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void EnterMenu()
    {
        InMenu = true;
        Dead = false;
        Time.timeScale = 0f;
        SetPlayerActive(false);
    }

    public void StartRun()
    {
        InMenu = false;
        Time.timeScale = 1f;
        Restart();
    }

    void SetPlayerActive(bool on)
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = on;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = on;
    }

    void Restart()
    {
        SetPlayerActive(true);

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            RbCompat.SetVel(rb, Vector2.zero);
            rb.position = startPos;
        }
        player.position = startPos;

        cam.transform.position = camStartPos;
        generator.ResetLevel();

        Height = 0f;
        Bonus = 0;
        Coins = 0;
        RunTime = 0f;
        lastLevel = 1;
        popTimer = 0f;
        bannerTimer = 0f;
        hintTimer = 5f;
        newBest = false;
        guardando = false;
        errorGuardando = "";
        Dead = false;
    }

    void Update()
    {
        if (player == null || cam == null || InMenu) return;

        if (!Dead)
        {
            Height = Mathf.Max(Height, player.position.y - startY);
            RunTime += Time.deltaTime;
            hintTimer -= Time.deltaTime;
            popTimer -= Time.deltaTime;
            bannerTimer -= Time.deltaTime;

            int lvl = Level;
            if (lvl > lastLevel) { lastLevel = lvl; bannerTimer = 2.5f; }

            if (player.position.y < cam.transform.position.y - cam.orthographicSize - 1f)
                Kill("Te quedaste atras");
        }
        else if (KeyPressed(KeyCode.R) || KeyPressed(KeyCode.Return))
        {
            Restart();
        }
        else if (KeyPressed(KeyCode.M) || KeyPressed(KeyCode.Escape))
        {
            EnterMenu();
        }
    }

    public void AddPoints(int pts)
    {
        if (Dead || InMenu) return;
        Coins++;
        popText = pts >= 25 ? "+1 gema" : "+1 moneda";
        popTimer = 0.8f;
    }

    public void Kill(string reason)
    {
        if (Dead || InMenu) return;
        Dead = true;
        deathReason = reason;
        SetPlayerActive(false);
        StartCoroutine(GuardarPartida());
    }

    IEnumerator GuardarPartida()
    {
        var jugador = SessionManager.JugadorActual;
        if (jugador == null) yield break;

        var cf = cam.GetComponent<CameraFollow>();
        float velFinal = cf != null ? cf.CurrentSpeed : 0f;

        newBest = Score > jugador.puntuacionMaxima;
        guardando = true;
        errorGuardando = "";
        bool guardada = false;

        var cuerpo = new PartidaRequest
        {
            jugadorId = jugador.id,
            puntuacionObtenida = Score,
            monedasRecolectadas = Coins,
            tiempoSupervivencia = Redondear(RunTime, 9999.99f),
            velocidadFinalPantalla = Redondear(velFinal, 9999.99f)
        };

        yield return ApiClient.Post<PartidaRequest, Vacio>("api/Partida", cuerpo,
            _ => guardada = true,
            (codigo, msg) => errorGuardando = "No se pudo guardar la partida (" + (codigo == 0 ? "sin conexion" : "error " + codigo) + ").");

        if (guardada)
        {
            yield return ApiClient.Get<JugadorDto>("api/Jugador/" + jugador.id,
                j => SessionManager.IniciarSesion(j),
                (codigo, msg) => { });
        }
        guardando = false;
    }

    static float Redondear(float v, float max)
    {
        return Mathf.Min(Mathf.Round(Mathf.Max(0f, v) * 100f) / 100f, max);
    }

    static bool KeyPressed(KeyCode k)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return false;
        switch (k)
        {
            case KeyCode.R: return kb.rKey.wasPressedThisFrame;
            case KeyCode.M: return kb.mKey.wasPressedThisFrame;
            case KeyCode.Escape: return kb.escapeKey.wasPressedThisFrame;
            case KeyCode.Return: return kb.enterKey.wasPressedThisFrame;
        }
        return false;
#else
        return Input.GetKeyDown(k);
#endif
    }
}
