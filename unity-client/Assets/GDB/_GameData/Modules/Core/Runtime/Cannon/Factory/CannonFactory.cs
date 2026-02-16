using UnityEngine;

public class CannonFactory : IFactory<Cannon, ShooterType>
{
    private readonly CannonData _cannonData;

    public CannonFactory(CannonData cannonData)
    {
        _cannonData = cannonData;
    }

    public Cannon Create(ShooterType type, Transform parent)
    {
        var info = _cannonData.GetCannonInfo(type);
        if (info == null) return null;

        var prefab = info.GetCannonPrefab(0);
        if (prefab == null) return null;

        return Object.Instantiate(prefab, parent);
    }
}