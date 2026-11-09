using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Movimiento del personaje: A / D para moverse, Espacio para saltar.
// La animacion esta en PlayerVisual.cs.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 6f;
    public float jumpForce = 13f;

    [Header("Sensacion de control")]
    public float coyoteTime = 0.1f;       // se puede saltar un instante despues de salir de una plataforma
    public float jumpBufferTime = 0.12f;  // se recuerda el salto si se presiona justo antes de tocar el suelo

    [Header("Dibujo")]
    public Sprite[] frames;               // cuadros del personaje (0 = quieto, 5 = en el aire)

    public event System.Action Jumped;
    public event System.Action<float> Landed;   // velocidad de impacto

    public float Facing { get; private set; }      // 1 = derecha, -1 = izquierda
    public float RelativeVx { get; private set; }  // velocidad propia (sin contar la plataforma)
    public bool Grounded { get { return grounded; } }
    public Vector2 Velocity { get { return RbCompat.GetVel(rb); } }

    Rigidbody2D rb;
    bool grounded, wasGrounded, jumpHeld, jumping;
    float moveInput, coyote, jumpBuffer, airTime, fallSpeed;
    Vector2 groundVel;                    // velocidad de la plataforma sobre la que estamos

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        Facing = 1f;

        // El dibujo ya queda centrado, asi que el cuerpo fisico tambien
        transform.localScale = Vector3.one;
        var col = GetComponent<BoxCollider2D>();
        Vector2 o = col.offset;
        o.x = 0f;
        col.offset = o;
    }

    void Update()
    {
        bool left = false, right = false, jumpDown = false;
        jumpHeld = false;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            left = kb.aKey.isPressed;
            right = kb.dKey.isPressed;
            jumpDown = kb.spaceKey.wasPressedThisFrame;
            jumpHeld = kb.spaceKey.isPressed;
        }
#else
        left = Input.GetKey(KeyCode.A);
        right = Input.GetKey(KeyCode.D);
        jumpDown = Input.GetKeyDown(KeyCode.Space);
        jumpHeld = Input.GetKey(KeyCode.Space);
#endif

        // A = izquierda, D = derecha
        moveInput = 0f;
        if (left) moveInput -= 1f;
        if (right) moveInput += 1f;
        if (moveInput != 0f) Facing = moveInput > 0f ? 1f : -1f;

        // Espacio = saltar (con un poco de margen antes y despues de tocar la plataforma)
        if (jumpDown) jumpBuffer = jumpBufferTime;
        else jumpBuffer -= Time.deltaTime;

        Vector2 v = Velocity;
        if (grounded && v.y <= 0.5f) coyote = coyoteTime;
        else coyote -= Time.deltaTime;

        // Aterrizaje: avisa con la velocidad con la que cayo
        if (grounded)
        {
            if (!wasGrounded && airTime > 0.1f && Landed != null) Landed(-fallSpeed);
            airTime = 0f;
            fallSpeed = 0f;
        }
        else
        {
            airTime += Time.deltaTime;
            fallSpeed = Mathf.Min(fallSpeed, v.y);
        }
        wasGrounded = grounded;
    }

    void FixedUpdate()
    {
        Vector2 v = RbCompat.GetVel(rb);
        float dt = Time.fixedDeltaTime;

        // Acelera y frena de forma suave. Sobre una plataforma movil se mide respecto a ella.
        float carry = grounded ? groundVel.x : 0f;
        float rel = v.x - carry;
        float target = moveInput * speed;
        float rate;
        if (moveInput == 0f) rate = grounded ? 70f : 20f;
        else if (Mathf.Abs(rel) > 0.1f && Mathf.Sign(rel) != Mathf.Sign(target)) rate = grounded ? 95f : 45f;
        else rate = grounded ? 60f : 38f;
        rel = Mathf.MoveTowards(rel, target, rate * dt);
        v.x = rel + carry;
        RelativeVx = rel;

        // Salto corto si se suelta la tecla mientras sube
        if (jumping && v.y > 0f && !jumpHeld)
        {
            v.y *= 0.5f;
            jumping = false;
        }
        if (v.y <= 0f) jumping = false;

        if (jumpBuffer > 0f && coyote > 0f)
        {
            v.y = jumpForce;
            jumpBuffer = 0f;
            coyote = 0f;
            jumping = true;
            if (Jumped != null) Jumped();
        }

        RbCompat.SetVel(rb, v);
        grounded = false;            // se vuelve a marcar en OnCollisionStay2D
        groundVel = Vector2.zero;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                grounded = true;
                groundVel = collision.rigidbody != null ? RbCompat.GetVel(collision.rigidbody) : Vector2.zero;
                return;
            }
        }
    }
}
