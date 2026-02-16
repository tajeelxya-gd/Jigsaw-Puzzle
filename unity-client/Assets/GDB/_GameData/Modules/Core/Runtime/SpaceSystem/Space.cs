using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Space : MonoBehaviour, ITickable
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }

    public bool IsHidden { get; private set; }

    public bool IsOccupy { get { return _isOccupied; } }
    [SerializeField][ReadOnly] private bool _isOccupied = false;

    private IShootable _shootable;
    public IShootable Shootable { get { return _shootable; } }
    private IMoveable _character;

    private GameObject _shootableObj;

    public void SetIsHidden(bool ishidden)
    {
        IsHidden = ishidden;
        _spriteRenderer.enabled = !ishidden;
    }

    public void SetOccupance(IShootable shootable, bool Move = true)
    {
        _shootable = shootable;
        _isOccupied = true;
        if (shootable == null) return;
        if (Move)
        {
            _character = shootable as IMoveable;
            _character.Move(transform.position);
            SignalBus.Publish(new OnCannonMovedToSpace());
        }

        if (shootable is MonoBehaviour monoBehaviour)
        {
            _shootableObj = monoBehaviour.gameObject;
        }

        SignalBus.Publish(new OnSpaceOcuupy());

        DOVirtual.DelayedCall(0.5f, () => _canShoot = true);
    }

    public bool IsShooterMatches(IShootable shootable) => _shootable == shootable;

    public void EmptySpace()
    {
        _isOccupied = false;
        _shootable = null;
        _character = null;
        _canShoot = false;
    }

    private bool _canShoot = false;
    public void Tick()
    {
        if (!_isOccupied) return;
        if (_shootable == null) return;
        if (!_canShoot) return;
        _shootable.Shot();
    }
    public void SetIconColor(Color color)
    {
        _spriteRenderer.color = color;
    }
    public void UpdatePosition(Vector3 pos)
    {
        Transform oldParent = null;
        if (_shootableObj)
        {
            oldParent = _shootableObj.transform.parent;
            _shootableObj.transform.SetParent(transform);
        }
        transform.localPosition = pos;
        if (_shootableObj)
            _shootableObj.transform.SetParent(oldParent);
    }
}