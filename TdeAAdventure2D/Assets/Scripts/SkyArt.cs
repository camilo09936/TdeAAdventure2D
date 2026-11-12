using UnityEngine;

public static class SkyArt
{
    static Sprite Make(Texture2D t, float ppu)
    {
        t.wrapMode = TextureWrapMode.Clamp;
        t.filterMode = FilterMode.Bilinear;
        t.hideFlags = HideFlags.HideAndDontSave;
        t.Apply();
        Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), ppu);
        s.hideFlags = HideFlags.HideAndDontSave;
        return s;
    }

    public static Sprite Gradient()
    {
        var t = new Texture2D(2, 64, TextureFormat.RGBA32, false);
        for (int y = 0; y < 64; y++)
        {
            float v = Mathf.Lerp(1f, 0.68f, y / 63f);
            for (int x = 0; x < 2; x++) t.SetPixel(x, y, new Color(v, v, v, 1f));
        }
        return Make(t, 64f);   
    }

    public static Sprite Cloud(int seed)
    {
        const int W = 128, H = 64;
        var rnd = new System.Random(seed);
        int n = 5 + rnd.Next(3);
        float[] cx = new float[n], cy = new float[n], r = new float[n];
        for (int i = 0; i < n; i++)
        {
            float k = n == 1 ? 0.5f : i / (float)(n - 1);
            cx[i] = W * (0.2f + 0.6f * k) + (float)(rnd.NextDouble() - 0.5) * 6f;
            float mid = 1f - Mathf.Abs(k - 0.5f) * 2f;         
            r[i] = H * (0.20f + 0.20f * mid) + (float)rnd.NextDouble() * 4f;
            cy[i] = H * 0.30f + mid * H * 0.12f;
        }

        var t = new Texture2D(W, H, TextureFormat.RGBA32, false);
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                float a = 0f;
                for (int i = 0; i < n; i++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx[i], cy[i]));
                    a = Mathf.Max(a, Mathf.Clamp01((1f - d / r[i]) * 2.4f));
                }
                a *= Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(y / (H * 0.22f)));   
                float shade = Mathf.Lerp(0.84f, 1f, y / (float)H);
                t.SetPixel(x, y, new Color(shade, shade, shade, a));
            }
        }
        return Make(t, 32f);   
    }

    public static Sprite Dot()
    {
        const int S = 32;
        var t = new Texture2D(S, S, TextureFormat.RGBA32, false);
        for (int y = 0; y < S; y++)
        {
            for (int x = 0; x < S; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(S / 2f, S / 2f)) / (S / 2f);
                float a = Mathf.Clamp01((1f - d) * 2.2f);
                t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        return Make(t, 32f);  
    }
}
