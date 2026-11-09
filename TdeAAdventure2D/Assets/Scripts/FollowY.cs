using UnityEngine;

// Mantiene este objeto a la misma altura del objetivo (para las paredes laterales).
public class FollowY : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 p = transform.position;
        p.y = target.position.y;
        transform.position = p;
    }
}
