using UnityEngine;

public class OnShowTutorialDialogue : ISignal
{
  public string DialogueTxt;
  public bool IsButtonActivated = false;
  public RectTransform OverrideTransform = null;
  public TutorialManager.InfoBoxDataBlock InfoBox;
  
}
