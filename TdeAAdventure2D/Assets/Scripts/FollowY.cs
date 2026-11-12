using UnityEngine;

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
