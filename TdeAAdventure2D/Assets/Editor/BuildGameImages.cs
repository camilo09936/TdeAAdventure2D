using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class BuildGame
{
    static Sprite BuildBackground(string path)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = BgCropH / BgHeightUnits;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
#pragma warning disable 618
        imp.spritesheet = new[]
        {
            new SpriteMetaData
            {
                name = "fondo_juego",
                rect = new Rect(BgCropX, BgCropY, BgCropW, BgCropH),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            }
        };
#pragma warning restore 618
        imp.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    }

    static Sprite[] BuildCharacter(string originalPath)
    {
        string pngPath = SpritesDir + "/personaje_sheet.png";

        // Leer el JPG (Unity lo carga SIN canal alfa)
        var src = new Texture2D(2, 2);
        src.LoadImage(File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), originalPath)));
        int w = src.width, h = src.height;
        Color32[] px = src.GetPixels32();
        Object.DestroyImmediate(src);

        // Volver transparente el fondo blanco que toca los bordes
        var isBg = new bool[px.Length];
        var stack = new Stack<int>();

        void Push(int x, int y)
        {
            int i = y * w + x;
            if (isBg[i]) return;
            if (px[i].r >= 205 && px[i].g >= 205 && px[i].b >= 205)
            {
                isBg[i] = true;
                stack.Push(i);
            }
        }

        for (int x = 0; x < w; x++) { Push(x, 0); Push(x, h - 1); }
        for (int y = 0; y < h; y++) { Push(0, y); Push(w - 1, y); }
        while (stack.Count > 0)
        {
            int i = stack.Pop();
            int x = i % w, y = i / w;
            if (x > 0) Push(x - 1, y);
            if (x < w - 1) Push(x + 1, y);
            if (y > 0) Push(x, y - 1);
            if (y < h - 1) Push(x, y + 1);
        }
        int clear = 0;
        for (int i = 0; i < px.Length; i++)
        {
            if (isBg[i]) { px[i] = new Color32(0, 0, 0, 0); clear++; }
        }

        // Textura nueva RGBA32 para que el alfa se conserve en el PNG
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(px);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), pngPath), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport);

        // Cortar en 6 cuadros iguales
        const int frameCount = 6;
        int fw = w / frameCount;
        var imp = (TextureImporter)AssetImporter.GetAtPath(pngPath);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = h / PlayerHeightUnits;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
#pragma warning disable 618
        var metas = new SpriteMetaData[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            metas[i] = new SpriteMetaData
            {
                name = "personaje_" + i,
                rect = new Rect(i * fw, 0, fw, h),
                alignment = (int)SpriteAlignment.Custom,
                pivot = new Vector2(0.5f, 0f) // los pies quedan en el origen
            };
        }
        imp.spritesheet = metas;
#pragma warning restore 618
        imp.SaveAndReimport();

        Debug.Log("[TDEA] personaje: " + clear + " de " + px.Length + " pixeles transparentes, alfa en PNG: " + imp.DoesSourceTextureHaveAlpha());
        return AssetDatabase.LoadAllAssetsAtPath(pngPath).OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }
}
