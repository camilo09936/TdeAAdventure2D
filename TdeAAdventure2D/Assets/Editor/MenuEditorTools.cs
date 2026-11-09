using UnityEditor;
using UnityEngine;

// Ajusta solas las imagenes que pongas en Assets/Resources/Menu (logo y fondo):
// sin compresion, sin redimensionar a potencias de 2 y con filtro suave.
public class MenuTexturePostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Replace("\\", "/").Contains("/Resources/Menu/")) return;

        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Default;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Bilinear;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.alphaIsTransparency = true;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.maxTextureSize = 4096;
    }
}

public static class MenuEditorTools
{
    [MenuItem("Tools/TDEA/Borrar jugadores y puntajes")]
    public static void ClearScores()
    {
        ScoreBoard.ClearAll();
        Debug.Log("[TDEA] Jugadores y puntajes borrados.");
    }
}
