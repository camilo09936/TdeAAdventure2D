using UnityEditor;
using UnityEngine;

public class MenuTexturePostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Replace("\\", "/").Contains("/Resources/Menu/")) return;

        var ti = (TextureImporter)assetImporter;
        string fileName = System.IO.Path.GetFileName(assetPath);
        if (fileName == "fondo_login.jpg" || fileName == "Logo_Login.png")
        {
            ti.textureType = TextureImporterType.Sprite;
        }
        else
        {
            ti.textureType = TextureImporterType.Default;
        }
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
