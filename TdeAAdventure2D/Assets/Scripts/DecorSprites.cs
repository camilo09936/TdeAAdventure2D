using UnityEngine;

public static class DecorSprites
{
    static Sprite calc, block;

    public static Sprite Calculator { get { if (calc == null) calc = MakeCalc(); return calc; } }
    public static Sprite Block { get { if (block == null) block = MakeBlock(); return block; } }

    static Texture2D Blank(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.SetPixels32(new Color32[w * h]);
        return t;
    }

    static Sprite Finish(Texture2D t, float ppu = 16f)
    {
        t.filterMode = FilterMode.Point;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
    }

    static void Fill(Texture2D t, int x0, int y0, int w, int h, Color32 c)
    {
        for (int y = y0; y < y0 + h; y++)
            for (int x = x0; x < x0 + w; x++)
                if (x >= 0 && y >= 0 && x < t.width && y < t.height) t.SetPixel(x, y, c);
    }

    static Sprite MakeCalc()
    {
        var t = Blank(12, 16);
        Fill(t, 0, 0, 12, 16, new Color32(45, 48, 62, 255));
        Fill(t, 1, 1, 10, 14, new Color32(200, 204, 214, 255));
        Fill(t, 2, 11, 8, 3, new Color32(140, 220, 130, 255));
        t.SetPixel(8, 12, new Color32(40, 90, 40, 255));
        t.SetPixel(6, 12, new Color32(40, 90, 40, 255));
        t.SetPixel(4, 13, new Color32(40, 90, 40, 255));
        var btn = new Color32(60, 70, 120, 255);
        var orange = new Color32(240, 140, 40, 255);
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                Fill(t, 2 + c * 3, 2 + r * 3, 2, 2, (r == 0 && c == 2) ? orange : btn);
        return Finish(t);
    }

    static Sprite MakeBlock()
    {
        var t = Blank(1, 1);
        t.SetPixel(0, 0, new Color32(255, 255, 255, 255));
        return Finish(t, 1f);
    }
}