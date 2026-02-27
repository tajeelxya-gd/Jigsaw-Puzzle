using UnityEngine;
using UnityEngine.UI;

public class RacerRank : MonoBehaviour, IGetRank
{
    [SerializeField] private Ranks _rank;
    [SerializeField] private Image _badge;
    public Ranks GetRank()
    {
        return _rank;
    }
    public void SetRank(Ranks rank, Sprite badge)
    {
        _rank = rank;
        _badge.sprite = badge;
        _badge.enabled = badge != null;
    }
}
public interface IGetRank
{
    Ranks GetRank();
}