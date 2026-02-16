using TMPro;
using UnityEngine;

public class CollectableVisual : MonoBehaviour
{
    [SerializeField] private protected TextMeshProUGUI _amountText;

    public virtual void Initialize()
    {
        _amountText.gameObject.SetActive(false);
        UpdateText(0);
    }

    public virtual void UpdateCount(float count)
    {
        UpdateText(count);
    }
    public virtual void UpdateCount(int count)
    {
        UpdateText(count);
    }
    private protected virtual void UpdateText(float amount)
    {
        _amountText.text = amount.ToString();
    }
    private protected virtual void UpdateText(int amount)
    {
        _amountText.text = amount.ToString();
    }
}