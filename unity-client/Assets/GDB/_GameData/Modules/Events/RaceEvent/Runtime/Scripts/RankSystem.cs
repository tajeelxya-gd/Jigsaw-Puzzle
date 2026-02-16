using System;
using Unity.VisualScripting;
using UnityEngine;

public class RankSystem : MonoBehaviour
{
    [SerializeField] private RacerRank[] _racers;
    private IGetCarType[] _carTypeGetter;
    private IGetWins[] _racersWins;

    private void Start()
    {
        SignalBus.Subscribe<AssignRanks>(UpdateRanks);
        _carTypeGetter = new IGetCarType[_racers.Length];
        _racersWins = new IGetWins[_racers.Length];
        for (int i = 0; i < _racers.Length; i++)
        {
            _carTypeGetter[i] = _racers[i].GetComponent<IGetCarType>();
            _racersWins[i] = _racers[i].GetComponent<IGetWins>();
        }
    }
    
    private void OnDestroy()
    {
        SignalBus.Unsubscribe<AssignRanks>(UpdateRanks);
    }

    private void UpdateRanks(AssignRanks signal)
    {
        for(int i = 0; i < _racers.Length; i++)
        {
            int rank = 1;
            int racerCurrentWins = _racersWins[i].GetWins();
            for(int j = 0; j < _racers.Length; j++)
            {
                if(i == j) continue;
                int otherRacerWins = _racersWins[j].GetWins();

                if(racerCurrentWins < otherRacerWins)
                {
                    rank++; 
                }
                else if (racerCurrentWins == otherRacerWins)
                {
                    if (j < i)
                    {
                        rank++;
                    }
                }
            }
            _racers[i].SetRank((Ranks)rank);
        }
    }
}

public enum Ranks
{
    None,
    First=1,
    Second=2,
    Third=3,
    Fourth=4,
    Fifth=5
}