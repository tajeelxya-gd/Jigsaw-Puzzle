using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CarMovement : MonoBehaviour, IGetCarType
{
    [SerializeField] private CarType _carType;
    [SerializeField] private GameObject _car;
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _moveDuration = 1f;
    [SerializeField] private Slider _progressSlider;
    private int _currentWaypointIndex = 0;
    private Vector3 _startPosition;
    private float _progression = 0.25f;

    public CarType GetCarType()
    {
        return _carType;
    }

    private void Start()
    {
        _startPosition = _car.transform.localPosition;
    }

    [Button]
    public void MoveCar(float progress)
    {
        _progression += progress;
        _progressSlider.value = _progression;
    }

    public void SetCarProgression(float progress)
    {
        _progression = progress;
        _progressSlider.value = _progression;
    }

    public float GetCurrentRaceProgress()
    {
        return _progression;
    }

    public void Reset()
    {
        _progression = 0.25f;
        _progressSlider.value = 0.25f;
    }
}

public enum CarType
{
    Player,
    AI
}

public interface IGetCarType
{
    CarType GetCarType();
}