using UnityEngine;

public class Link : MonoBehaviour
{
    [SerializeField] private Transform _from, _to;
    [SerializeField] private Renderer _fromRendrer, _toRendrer;

    public void ApplyColorToFirstRendrer(Color color)
    {
        _fromRendrer.SetColor(color);
    }
    public void ApplyColorToSecondRendrer(Color color)
    {
        _toRendrer.SetColor(color);
    }
    public void HandleOutline(bool en)
    {
        //_outline.enabled = en;
    }

    private Transform _cannonA, _cannonB;
    public void Initialize(Transform cannonA, Transform cannonB)
    {
        _cannonA = cannonA;
        _cannonB = cannonB;
        UpdateLink();
    }
    private void LateUpdate()
    {
        UpdateLink();
    }
    Vector3 offset = Vector3.up * 0.5f;
    private void UpdateLink()
    {
        if (_cannonA == null || _cannonB == null) return;

        Vector3 posA = _cannonA.position + offset;
        Vector3 posB = _cannonB.position + offset;

        // Set bone positions
        _from.position = posA;
        _to.position = posB;

        // Set link center correctly
        transform.position = (posA + posB) * 0.5f;

        // Use the same positions to calculate direction
        Vector3 dir = posB - posA;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}