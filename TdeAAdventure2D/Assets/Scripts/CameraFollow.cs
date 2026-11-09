using UnityEngine;

// La camara sigue al personaje hacia arriba y ademas sube sola (RF06):
// VelocidadActual = velocidadInicial + (tiempo transcurrido x factorAceleracion).
// Si el personaje queda por debajo del limite inferior de la camara, pierde (ver GameManager).
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float lookAhead = 1.5f;
    public float smooth = 5f;

    [Header("Pantalla que sube sola (RF06)")]
    public float velocidadInicial = 0.25f;     // unidades por segundo al empezar
    public float factorAceleracion = 0.02f;    // cuanto aumenta la velocidad cada segundo
    public float velocidadMaxima = 50f;        // tope de seguridad (la API acepta hasta 999.99)

    public float CurrentSpeed
    {
        get
        {
            GameManager gm = GameManager.Instance;
            float t = gm != null ? gm.RunTime : 0f;
            return Mathf.Min(velocidadInicial + t * factorAceleracion, velocidadMaxima);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float wanted = target.position.y + lookAhead;
        Vector3 p = transform.position;
        if (wanted > p.y)
            p.y = Mathf.Lerp(p.y, wanted, 1f - Mathf.Exp(-smooth * Time.deltaTime));

        GameManager gm = GameManager.Instance;
        if (gm != null && !gm.Dead && !gm.InMenu)
            p.y += CurrentSpeed * Time.deltaTime;

        p.x = 0f;
        transform.position = p;
    }
}