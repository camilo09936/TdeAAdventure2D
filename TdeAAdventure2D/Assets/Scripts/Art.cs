using System.Collections.Generic;
using UnityEngine;

// Dibujos pixel-art generados por codigo: monedas, pinchos y sierra.
public static class Art
{
    static Sprite coin, special, saw;
    static readonly Dictionary<int, Sprite> spikes = new Dictionary<int, Sprite>();

    static Texture2D NewTex(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.SetPixels32(new Color32[w * h]); // todo transparente
        return t;
    }

    static Sprite Finish(Texture2D t, Vector2 pivot)
    {
        t.filterMode = FilterMode.Point;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), pivot, 16f);
    }

    public static Sprite Coin(bool isSpecial)
    {
        if (isSpecial)
        {
            if (special == null)
                special = MakeCoin(new Color32(120, 235, 255, 255), new Color32(20, 110, 165, 255));
            return special;
        }
        if (coin == null)
            coin = MakeCoin(new Color32(255, 205, 40, 255), new Color32(170, 105, 10, 255));
        return coin;
    }

    static Sprite MakeCoin(Color32 fill, Color32 rim)
    {
        var t = NewTex(16, 16);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 7.5f, dy = y - 7.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                if (r > 7.6f) continue;
                Color32 c = r > 6f ? rim : fill;
                if ((x == 7 || x == 8) && y >= 4 && y <= 11) c = rim;
                if ((x == 4 || x == 5) && y >= 9 && y <= 11) c = new Color32(255, 255, 255, 255);
                t.SetPixel(x, y, c);
            }
        }
        return Finish(t, new Vector2(0.5f, 0.5f));
    }

    // Tira de pinchos: cada diente mide 0.5 unidades. El pivote queda abajo al centro.
    public static Sprite Spikes(int teeth)
    {
        Sprite s;
        if (spikes.TryGetValue(teeth, out s) && s != null) return s;

        var t = NewTex(8 * teeth, 8);
        for (int x = 0; x < 8 * teeth; x++)
        {
            int lx = x % 8;
            int m = Mathf.Min(lx, 7 - lx);
            int hgt = 2 + m * 2;
            for (int y = 0; y < hgt; y++)
            {
                Color32 c = new Color32(176, 181, 192, 255);
                if (y >= hgt - 2) c = new Color32(228, 60, 50, 255);
                if (lx == 0 || lx == 7) c = new Color32(90, 94, 104, 255);
                t.SetPixel(x, y, c);
            }
        }
        s = Finish(t, new Vector2(0.5f, 0f));
        spikes[teeth] = s;
        return s;
    }

    public static Sprite Saw()
    {
        if (saw != null) return saw;
        var t = NewTex(16, 16);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 7.5f, dy = y - 7.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Atan2(dy, dx);
                float limit = 5.5f + (Mathf.Cos(a * 8f) > 0.2f ? 2f : 0f);
                if (r > limit) continue;
                Color32 c = new Color32(190, 195, 205, 255);
                if (r < 2f) c = new Color32(60, 60, 70, 255);
                else if (r < 4f) c = new Color32(120, 125, 135, 255);
                else if (r > limit - 1.6f) c = new Color32(225, 60, 50, 255);
                t.SetPixel(x, y, c);
            }
        }
        saw = Finish(t, new Vector2(0.5f, 0.5f));
        return saw;
    }
}
