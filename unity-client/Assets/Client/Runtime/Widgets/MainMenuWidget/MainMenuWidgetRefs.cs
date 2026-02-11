using UnityEngine;
using UnityEngine.UI;
using UniTx.Runtime.Widgets;
using TMPro;

namespace Client.Runtime
{
    public sealed class MainMenuWidgetRefs : WidgetRefs
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _playButtonText;

        public Button PlayButton => _playButton;
        public TMP_Text PlayButtonText => _playButtonText;
    }
}
