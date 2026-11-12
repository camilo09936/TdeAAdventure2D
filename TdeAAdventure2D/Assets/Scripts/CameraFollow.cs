using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float lookAhead = 1.5f;
    public float smooth = 5f;

    [Header("Pantalla que sube sola (RF06)")]
    public float velocidadInicial = 0.25f;  
    public float factorAceleracion = 0.02f;   
    public float velocidadMaxima = 50f;       

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