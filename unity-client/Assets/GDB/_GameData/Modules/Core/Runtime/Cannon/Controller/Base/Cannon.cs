using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Cannon : MonoBehaviour, IInterectable, IShootable
{
    public bool CanShoot => _shotAvailable;
    private protected bool _shotAvailable = true;

    [SerializeField] private protected CannonConfig _cannonConfig;
    [InfoBox("Model Configs")]
    [SerializeField] private protected Transform _bulletPoint;

    public bool CanInterect { set { _canInterect = value; } get { return _canInterect; } }
    private protected bool _canInterect = false;

    [ReadOnly, ShowInInspector]
    public bool IsFirstInRow { get; set; }
    [ReadOnly, ShowInInspector]
    public int ColumnId { get; set; }

    [ReadOnly, ShowInInspector]
    public ColorType _colorType { get; protected set; }
    public ShooterType _cannonType { get; protected set; }
    public bool IsHidden { get; protected set; }
    private protected int _projectileAmount;
    public int TotalProjectileRemaining { get { return _projectileAmount; } }
    private protected IEnemyProvider _enemyProvider;
    public virtual void Initialize(ColorType colorType, int projectileAmount, IEnemyProvider enemyProvider, ShooterType cannonType, bool isHidden = false)
    {
        _colorType = colorType;
        _cannonType = cannonType;
        _projectileAmount = projectileAmount;
        _enemyProvider = enemyProvider;
        IsHidden = isHidden;
    }
    public abstract void InitializeForShoot();
    public abstract void Shot();
    public virtual void UpdateProjectile(int Number)
    {
        _projectileAmount = Number;
    }
    public virtual void SetFirstInRow()
    {
        IsFirstInRow = true;
        CanInterect = true;
    }
    public virtual void SetNotFirstInRow()
    {
        IsFirstInRow = false;
        CanInterect = false;
    }
    public abstract void Remove();
}
