using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Boggie : MediumStickMan
{
    [SerializeField] private MeshFilter _meshFilter;

    public List<Boggie> _chainBoggies;

    private int _totalHealthCount = 1;

    public bool IsHalted { get; set; }

    public void AssignBoggies(OnBoggieSpawnSignal onBoggieSpawn)
    {
        if (!_isFirst) return;
        if (onBoggieSpawn.propID != _propID) return;

        onBoggieSpawn.boggie.AssignMainBoggie(this);
        _chainBoggies.Add(onBoggieSpawn.boggie);
        _totalHealthCount += 2;
        SortChainBoggies();
    }
    private Boggie _mainBoggie;
    public void AssignMainBoggie(Boggie boggie)
    {
        _mainBoggie = boggie;
    }
    private void SortChainBoggies()
    {
        _chainBoggies.Sort((a, b) => a._boggieNumber.CompareTo(b._boggieNumber));
    }

    public override void Initialize(ColorType colorType, int health, bool OverideSpeed = false, float Speed = 1)
    {
        SetColor(ColorProvider.GetColor(colorType));

        _navMeshAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;

        if (OverideSpeed)
            _navMeshAgent.speed = Speed;
        else
            SpeedCurve();

        Health = health;

        CanMove = true;
    }

    public override void Move(Vector3 target)
    {
        if (!CanMove) return;

        CanMove = false;
        _isDestined = true;

        _navMeshAgent.SetDestination(new Vector3(target.x, target.y, target.z));
    }

    public override void Damage(float val)
    {
        if (!_isFirst) return;

        if (_chainBoggies.Count == 0)
        {
            isLocked = true;
            KillMe();
            return;
        }

        Boggie last = _chainBoggies[^1];
        last.TakeChainDamage(val);
    }

    private void TakeChainDamage(float val)
    {
        _remainingHits -= (int)val;

        if (_remainingHits <= 0)
        {
            KillMe();
        }
    }

    protected override void KillMe()
    {
        IsHalted = true;

        if (!_isFirst)
        {
            _mainBoggie.OnBoggieDead();
        }

        transform.DOScale(Vector3.one * 0.1f, 0.2f)
       .SetEase(_enemyConfig.KillEffectCurve);
        OnDie();
        //    .OnComplete(OnKillTweenComplete);

        // DOVirtual.DelayedCall(1.5f, () =>
        // {
        onEnemyDieSignal.enemy = this;
        SignalBus.Publish(onEnemyDieSignal);
        OnKillTweenComplete();
        // });
    }

    public override void OnBulletLock()
    {
        _totalHealthCount--;

        isLocked = _totalHealthCount <= 0;
        _remainingHits--;
    }

    public override void OnKillTweenComplete()
    {
        PoolManager.GetPool<Boggie>().Release(this);
    }

    private bool _isFirst;
    public bool isFirst => _isFirst;
    private int _propID, _boggieNumber;
    public void InitializeBoggie(int PropID, int subID, ColorType colorType)
    {
        ColorType = colorType;
        SetColor(ColorProvider.GetColor(colorType));
        _propID = PropID;
        _boggieNumber = subID;
        _totalHealthCount = 2;
        _mainBoggie = null;

        ColorType = colorType;
        _remainingHits = 2;
        _canLookedMultipleTimes = false;
        CanAttack = false;

        _isFirst = _boggieNumber == 0 ? true : false;

        if (!_isFirst)
        {
            isLocked = true;
            ConvertToSimpleBoggie();
        }
        else
        {
            isLocked = false;
            ConvertToFirstBoggie();
        }

        SignalBus.Unsubscribe<OnBoggieSpawnSignal>(AssignBoggies);

        _chainBoggies?.Clear();
        _chainBoggies = null;

        IsHalted = false;

        if (_isFirst)
        {
            _chainBoggies = new List<Boggie>();
            SignalBus.Subscribe<OnBoggieSpawnSignal>(AssignBoggies);
        }
    }

    public void OnSpawn()
    {
        if (_isFirst) return;
        SignalBus.Publish(new OnBoggieSpawnSignal { propID = _propID, boggie = this });
    }

    private Mesh _oldMesh;
    private BoggieFaceData _boggieFaceData;
    private void ConvertToFirstBoggie()
    {
        _meshFilter.transform.localScale = Vector3.one * 1.4f;

        _oldMesh = _meshFilter.sharedMesh;

        if (!_boggieFaceData)
            _boggieFaceData = Resources.Load<BoggieFaceData>("BoggieFaceData");

        _meshFilter.sharedMesh = _boggieFaceData.FaceMesh;

        var mats = _renderer[0].sharedMaterials;

        if (mats.Length < 3)
        {
            System.Array.Resize(ref mats, 3);
        }

        mats[1] = _boggieFaceData.EyeBallMat;
        mats[2] = _boggieFaceData.EyeMat;

        _renderer[0].sharedMaterials = mats;
    }
    private void ConvertToSimpleBoggie()
    {
        _meshFilter.transform.localScale = Vector3.one;
        if (_oldMesh)
            _meshFilter.sharedMesh = _oldMesh;
        _renderer[0].sharedMaterials[1] = null;
        _renderer[0].sharedMaterials[2] = null;
    }

    public bool IsMatch(int propID, ColorType colorType)
    {
        return _propID == propID && ColorType == colorType;
    }
    public bool IsMatch(int propID, int boggieID, ColorType colorType)
    {
        return _propID == propID && _boggieNumber == boggieID && ColorType == colorType;
    }
    public override void OnResume()
    {
        base.OnResume();
        _canLookedMultipleTimes = false;
        if (!_isFirst)
        {
            isLocked = true;
        }
        else
        {
            isLocked = false;
        }
    }
    public void OnBoggieDead()
    {
        if (!_isFirst) return;
        if (_chainBoggies.Count == 0) return;

        _chainBoggies.RemoveAt(_chainBoggies.Count - 1);
    }
}
public class OnBoggieSpawnSignal : ISignal
{
    public int propID;
    public Boggie boggie;
}
public class LastBoggieHitSignal : ISignal
{
    public int propID;
    public ColorType colorType;
}
public class BoggieDeadSignal : ISignal
{
    public int propID, previousBoggieID;
    public ColorType colorType;
}