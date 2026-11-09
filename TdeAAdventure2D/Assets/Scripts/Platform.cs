using System.Collections;
using UnityEngine;

public enum PlatformType { Static, Moving, Falling }

// Compatibilidad entre versiones de Unity para la velocidad del Rigidbody2D
public static class RbCompat
{
    public static Vector2 GetVel(Rigidbody2D rb)
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    public static void SetVel(Rigidbody2D rb, Vector2 v)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = v;
#else
        rb.velocity = v;
#endif
    }
}

// Plataforma de un solo sentido: se puede atravesar desde abajo y pararse arriba.
// Static = fija, Moving = se mueve de lado a lado, Falling = cae al pararse encima.
public class Platform : MonoBehaviour
{
    public PlatformType type;
    public float width = 2.4f;
    public float amplitude = 2f;
    public float moveSpeed = 1.2f;
    public float fallDelay = 0.45f;

    const float Height = 0.5f;

    float startX, phase, spawnTime;
    Rigidbody2D rb;
    Collider2D col;
    SpriteRenderer sr;
    bool triggered;

    public static Color ColorFor(PlatformType t)
    {
        switch (t)
        {
            case PlatformType.Moving: return new Color(0.35f, 0.62f, 0.95f);
            case PlatformType.Falling: return new Color(0.95f, 0.62f, 0.25f);
            default: return new Color(0.62f, 0.66f, 0.72f);
        }
    }

    public void Setup(Sprite sprite, PlatformType t, float w, float amp, float speed, float delay)
    {
        type = t;
        width = w;
        amplitude = amp;
        moveSpeed = speed;
        fallDelay = delay;
        startX = transform.position.x;
        phase = Random.value * 6.28f;
        spawnTime = Time.time;

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(width, Height);
        sr.sortingOrder = 2;
        sr.color = ColorFor(t);

        var eff = gameObject.AddComponent<PlatformEffector2D>();
        eff.useOneWay = true;
        eff.surfaceArc = 160f;
        eff.useSideFriction = false;
        eff.useSideBounce = false;

        var bc = gameObject.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(width, Height);
        bc.usedByEffector = true;
        col = bc;

        if (t == PlatformType.Moving)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            Vector3 p = transform.position;
            p.x = startX + Mathf.Sin(phase) * amplitude;
            transform.position = p;
        }
    }

    void FixedUpdate()
    {
        if (type != PlatformType.Moving || rb == null) return;
        float t = (Time.time - spawnTime) * moveSpeed + phase;
        float targetX = startX + Mathf.Sin(t) * amplitude;
        float vx = (targetX - rb.position.x) / Time.fixedDeltaTime;
        RbCompat.SetVel(rb, new Vector2(vx, 0f));
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (type != PlatformType.Falling || triggered) return;
        if (c.collider.GetComponent<PlayerController>() == null) return;
        if (c.transform.position.y < transform.position.y) return; // solo si lo pisa desde arriba
        triggered = true;
        StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        Vector3 basePos = transform.position;
        Color baseColor = ColorFor(type);
        float t = 0f;
        while (t < fallDelay)
        {
            t += Time.deltaTime;
            float shake = Mathf.Sin(t * 70f) * 0.03f;
            transform.position = basePos + new Vector3(shake, 0f, 0f);
            sr.color = Color.Lerp(baseColor, new Color(0.9f, 0.2f, 0.15f), t / fallDelay);
            yield return null;
        }

        col.enabled = false; // el personaje cae junto con la plataforma
        float speed = 0f;
        float life = 0f;
        while (life < 3f)
        {
            speed += 25f * Time.deltaTime;
            transform.position += Vector3.down * speed * Time.deltaTime;
            life += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
