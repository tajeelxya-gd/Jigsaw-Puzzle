using UnityEngine;

public class LeagueCategoryButtonUI : MonoBehaviour
{
  [SerializeField] private GameObject referenceObject;



  public void OnSelected(bool selected)
  {
    referenceObject.gameObject.SetActive(selected);
    AudioController.PlaySFX(AudioType.InstantiatingSound);
  }
}
