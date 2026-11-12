using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InfiniteBackground : MonoBehaviour
{
    public Transform cam;
    public float width = 22f;
    public float height = 16f;

    SpriteRenderer sr;
    float tileH = 1f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null) return;

        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        tileH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
        float h = Mathf.Ceil(height / tileH) * tileH;
        sr.size = new Vector2(width, h);
    }

    void LateUpdate()
    {
        if (cam == null) return;
        float y = Mathf.Round(cam.position.y / tileH) * tileH;
        transform.position = new Vector3(0f, y, transform.position.z);
    }
}
