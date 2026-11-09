using UnityEngine;

// Nubecita de polvo: crece un poco, se frena y se desvanece sola.
public class Dust : MonoBehaviour
{
    static Sprite sprite;

    SpriteRenderer sr;
    Vector2 vel;
    float size, life, age;
    Color baseColor;

    public static void Puff(Vector3 pos, Vector2 vel, float size, float life)
    {
        if (sprite == null) sprite = MakeSprite();

        var go = new GameObject("Polvo");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * size;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        var d = go.AddComponent<Dust>();
        d.sr = sr;
        d.vel = vel;
        d.size = size;
        d.life = Mathf.Max(0.05f, life);
        d.baseColor = new Color(0.95f, 0.92f, 0.84f, 0.85f);
        sr.color = d.baseColor;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = age / life;
        if (t >= 1f) { Destroy(gameObject); return; }

        transform.position += (Vector3)(vel * Time.deltaTime);
        vel *= Mathf.Exp(-4f * Time.deltaTime);
        transform.localScale = Vector3.one * size * (1f + 0.8f * t);

        Color c = baseColor;
        c.a = baseColor.a * (1f - t);
        sr.color = c;
    }

    static Sprite MakeSprite()
    {
        var t = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        t.SetPixels32(new Color32[64]);
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                float dx = x - 3.5f, dy = y - 3.5f;
                if (Mathf.Sqrt(dx * dx + dy * dy) <= 3.6f)
                    t.SetPixel(x, y, new Color32(255, 255, 255, 255));
            }
        }
        t.filterMode = FilterMode.Point;
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
    }
}
