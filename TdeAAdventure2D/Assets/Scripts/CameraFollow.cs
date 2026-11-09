using UnityEngine;

// La camara solo sube. Despues de cierta altura sube sola, cada vez mas rapido:
// si el personaje se queda atras, pierde.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float lookAhead = 1.5f;
    public float smooth = 5f;

    [Header("Camara que sube sola")]
    public float creepStartHeight = 40f;  // metros donde empieza
    public float creepMin = 0.25f;        // unidades por segundo al empezar
    public float creepMax = 1.6f;         // unidades por segundo en dificultad maxima

    void LateUpdate()
    {
        if (target == null) return;

        float wanted = target.position.y + lookAhead;
        Vector3 p = transform.position;
        if (wanted > p.y)
            p.y = Mathf.Lerp(p.y, wanted, 1f - Mathf.Exp(-smooth * Time.deltaTime));

        GameManager gm = GameManager.Instance;
        if (gm != null && !gm.Dead && gm.Height > creepStartHeight)
            p.y += Mathf.Lerp(creepMin, creepMax, Difficulty.At(gm.Height)) * Time.deltaTime;

        p.x = 0f;
        transform.position = p;
    }
}
