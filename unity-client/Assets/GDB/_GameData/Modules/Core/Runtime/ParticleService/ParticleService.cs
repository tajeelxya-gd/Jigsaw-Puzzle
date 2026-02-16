using UnityEngine;
using UnityEngine.Pool;

public class ParticleService : IParticleService
{
    private ParticleData _particleData;

    public ParticleService(ParticleData particleData)
    {
        _particleData = particleData;

        foreach (var p in _particleData.Data)
        {
            p.Pool = new ObjectPool<GameObject>(
                () => Object.Instantiate(p.Particle),
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                obj => Object.Destroy(obj),
                false,
                10,
                20
            );
        }
        _mpb = new MaterialPropertyBlock();
    }
    private MaterialPropertyBlock _mpb;
    public void PlayParticle(ParticleType particleType, Transform transform = null, bool makeChild = false, bool pool = true, bool SetColor = false, Color color = new Color())
    {
        GameObject obj = GetParticleObj(particleType, pool);

        if (obj == null) return;

        if (transform)
        {
            obj.transform.SetPositionAndRotation(transform.position, transform.rotation);

            if (makeChild)
            {
                obj.transform.SetParent(transform);
                obj.transform.SetParent(null, true);
            }
        }

        if (SetColor)
        {
            _mpb.SetColor("_Color", color);
            obj.GetComponent<ParticleSystemRenderer>().SetPropertyBlock(_mpb);
        }

        obj.GetComponent<ParticleSystem>().Play();
    }

    public void PlayParticle(ParticleType particleType, Vector3 pos, bool pool = true, bool SetColor = false, Color color = new Color())
    {
        GameObject obj = GetParticleObj(particleType, pool);

        if (obj == null) return;

        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.identity;

        if (SetColor)
        {
            _mpb.SetColor("_Color", color);
            obj.GetComponent<ParticleSystemRenderer>().SetPropertyBlock(_mpb);
        }

        obj.GetComponent<ParticleSystem>().Play();
    }

    private GameObject GetParticleObj(ParticleType type, bool usePool)
    {
        var particle = _particleData.Data[(int)type];

        // if (!usePool)
        //     return Object.Instantiate(particle.Particle);

        if (particle.Pool.CountActive >= 20)
            return null;

        var obj = particle.Pool.Get();

        if (obj == null) return null;

        var ps = obj.GetComponent<ParticleSystem>();
        var callback = obj.GetComponent<ParticleReturnToPool>();
        if (!callback) callback = obj.AddComponent<ParticleReturnToPool>();

        callback.Init(ps, () => particle.Pool.Release(obj));
        return obj;
    }
}