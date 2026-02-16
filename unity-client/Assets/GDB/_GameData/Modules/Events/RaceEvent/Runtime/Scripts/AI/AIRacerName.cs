using UnityEngine;

public class AIRacerName : MonoBehaviour,IAIRacerName
{
    [SerializeField] private string _aiNames;
    [SerializeField] private TMPro.TextMeshProUGUI _nameText;

    public void SetRandomName(string name)
    {
        _aiNames = name;
        _nameText.text = _aiNames;
    }

    public string GetName()
    {
        return _aiNames;
    }
}
public interface IAIRacerName
{
    public void SetRandomName(string name);
    public string GetName();
}
