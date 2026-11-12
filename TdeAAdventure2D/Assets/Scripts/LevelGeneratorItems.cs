using UnityEngine;

public partial class LevelGenerator
{
    float AddSpikes(Platform p, float width, float d)
    {
        float spikeW = Mathf.Round(Mathf.Lerp(1f, 2f, d));
        float free = width - spikeW;

        int pos;
        if (free < 2f) pos = Random.value < 0.5f ? 0 : 2; 
        else pos = Random.Range(0, 3);                   

        float spikeX = (pos - 1) * free * 0.5f;
        Hazard.Spikes(p.transform, spikeX, spikeW);
        return spikeX;
    }

    void AddRewards(Platform p, PlatformType type, float x, float y, float width, float amp,
                    float d, float h, bool spikes, float spikeX)
    {
        if (!spikes && Random.value < 0.35f)
            Track(Coin.Create(new Vector3(x, y + 0.95f, 0f), false).gameObject);

        if (spikes)
            Coin.Create(p.transform.position + new Vector3(spikeX, 1.6f, 0f), true, p.transform);

        float hardChance = h < 15f ? 0f : Mathf.Lerp(0.12f, 0.32f, d);
        if (Random.value < hardChance)
        {
            float side = x > 0f ? -1f : 1f;
            float gx = x + side * (width * 0.5f + amp + Random.Range(2.0f, 3.0f));
            gx = Mathf.Clamp(gx, -halfWidth - 0.6f, halfWidth + 0.6f);
            float gy = y + Random.Range(1.5f, 2.2f);
            Vector3 gpos = new Vector3(gx, gy, 0f);
            Track(Coin.Create(gpos, true).gameObject);

            if (d > 0.35f && Random.value < 0.45f)
                Track(Hazard.Saw(gpos, 1.5f, Mathf.Lerp(1.2f, 2.2f, d)).gameObject);
        }
    }
}
