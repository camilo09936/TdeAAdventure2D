using UnityEngine;

public static class GameSprites
{
    static Sprite coin, gem, paper, pencil, floor;

    public static Sprite Coin { get { if (coin == null) coin = MakeCoin(); return coin; } }
    public static Sprite Gem { get { if (gem == null) gem = MakeGem(); return gem; } }
    public static Sprite Paper { get { if (paper == null) paper = MakePaper(); return paper; } }
    public static Sprite Pencil { get { if (pencil == null) pencil = MakePencil(); return pencil; } }
    public static Sprite Floor { get { if (floor == null) floor = MakeFloor(); return floor; } }

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

    static Sprite MakePaper()
    {
        var t = Blank(16, 8);
        var edge = new Color32(120, 120, 135, 255);
        var paperC = new Color32(248, 248, 240, 255);
        var line = new Color32(90, 130, 215, 255);
        var red = new Color32(210, 40, 40, 255);
        for (int y = 0; y < 8; y++)
        {
            for (int x = 1; x < 15; x++)
            {
                bool border = x == 1 || x == 14 || y == 0 || y == 7;
                t.SetPixel(x, y, border ? edge : paperC);
            }
        }
        // renglones
        for (int x = 3; x <= 8; x++) { t.SetPixel(x, 5, line); t.SetPixel(x, 3, line); }
        // "0" rojo (anillo de 4x5)
        for (int x = 10; x <= 12; x++) { t.SetPixel(x, 2, red); t.SetPixel(x, 6, red); }
        for (int y = 3; y <= 5; y++) { t.SetPixel(10, y, red); t.SetPixel(12, y, red); }
        return Finish(t);
    }

    static Sprite MakePencil()
    {
        var t = Blank(16, 16);
        var pink = new Color32(240, 140, 150, 255);
        var metal = new Color32(175, 180, 195, 255);
        var yel = new Color32(250, 205, 40, 255);
        var yelDark = new Color32(210, 160, 20, 255);
        var wood = new Color32(235, 200, 150, 255);
        var lead = new Color32(50, 50, 60, 255);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                int diff = x - y;
                if (Mathf.Abs(diff) > 1) continue;
                int s = (x + y) / 2;
                Color32 c;
                if (s <= 2) c = pink;
                else if (s == 3) c = metal;
                else if (s <= 11) c = diff == 1 ? yelDark : yel;
                else if (s <= 13) c = wood;
                else c = lead;
                t.SetPixel(x, y, c);
            }
        }
        return Finish(t);
    }

    static Sprite MakeFloor()
    {
        var t = Blank(16, 16);
        var green = new Color32(74, 150, 70, 255);
        var greenL = new Color32(110, 185, 95, 255);
        var greenD = new Color32(40, 100, 50, 255);
        var brick = new Color32(176, 96, 78, 255);
        var brickD = new Color32(140, 72, 60, 255);
        var mortar = new Color32(215, 200, 185, 255);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                Color32 c;
                if (y >= 12) c = ((x * 7 + y * 3) % 5 == 0) ? greenL : green;
                else if (y == 11) c = greenD;
                else
                {
                    int row = y / 4;
                    int off = (row % 2 == 0) ? 0 : 4;
                    bool mortarLine = (y % 4 == 3) || ((x + off) % 8 == 0);
                    c = mortarLine ? mortar : ((x + y) % 6 == 0 ? brickD : brick);
                }
                t.SetPixel(x, y, c);
            }
        }
        return Finish(t);
    }
}