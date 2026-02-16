using UnityEngine;

public class ParticleReturnToPool : MonoBehaviour
{
    private System.Action _returnToPool;
    private ParticleSystem _ps;

    public void Init(ParticleSystem ps, System.Action returnToPool)
    {
        _ps = ps;
        _returnToPool = returnToPool;
    }
    private void OnParticleSystemStopped()
    {
        _returnToPool?.Invoke();
    }
}