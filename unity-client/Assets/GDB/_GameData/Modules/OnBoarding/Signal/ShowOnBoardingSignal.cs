using UnityEngine;

public class ShowOnBoardingSignal :ISignal
{
   public bool DoActivate = false;
   public RectTransform FocusTargetTransform;
   public RectTransform FocusHandPosition;
   public RectTransform InfoPanelPosition;
   public string InfoText = "";
   public OnBoardingMainMenue.InfoAlignment Alignment;
}
