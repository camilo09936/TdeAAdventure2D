using UnityEngine;

// Animacion del personaje. Parte el dibujo en cabeza, torso y dos piernas y las mueve por codigo.
// La animacion en si esta en PlayerVisualAnim.cs (misma clase).
[RequireComponent(typeof(PlayerController))]
public partial class PlayerVisual : MonoBehaviour
{
    // Cuadro de 32x32 px, filas contadas desde arriba: cabeza 0-17, torso 18-25, piernas 26-31.
    // El centro horizontal del dibujo esta en x = 13.5; las piernas en x = 12.5 y 18.5.
    const float ArtX = 13.5f;
    const int LegH = 6, TorsoH = 8, HeadH = 18;

    PlayerController pc;
    bool ok, inAir;
    Transform visual, torso, head, legA, legB, airT;
    SpriteRenderer srTorso, srHead, srLegA, srLegB, srAir;
    float unit, legAX, legBX;

    float turn = 1f, lastFacing = 1f, phase, legAmp, airTimer, dustTimer, lean;
    float sqX = 1f, sqY = 1f, sqVx, sqVy;
    float headAng, headAngVel, headY, headYVel;

    Sprite[] defaultFrames;
    void Awake()
    {
        pc = GetComponent<PlayerController>();
        defaultFrames = pc.frames;
        Build();
    }

    void Build()
    {
        ok = false;
        if (pc.frames == null || pc.frames.Length < 6 || pc.frames[0] == null || pc.frames[5] == null)
            return; // sin cuadros: se queda el dibujo normal

        Sprite idle = pc.frames[0];
        unit = 1f / idle.pixelsPerUnit;
        legAX = (12.5f - ArtX) * unit;
        legBX = (18.5f - ArtX) * unit;

        GetComponent<SpriteRenderer>().enabled = false;

        visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);

        srLegA = Make("Pierna_A", visual, Cut(idle, 0, 0, 16, LegH, 12.5f, LegH), 5, new Vector3(legAX, LegH * unit, 0f), out legA);
        srLegB = Make("Pierna_B", visual, Cut(idle, 16, 0, 16, LegH, 2.5f, LegH), 5, new Vector3(legBX, LegH * unit, 0f), out legB);
        srTorso = Make("Torso", visual, Cut(idle, 0, LegH, 32, TorsoH, ArtX, 0f), 6, new Vector3(0f, LegH * unit, 0f), out torso);
        srHead = Make("Cabeza", torso, Cut(idle, 0, LegH + TorsoH, 32, HeadH, ArtX, 0f), 7, new Vector3(0f, TorsoH * unit, 0f), out head);
        srAir = Make("Salto", visual, Cut(pc.frames[5], 0, 0, 32, 32, ArtX, 0f), 6, Vector3.zero, out airT);
        srAir.enabled = false;

        pc.Jumped += OnJumped;
        pc.Landed += OnLanded;
        pc.Jumped += OnJumped;
        pc.Landed += OnLanded;
        ok = true;
    }

    public void ApplySkin(Texture2D hoja)
    {
        if (hoja == null)
        {
            pc.frames = defaultFrames;
        }
        else
        {
            float ppu = defaultFrames[0].pixelsPerUnit;
            int fw = hoja.width / 6;
            var f = new Sprite[6];
            for (int i = 0; i < 6; i++)
                f[i] = Sprite.Create(hoja, new Rect(i * fw, 0, fw, hoja.height), new Vector2(0.5f, 0f), ppu, 0, SpriteMeshType.FullRect);
            pc.frames = f;
        }

        if (visual != null)
        {
            visual.gameObject.SetActive(false);
            Destroy(visual.gameObject);
            visual = null;
        }
        Build();
    }

    void OnDestroy()
    {
        if (pc == null) return;
        pc.Jumped -= OnJumped;
        pc.Landed -= OnLanded;
    }

    // Recorta una pieza del cuadro. (px, py) es el punto de giro dentro de la pieza, en pixeles.
    static Sprite Cut(Sprite frame, int x, int y, int w, int h, float px, float py)
    {
        Rect fr = frame.rect;
        return Sprite.Create(frame.texture, new Rect(fr.x + x, fr.y + y, w, h),
            new Vector2(px / w, py / h), frame.pixelsPerUnit, 0, SpriteMeshType.FullRect);
    }

    static SpriteRenderer Make(string name, Transform parent, Sprite sp, int order, Vector3 pos, out Transform t)
    {
        var go = new GameObject(name);
        t = go.transform;
        t.SetParent(parent, false);
        t.localPosition = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sortingOrder = order;
        return sr;
    }

    // Resorte con rebote: se acerca al objetivo y se pasa un poquito antes de asentarse
    static float Spring(float cur, ref float vel, float target, float k, float damp, float dt)
    {
        vel += (target - cur) * k * dt;
        vel *= Mathf.Exp(-damp * dt);
        return cur + vel * dt;
    }

    void SetPose(bool air)
    {
        srAir.enabled = air;
        srTorso.enabled = !air;
        srHead.enabled = !air;
        srLegA.enabled = !air;
        srLegB.enabled = !air;
    }

    void Puffs(int n, float spread, float up)
    {
        for (int i = 0; i < n; i++)
        {
            float side = (i % 2 == 0) ? -1f : 1f;
            Vector3 p = transform.position + new Vector3(side * Random.Range(0.1f, 0.35f), 0.1f, 0f);
            Vector2 v = new Vector2(side * Random.Range(0.4f, 1f) * spread, Random.Range(0.2f, 1f) * up);
            Dust.Puff(p, v, Random.Range(0.3f, 0.5f), Random.Range(0.3f, 0.5f));
        }
    }

    void OnJumped()
    {
        // estirarse al despegar
        sqX = 0.8f; sqY = 1.28f; sqVx = 0f; sqVy = 0f;
        headY = -0.06f; headYVel = 0f;
        Puffs(3, 1.4f, 0.5f);
    }

    void OnLanded(float impact)
    {
        float k = Mathf.Clamp01((impact - 2f) / 16f);
        if (k <= 0f) return;
        // aplastarse al caer, la cabeza se hunde un poco
        sqX = 1f + 0.4f * k; sqY = 1f - 0.35f * k; sqVx = 0f; sqVy = 0f;
        headY = -0.1f * k; headYVel = 0f;
        if (impact > 4f) Puffs(2 + Mathf.RoundToInt(4f * k), 2f, 0.6f);
    }
}
