using System.IO;
using UnityEditor;
using UnityEngine;

public static partial class BuildGame
{
    static Sprite BuildPlatformSprite()
    {
        string path = SpritesDir + "/plataforma.png";
        var tex = new Texture2D(16, 8, TextureFormat.RGBA32, false);
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                Color32 c = new Color32(226, 229, 233, 255);
                if (y == 7) c = new Color32(255, 255, 255, 255);
                else if (y == 6) c = new Color32(240, 243, 246, 255);
                else if (y == 1) c = new Color32(186, 191, 200, 255);
                else if (y == 0) c = new Color32(140, 146, 158, 255);
                if (x == 0 || x == 15) c = Shade(c, -35);
                if ((x == 1 || x == 14) && y == 4) c = new Color32(150, 156, 168, 255); 
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), path), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = 16f;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.spriteBorder = new Vector4(3, 2, 3, 2);
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite BuildBrickTile()
    {
        const int tw = 128, th = 112, bh = 28, bw = 64;
        string path = SpritesDir + "/ladrillos.png";

        var mortar = new Color32(117, 98, 71, 255);
        var tones = new[]
        {
            new Color32(185, 170, 129, 255),
            new Color32(199, 181, 146, 255),
            new Color32(176, 160, 120, 255),
            new Color32(190, 174, 136, 255)
        };

        var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false);
        for (int y = 0; y < th; y++)
        {
            int row = y / bh;
            int ry = y % bh;
            int off = (row % 2) * (bw / 2);
            for (int x = 0; x < tw; x++)
            {
                int xx = (x + off) % tw;
                int bi = xx / bw;
                int rx = xx % bw;

                Color32 c;
                if (ry < 3 || rx < 3)
                {
                    c = mortar;
                }
                else
                {
                    c = tones[(row * 5 + bi * 3 + row * bi) & 3];
                    if (ry >= bh - 3) c = Shade(c, 12);   
                    else if (ry < 6) c = Shade(c, -10);      
                    int n = (x * 37 + y * 101 + x * y) % 100;
                    if (n < 6) c = Shade(c, -14);
                    else if (n > 94) c = Shade(c, 9);
                }
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), path), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = BgCropH / BgHeightUnits; 
        imp.filterMode = FilterMode.Point;
        imp.wrapMode = TextureWrapMode.Repeat;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        var settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(settings);
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Color32 Shade(Color32 c, int d)
    {
        return new Color32(
            (byte)Mathf.Clamp(c.r + d, 0, 255),
            (byte)Mathf.Clamp(c.g + d, 0, 255),
            (byte)Mathf.Clamp(c.b + d, 0, 255),
            c.a);
    }
}
