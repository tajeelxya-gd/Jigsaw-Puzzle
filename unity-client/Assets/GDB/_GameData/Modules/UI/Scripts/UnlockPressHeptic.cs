using System;
using UnityEngine;
using UnityEngine.UI;

public class UnlockPressHeptic : MonoBehaviour
{
    void Start()
    {
        if (TryGetComponent<Button>(out var btn))
        {
            btn.onClick.AddListener(PlayHaptic);
        }
    }


    void PlayHaptic()
    {
        HapticController.VibrateForcefully(HapticType.Wrong);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
