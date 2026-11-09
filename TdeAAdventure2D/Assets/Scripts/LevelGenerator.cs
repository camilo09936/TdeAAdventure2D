using System.Collections.Generic;
using UnityEngine;

// Crea plataformas hacia arriba sin fin. Mientras mas alto, mas dificil.
// Las monedas y trampas estan en LevelGeneratorItems.cs (misma clase).
public partial class LevelGenerator : MonoBehaviour
{
    public Transform player;
    public Sprite platformSprite;
    public float halfWidth = 7f;        // limite horizontal del centro de las plataformas
    public float startY = 0.95f;        // altura de la primera plataforma (alcanzable desde el suelo)
    public float heightOrigin = -0.76f; // altura donde empieza el personaje (0 metros)
    public float generateAhead = 16f;   // cuanto crear por encima del jugador
    public float cleanupBelow = 14f;    // cuanto conservar por debajo del jugador

    [Header("Pisos de la Universidad")]
    public float floorEvery = 50f;
    GameObject floorTemplate;
    float refTop, nextFloorH;

    readonly List<GameObject> tracked = new List<GameObject>();
    Transform holder;
    float nextY, lastX;
    PlatformType lastType = PlatformType.Static;

    void Awake()
    {
        holder = new GameObject("Plataformas").transform;
        floorTemplate = GameObject.Find("Background");
        GameObject ground = GameObject.Find("Ground");
        Collider2D gc = ground != null ? ground.GetComponent<Collider2D>() : null;
        refTop = gc != null ? gc.bounds.max.y : -1.06f;
        ResetLevel();
    }

    public void ResetLevel()
    {
        foreach (GameObject g in tracked)
            if (g != null) Destroy(g);
        tracked.Clear();

        // La primera plataforma es fija y queda lejos de donde aparece el personaje
        lastX = -3.5f;
        lastType = PlatformType.Static;
        nextY = Mathf.Max(startY, 0.95f);
        Spawn(PlatformType.Static, lastX, nextY, 3f, 0f, 0f, 0.5f);
        nextFloorH = floorEvery;
    }

    void Update()
    {
        if (player == null) return;

        while (nextY < player.position.y + generateAhead)
            SpawnRow();

        for (int i = tracked.Count - 1; i >= 0; i--)
        {
            GameObject g = tracked[i];
            if (g == null) { tracked.RemoveAt(i); continue; }
            if (g.transform.position.y < player.position.y - cleanupBelow)
            {
                Destroy(g);
                tracked.RemoveAt(i);
            }
        }
    }

    void SpawnRow()
    {
        float h = Mathf.Max(0f, nextY - heightOrigin);
        float d = Difficulty.At(h);

        // Separacion vertical: crece con la dificultad (el salto llega a unos 2.8)
        float gap = Random.Range(Mathf.Lerp(1.5f, 1.8f, d), Mathf.Lerp(1.9f, 2.35f, d));
        nextY += gap;

        if (floorEvery > 0f && nextY >= heightOrigin + nextFloorH - 1f)
        {
            SpawnFloor(nextY);
            nextY -= 0.25f;
            lastType = PlatformType.Static;
            nextFloorH += floorEvery;
            return;
        }

        // Mas plataformas moviles y que se caen mientras mas alto
        float movingChance = Mathf.Lerp(0.12f, 0.45f, d);
        float fallingChance = Mathf.Lerp(0.06f, 0.30f, d);
        float r = Random.value;
        PlatformType type = PlatformType.Static;
        if (r < fallingChance) type = PlatformType.Falling;
        else if (r < fallingChance + movingChance) type = PlatformType.Moving;
        if (type == PlatformType.Falling && lastType == PlatformType.Falling)
            type = PlatformType.Static;

        float width = Mathf.Lerp(2.8f, 1.8f, d);
        float x = Mathf.Clamp(lastX + Random.Range(-4f, 4f), -halfWidth, halfWidth);

        float amp = 0f, speed = 0f;
        if (type == PlatformType.Moving)
        {
            speed = Random.Range(1.0f, 1.8f) * Mathf.Lerp(1f, 1.8f, d);
            amp = Mathf.Min(Random.Range(1.5f, 3f), halfWidth - Mathf.Abs(x));
            if (amp < 0.8f) { type = PlatformType.Static; amp = 0f; speed = 0f; }
        }

        // Pinchos desde los 20 m: la plataforma se hace mas ancha para dejar espacio seguro
        bool spikes = type != PlatformType.Falling && h > 20f && Random.value < Mathf.Lerp(0.10f, 0.45f, d);
        if (spikes) width = Mathf.Max(width, 3.2f);

        float fallDelay = Mathf.Lerp(0.5f, 0.25f, d);
        Platform p = Spawn(type, x, nextY, width, amp, speed, fallDelay);

        float spikeX = 0f;
        if (spikes) spikeX = AddSpikes(p, width, d);
        AddRewards(p, type, x, nextY, width, amp, d, h, spikes, spikeX);

        lastX = x;
        lastType = type;

        // A veces una segunda plataforma fija en la misma fila, para dar opciones
        if (Random.value < Mathf.Lerp(0.30f, 0.10f, d))
        {
            float x2 = x > 0f ? Random.Range(-halfWidth, -2f) : Random.Range(2f, halfWidth);
            Spawn(PlatformType.Static, x2, nextY + Random.Range(-0.2f, 0.2f), width, 0f, 0f, 0.5f);
        }
    }

    Platform Spawn(PlatformType type, float x, float y, float width, float amp, float speed, float fallDelay)
    {
        var go = new GameObject("Plataforma_" + type);
        go.transform.SetParent(holder, false);
        go.transform.position = new Vector3(x, y, 0f);
        var p = go.AddComponent<Platform>();
        p.Setup(platformSprite, type, width, amp, speed, fallDelay);
        tracked.Add(go);
        return p;
    }

    void Track(GameObject g)
    {
        tracked.Add(g);
    }

    void SpawnFloor(float top)
    {
        var go = new GameObject("Piso");
        go.transform.SetParent(holder, false);
        go.transform.position = new Vector3(0f, top - 0.25f, 0f);

        var eff = go.AddComponent<PlatformEffector2D>();
        eff.useOneWay = true;
        eff.surfaceArc = 160f;
        eff.useSideFriction = false;
        eff.useSideBounce = false;

        var bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(18f, 0.5f);
        bc.usedByEffector = true;
        tracked.Add(go);

        if (floorTemplate != null)
        {
            var copy = Instantiate(floorTemplate,
                new Vector3(0f, top - refTop, floorTemplate.transform.position.z), Quaternion.identity);
            copy.name = "PisoFondo";
            var wall = copy.GetComponent<SpriteRenderer>();
            if (wall != null) wall.sortingOrder = -7;   // por encima del cielo del nivel 2
            tracked.Add(copy);
        }
    }
}
