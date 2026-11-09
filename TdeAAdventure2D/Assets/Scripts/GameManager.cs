using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Puntos = metros de altura + monedas/gemas. Al perder, la partida se guarda a nombre del jugador
// (ver ScoreBoard.cs). El marcador esta en GameManagerHUD.cs y el menu de inicio en MainMenu.cs.
public partial class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform player;
    public Camera cam;
    public LevelGenerator generator;

    public float Height { get; private set; }
    public int Bonus { get; private set; }
    public int Coins { get; private set; }
    public bool Dead { get; private set; }
    public bool InMenu { get; private set; }
    public int Score { get { return Mathf.FloorToInt(Height) + Bonus; } }
    public int Level { get { return Difficulty.Level(Height); } }

    // Monedero del jugador actual: para la tienda de skins y objetos
    public static int Wallet { get { return ScoreBoard.Wallet; } }
    public static bool Spend(int amount) { return ScoreBoard.Spend(amount); }

    Vector3 startPos, camStartPos;
    float startY, hintTimer = 8f, popTimer, bannerTimer;
    int lastLevel = 1, rank;
    bool newBest;
    string popText = "", deathReason = "";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        startPos = player.position;
        camStartPos = cam.transform.position;
        startY = startPos.y;
        EnterMenu();
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    // Vuelve al menu: el juego queda en pausa detras
    public void EnterMenu()
    {
        InMenu = true;
        Dead = false;
        Time.timeScale = 0f;
        SetPlayerActive(false);
    }

    // Empieza una partida con el nombre escrito en el menu
    public void StartRun(string playerName)
    {
        ScoreBoard.SetCurrent(playerName);
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
        lastLevel = 1;
        popTimer = 0f;
        bannerTimer = 0f;
        hintTimer = 5f;
        Dead = false;
    }

    void Update()
    {
        if (player == null || cam == null || InMenu) return;

        if (!Dead)
        {
            Height = Mathf.Max(Height, player.position.y - startY);
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
        Bonus += pts;
        Coins++;
        popText = "+" + pts;
        popTimer = 0.8f;
    }

    public void Kill(string reason)
    {
        if (Dead || InMenu) return;
        Dead = true;
        deathReason = reason;
        rank = ScoreBoard.Submit(Score, out newBest);
        SetPlayerActive(false);
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
