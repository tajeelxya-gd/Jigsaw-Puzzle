using Sirenix.OdinInspector;
using UnityEngine;

public class Shatter : MonoBehaviour
{
    public Rigidbody[] Bodies;

    public float expForce, Radius;


    [Button]
    public void Shattered()
    {
        for (int i = 0; i < Bodies.Length; i++)
        {
            Bodies[i].gameObject.SetActive(true);
            Bodies[i].isKinematic = false;
            Bodies[i].AddExplosionForce(expForce, transform.position, Radius);
        }
    }
}