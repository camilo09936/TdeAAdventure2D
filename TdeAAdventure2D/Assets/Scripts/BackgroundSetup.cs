using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundSetup : MonoBehaviour
{
    void Awake()
    {
        GetComponent<SpriteRenderer>().sortingOrder = -10;
    }
}
