using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FloorTint : MonoBehaviour
{
    static readonly Color Dusk = new Color(1f, 0.78f, 0.74f);
    static readonly Color Night = new Color(0.30f, 0.34f, 0.56f);
    SpriteRenderer sr;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        sr.color = SkyAmbience.ThreeStage(Color.white, Dusk, Night, SkyAmbience.NightAmount);
    }
}