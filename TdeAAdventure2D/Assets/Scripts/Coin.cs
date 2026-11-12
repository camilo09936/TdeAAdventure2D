using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 5;
    float baseY, phase;
    bool taken;

    public static Coin Create(Vector3 pos, bool gem, Transform parent = null)
    {
        var go = new GameObject(gem ? "Gema" : "Moneda");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = gem ? GameSprites.Gem : GameSprites.Coin;
        sr.sortingOrder = 4;

        var cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.45f;

        var c = go.AddComponent<Coin>();
        c.value = gem ? 25 : 5;
        return c;
    }

    void Start()
    {
        baseY = transform.position.y;
        phase = Random.value * 6.28f;
    }

    void Update()
    {
        float t = Time.time * 3f + phase;
        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(t) * 0.08f;
        transform.position = p;

        transform.localScale = new Vector3(0.25f + 0.75f * Mathf.Abs(Mathf.Cos(t * 0.7f)), 1f, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        AudioManager.Play(value >= 25 ? "gem" : "coin");
        if (taken) return;
        if (other.GetComponent<PlayerController>() == null) return;
        taken = true;
        if (GameManager.Instance != null) GameManager.Instance.AddPoints(value);
        Destroy(gameObject);
    }
}
