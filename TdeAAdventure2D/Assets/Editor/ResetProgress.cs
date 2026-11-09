using UnityEditor;
using UnityEngine;

// Pone en cero el monedero y el mejor puntaje guardados (util para probar la tienda).
public static class ResetProgress
{
    [MenuItem("Tools/TDEA/Reiniciar monedero y mejor puntaje")]
    public static void Reset()
    {
        PlayerPrefs.DeleteKey("TDEA_WALLET");
        PlayerPrefs.DeleteKey("TDEA_BEST_SCORE");
        PlayerPrefs.Save();
        Debug.Log("[TDEA] Monedero y mejor puntaje en cero.");
    }
}
