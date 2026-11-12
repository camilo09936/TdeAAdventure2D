using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public float musicVolume = 0.35f;
    public float sfxVolume = 0.8f;
    public bool botonesAlaIzquierda = false;  

    AudioSource music, sfx;
    readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    string currentMusic = "";
    bool musicOn = true, sfxOn = true;
    PlayerController player;
    int lastLevel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Instance != null) return;
        var go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<AudioManager>();
    }

    void Awake()
    {
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        musicOn = PlayerPrefs.GetInt("musicOn", 1) == 1;
        sfxOn = PlayerPrefs.GetInt("sfxOn", 1) == 1;
    }

    void Save()
    {
        PlayerPrefs.SetInt("musicOn", musicOn ? 1 : 0);
        PlayerPrefs.SetInt("sfxOn", sfxOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    AudioClip Get(string folder, string name)
    {
        string key = folder + "/" + name;
        AudioClip c;
        if (!cache.TryGetValue(key, out c))
        {
            c = Resources.Load<AudioClip>("Audio/" + key);
            cache[key] = c;
        }
        return c;
    }

    public static void Play(string name, float volume = 1f)
    {
        if (Instance == null || !Instance.sfxOn) return;
        AudioClip c = Instance.Get("Sfx", name);
        if (c != null) Instance.sfx.PlayOneShot(c, Instance.sfxVolume * volume);
    }

    void PlayMusic(string name)
    {
        if (name == currentMusic) return;
        currentMusic = name;
        AudioClip c = Get("Music", name);
        music.Stop();
        music.clip = c;
        if (c != null) music.Play();  
    }

    void OnJump() { Play("jump", 0.7f); }
    void OnLand(float speed) { if (speed > 4f) Play("land", Mathf.Clamp01(speed / 15f)); }

    void Update()
    {
        music.mute = !musicOn;
        music.volume = musicVolume;

        GameManager gm = GameManager.Instance;
        bool enLogin = SceneManager.GetActiveScene().name == "Login";
        bool enMenu = enLogin || gm == null || gm.InMenu;

        PlayMusic(enMenu ? "menu" : "game");

        if (enMenu && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Play("click", 0.6f);

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
            if (player != null) { player.Jumped += OnJump; player.Landed += OnLand; }
        }

        if (gm != null && !enMenu)
        {
            int lv = Mathf.FloorToInt(Mathf.Max(0f, gm.Height) / Difficulty.MetersPerLevel);
            if (lv > lastLevel) Play("level");
            lastLevel = lv;
        }
        else lastLevel = 0;
    }

    void OnGUI()
    {
        GUI.depth = -1000;
        float s = Mathf.Max(0.8f, Screen.height / 720f);
        float w = 96f * s, h = 26f * s, gap = 4f * s, margin = 8f * s;
        float total = 3f * w + 2f * gap;
        float x = botonesAlaIzquierda ? margin : Screen.width - total - margin;
        float y = margin;

        var st = new GUIStyle(GUI.skin.button);
        st.fontSize = Mathf.RoundToInt(13f * s);
        st.fontStyle = FontStyle.Bold;

        Color old = GUI.color;

        GUI.color = musicOn ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.6f, 0.6f);
        if (GUI.Button(new Rect(x, y, w, h), "Música: " + (musicOn ? "ON" : "OFF"), st))
        {
            musicOn = !musicOn;
            Save();
        }

        GUI.color = sfxOn ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.6f, 0.6f);
        if (GUI.Button(new Rect(x + w + gap, y, w, h), "Efectos: " + (sfxOn ? "ON" : "OFF"), st))
        {
            sfxOn = !sfxOn;
            Save();
        }

        bool todo = musicOn || sfxOn;
        GUI.color = todo ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.6f, 0.6f);
        if (GUI.Button(new Rect(x + 2f * (w + gap), y, w, h), "Todo: " + (todo ? "ON" : "OFF"), st))
        {
            bool nuevo = !todo;  
            musicOn = nuevo;
            sfxOn = nuevo;
            Save();
        }

        GUI.color = old;
    }
}