using UnityEngine;

public class ObjectFollowEffect : MonoBehaviour
{
    [SerializeField] Transform _target;
    void Update()
    {
        transform.position = _target.position;
    }
}
