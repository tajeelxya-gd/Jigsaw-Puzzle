using UnityEngine;

public class RacerRank : MonoBehaviour, IGetRank
{
    [SerializeField] private Ranks _rank;
    public Ranks GetRank()
    {
        return _rank;
    }
    public void SetRank(Ranks rank)
    {
        _rank = rank;
    }
}
public interface IGetRank
{
    Ranks GetRank();
}