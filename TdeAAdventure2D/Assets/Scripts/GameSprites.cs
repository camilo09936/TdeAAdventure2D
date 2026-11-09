using UnityEngine;

// Sprites pixel-art creados por codigo: moneda, gema, pincho y sierra.
public static class GameSprites
{
    static Sprite coin, gem, tooth, saw;

    public static Sprite Coin { get { if (coin == null) coin = MakeCoin(); return coin; } }
    public static Sprite Gem { get { if (gem == null) gem = MakeGem(); return gem; } }
    public static Sprite Tooth { get { if (tooth == null) tooth = MakeTooth(); return tooth; } }
    public static Sprite Saw { get { if (saw == null) saw = MakeSaw(); return saw; } }

    static Texture2D Blank(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.SetPixels32(new Color32[w * h]);
        return t;
    }

    static Sprite Finish(Texture2D t)
    {
        t.filterMode = FilterMode.Point;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
    }

    static Sprite MakeCoin()
    {
        var t = Blank(12, 12);
        var edge = new Color32(150, 98, 12, 255);
        var fill = new Color32(255, 205, 45, 255);
        var dark = new Color32(224, 160, 20, 255);
        var shine = new Color32(255, 243, 160, 255);
        for (int y = 0; y < 12; y++)
        {
            for (int x = 0; x < 12; x++)
            {
                float dx = x - 5.5f, dy = y - 5.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                if (r > 5.6f) continue;
                Color32 c = r > 4.5f ? edge : fill;
                if (r <= 4.5f && Mathf.Abs(dx) < 1f && Mathf.Abs(dy) < 2.8f) c = dark;
                if (r <= 4.5f && r > 2.5f && dx < -1.5f && dy > 1.5f) c = shine;
                t.SetPixel(x, y, c);
            }
        }
        return Finish(t);
    }

    static Sprite MakeGem()
    {
        var t = Blank(12, 12);
        var edge = new Color32(20, 95, 140, 255);
        var fill = new Color32(60, 205, 240, 255);
        var light = new Color32(175, 245, 255, 255);
        var deep = new Color32(30, 140, 200, 255);
        for (int y = 0; y < 12; y++)
        {
            for (int x = 0; x < 12; x++)
            {
                float dx = x - 5.5f, dy = y - 5.5f;
                float m = Mathf.Abs(dx) + Mathf.Abs(dy);
                if (m > 6f) continue;
                Color32 c = m > 4.8f ? edge : fill;
                if (m <= 4.8f)
                {
                    if (dx < 0f && dy > 0f) c = light;
                    else if (dx > 0f && dy < 0f) c = deep;
                }
                t.SetPixel(x, y, c);
            }
        }
        return Finish(t);
    }

    static Sprite MakeTooth()
    {
        var t = Blank(8, 8);
        var edge = new Color32(60, 62, 75, 255);
        var lightSide = new Color32(215, 220, 230, 255);
        var darkSide = new Color32(165, 170, 185, 255);
        var tip = new Color32(235, 70, 70, 255);
        for (int y = 0; y < 8; y++)
        {
            float hw = (7 - y) * 0.5f + 0.5f;
            for (int x = 0; x < 8; x++)
            {
                float dx = Mathf.Abs(x - 3.5f);
                if (dx > hw) continue;
                Color32 c;
                if (dx > hw - 1f || y == 0) c = edge;
                else if (y >= 5) c = tip;
                else c = x < 4 ? lightSide : darkSide;
                t.SetPixel(x, y, c);
            }
        }
        return Finish(t);
    }

    static Sprite MakeSaw()
    {
        var t = Blank(16, 16);
        var dark = new Color32(60, 62, 75, 255);
        var steel = new Color32(195, 200, 212, 255);
        var red = new Color32(190, 55, 55, 255);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 7.5f, dy = y - 7.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx);
                float rad = 5.2f + (Mathf.Sin(ang * 8f) > 0f ? 2.6f : 0f);
                if (r > rad) continue;
                Color32 c = r > rad - 1.3f ? dark : steel;
                if (r < 2.4f) c = red;
                if (r < 1f) c = dark;
                t.SetPixel(x, y, c);
            }
        }
        return Finish(t);
    }
}
