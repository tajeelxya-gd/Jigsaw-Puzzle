using UnityEngine;

public class PlayerWins : MonoBehaviour, IGetWins
{
    [SerializeField] private PlayerManager _playerManager;
    public int GetWins()
    {
        return _playerManager.GetWins();
    }
}
