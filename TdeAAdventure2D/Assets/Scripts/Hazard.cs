using UnityEngine;

// Trampa: si el personaje la toca, pierde. Pinchos (sobre plataformas) y sierras (giran alrededor de una gema).
public class Hazard : MonoBehaviour
{
    public string reason = "Pinchos";
    public bool orbit;
    public Vector3 center;
    public float radius = 1.5f;
    public float speed = 1.5f;
    public float angle;

    public static void Spikes(Transform parent, float localX, float width)
    {
        var go = new GameObject("Pinchos");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(localX, 0.5f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GameSprites.Tooth;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(width, 0.5f);
        sr.sortingOrder = 3;

        var bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(width - 0.15f, 0.36f);
        bc.offset = new Vector2(0f, -0.04f);

        var h = go.AddComponent<Hazard>();
        h.reason = "Pinchos";
    }

    public static Hazard Saw(Vector3 center, float radius, float speed)
    {
        var go = new GameObject("Sierra");
        go.transform.position = center + new Vector3(radius, 0f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GameSprites.Saw;
        sr.sortingOrder = 4;

        var cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.38f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var h = go.AddComponent<Hazard>();
        h.reason = "Sierra";
        h.orbit = true;
        h.center = center;
        h.radius = radius;
        h.speed = speed;
        h.angle = Random.value * 6.28f;
        return h;
    }

    void Update()
    {
        if (!orbit) return;
        angle += speed * Time.deltaTime;
        transform.position = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        transform.Rotate(0f, 0f, -360f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other) { Check(other); }
    void OnTriggerStay2D(Collider2D other) { Check(other); }

    void Check(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        if (GameManager.Instance != null) GameManager.Instance.Kill(reason);
    }
}
