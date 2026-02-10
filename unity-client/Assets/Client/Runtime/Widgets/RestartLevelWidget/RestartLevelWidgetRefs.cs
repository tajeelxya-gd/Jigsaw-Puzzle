using TMPro;
using UniTx.Runtime.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class RestartLevelWidgetRefs : WidgetRefs
    {
        [SerializeField] private Button _restartBtn;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private TMP_Text _levelText;

        public Button RestartBtn => _restartBtn;
        public Button CloseBtn => _closeBtn;
        public TMP_Text LevelText => _levelText;
    }
}
