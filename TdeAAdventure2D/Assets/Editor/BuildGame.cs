using System.IO;
using UnityEditor;
using UnityEngine;

// Prepara las imagenes (fondo y personaje) y arma la escena del juego.
// Para ejecutarlo: menu Tools > TDEA > Construir juego.
public static partial class BuildGame
{
    const string SpritesDir = "Assets/Sprites";
    const float PlayerHeightUnits = 1.6f;

    // Zona util del fondo (sin el borde blanco de la imagen), en pixeles
    const float BgCropX = 28f, BgCropY = 18f, BgCropW = 976f, BgCropH = 566f;
    const float BgHeightUnits = 10f;
    const float GroundFraction = 0.606f; // posicion del borde verde medida desde arriba

    [MenuItem("Tools/TDEA/Construir juego")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(SpritesDir))
            AssetDatabase.CreateFolder("Assets", "Sprites");

        string fondoPath = MoveTo(FindTexture("fondo"), SpritesDir + "/fondo.jpeg");
        string personajePath = MoveTo(FindTexture("personaje"), SpritesDir + "/personaje.jpeg");
        if (fondoPath == null || personajePath == null)
        {
            Debug.LogError("[TDEA] No encontre las imagenes fondo y personaje en el proyecto.");
            return;
        }

        AssetDatabase.DeleteAsset(SpritesDir + "/pilar.png"); // ya no se usa

        Sprite bg = BuildBackground(fondoPath);
        Sprite[] frames = BuildCharacter(personajePath);
        Sprite platform = BuildPlatformSprite();
        Sprite brick = BuildBrickTile();

        if (bg == null || frames == null || frames.Length < 6 || platform == null || brick == null)
        {
            Debug.LogError("[TDEA] Faltan sprites, no se pudo armar la escena.");
            return;
        }

        BuildScene(frames, bg, platform, brick);

        AssetDatabase.SaveAssets();
        Debug.Log("[TDEA] Juego construido. Dale a Play.");
    }

    // ---------- Utilidades ----------

    static PhysicsMaterial2D FrictionlessMaterial()
    {
        string path = SpritesDir + "/SinFriccion.asset";
        var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (m == null)
        {
            m = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    static GameObject GetOrCreate(string name)
    {
        var g = GameObject.Find(name);
        if (g == null) g = new GameObject(name);
        return g;
    }

    static T Ensure<T>(GameObject g) where T : Component
    {
        var c = g.GetComponent<T>();
        if (c == null) c = g.AddComponent<T>();
        return c;
    }

    static string FindTexture(string name)
    {
        foreach (string guid in AssetDatabase.FindAssets(name + " t:Texture2D"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            string ext = Path.GetExtension(p).ToLower();
            if (Path.GetFileNameWithoutExtension(p) == name && (ext == ".jpeg" || ext == ".jpg"))
                return p;
        }
        return null;
    }

    static string MoveTo(string from, string to)
    {
        if (from == null) return null;
        if (from == to) return to;
        string err = AssetDatabase.MoveAsset(from, to);
        if (!string.IsNullOrEmpty(err))
        {
            Debug.LogWarning("[TDEA] " + err);
            return from;
        }
        return to;
    }
}
