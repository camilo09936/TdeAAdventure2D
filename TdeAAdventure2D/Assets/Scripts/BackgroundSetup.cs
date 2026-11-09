using UnityEngine;

// Marca este objeto como fondo: agrega el SpriteRenderer automaticamente.
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundSetup : MonoBehaviour
{
    void Awake()
    {
        GetComponent<SpriteRenderer>().sortingOrder = -10;
    }
}
