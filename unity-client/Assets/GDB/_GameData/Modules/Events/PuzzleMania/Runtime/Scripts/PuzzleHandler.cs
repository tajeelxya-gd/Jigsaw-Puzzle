using UnityEngine;

public class PuzzleHandler : MonoBehaviour
{
    [SerializeField] private PuzzleManiaUI _puzzleMania;

    public void OpenPanel()
    {
        _puzzleMania.OpenPuzzleManiaPanel();
    }
}

public class OpenPuzzleManiaSignal : ISignal
{
    public bool _playAnimation;
}
