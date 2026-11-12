using System.Collections.Generic;
using UnityEngine;

public partial class LevelGenerator : MonoBehaviour
{
    public Transform player;
    public Sprite platformSprite;
    public float halfWidth = 7f;      
    public float startY = 0.95f;       
    public float heightOrigin = -0.76f; 
    public float generateAhead = 16f; 
    public float cleanupBelow = 14f;   

    [Header("Pisos de la Universidad")]
    public float floorEvery = 50f;
    public float floorApproach = 2.1f;  
    float nextFloorH;
    float highestY;                  

    readonly List<GameObject> tracked = new List<GameObject>();
    Transform holder;
    float nextY, lastX;
    PlatformType lastType = PlatformType.Static;

    void Awake()
    {
        holder = new GameObject("Plataformas").transform;
        var sh = gameObject.AddComponent<SkyHazards>();
        sh.player= player;
        ResetLevel();
    }

    public void ResetLevel()
    {
        foreach (GameObject g in tracked)
            if (g != null) Destroy(g);
        tracked.Clear();

        lastX = -3.5f;
        lastType = PlatformType.Static;
        nextY = Mathf.Max(startY, 0.95f);
        highestY = nextY;
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

        float gap = Random.Range(Mathf.Lerp(1.5f, 1.8f, d), Mathf.Lerp(1.9f, 2.35f, d));
        nextY += gap;

        if (floorEvery > 0f && nextY >= heightOrigin + nextFloorH - 1f)
        {

            float floorTop = Mathf.Max(nextY, highestY + floorApproach);
            SpawnFloor(floorTop);
            highestY = floorTop;
            nextY = floorTop;              
            lastType = PlatformType.Static;
            nextFloorH += floorEvery;
            return;
        }

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

        bool spikes = type != PlatformType.Falling && h > 20f && Random.value < Mathf.Lerp(0.10f, 0.45f, d);
        if (spikes) width = Mathf.Max(width, 3.2f);

        float fallDelay = Mathf.Lerp(0.5f, 0.25f, d);
        Platform p = Spawn(type, x, nextY, width, amp, speed, fallDelay);

        float spikeX = 0f;
        if (spikes) spikeX = AddSpikes(p, width, d);
        AddRewards(p, type, x, nextY, width, amp, d, h, spikes, spikeX);

        lastX = x;
        lastType = type;

        if (Random.value < Mathf.Lerp(0.30f, 0.10f, d))
        {
            float minDist = width + amp + 0.8f;  
            for (int i = 0; i < 8; i++)
            {
                float x2 = Random.Range(-halfWidth, halfWidth);
                if (Mathf.Abs(x2 - x) < minDist) continue;
                Spawn(PlatformType.Static, x2, nextY + Random.Range(-0.15f, 0.15f), width, 0f, 0f, 0.5f);
                break;
            }
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
        if (y > highestY) highestY = y;
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

        var art = new GameObject("PisoArte");
        art.transform.SetParent(go.transform, false);
        art.transform.localPosition = new Vector3(0f, -0.25f, 0f); 
        var sr = art.AddComponent<SpriteRenderer>();
        sr.sprite = GameSprites.Floor;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(18.4f, 1f);
        sr.sortingOrder = -7;
        art.AddComponent<FloorTint>();

        tracked.Add(go);
    }
}