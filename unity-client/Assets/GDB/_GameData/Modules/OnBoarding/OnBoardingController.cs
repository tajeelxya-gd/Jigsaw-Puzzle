using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class OnBoardingMainMenue : MonoBehaviour
{
   public enum InfoAlignment { Right, Left, Bottom, Top }
   [SerializeField] private SoftMaskUI softMaskUI;
   [SerializeField] private BulkPopUpEffect starsGeneratorEffect;
   [SerializeField] private OnBoardingInfoBlock[] infoBlockType;
   [SerializeField] private TextMeshProUGUI infoText;
   private void Awake()
   {
      SignalBus.Subscribe<ShowOnBoardingSignal>(OnShowOnBoardingPanel);
   }


   void OnShowOnBoardingPanel(ShowOnBoardingSignal signal)
   {
      Canvas.ForceUpdateCanvases(); // Ensure all UI positions are calculated
      if (!signal.DoActivate)
         if (softMaskUI.gameObject.activeInHierarchy)
            DOVirtual.DelayedCall(0.1f, ShowStarsEffect);

      softMaskUI.gameObject.SetActive(signal.DoActivate);
      if (!signal.DoActivate) { return; }
      infoText.text = signal.InfoText;
      foreach (var info in infoBlockType)
      {
         info.infoBlock.gameObject.SetActive(info.alignment == signal.Alignment);
         if (info.alignment == signal.Alignment)
         {
            info.infoBlock.position = signal.InfoPanelPosition.transform.position;
            infoText.transform.position = info.InfoPanelPosition.transform.position;
         }
      }

      signal.FocusTargetTransform.SetParent(softMaskUI.transform,true);
      softMaskUI.ShowFocusesMask(signal.FocusTargetTransform, signal.FocusHandPosition, true, false);

   }

   void ShowStarsEffect()
   {
      starsGeneratorEffect.Generate(null, 15);
   }

   private void OnDisable()
   {
      SignalBus.Unsubscribe<ShowOnBoardingSignal>(OnShowOnBoardingPanel);

   }

   [System.Serializable]
   class OnBoardingInfoBlock
   {
      [SerializeField] public InfoAlignment alignment;
      [SerializeField] public RectTransform infoBlock;
      [SerializeField] public RectTransform InfoPanelPosition;
   }
}
