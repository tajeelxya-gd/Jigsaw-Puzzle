using TMPro;
using UniTx.Runtime.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public class LevelCompletedWidgetRefs : WidgetRefs
    {
        [SerializeField] private Button _nextLevelBtn;
        [SerializeField] private TMP_Text _nextLevelText;
        [SerializeField] private ScriptableObject _levelCompleted;

        public Button NextLevelBtn => _nextLevelBtn;
        public TMP_Text NextLevelText => _nextLevelText;
        public ScriptableObject LevelCompleted => _levelCompleted;
    }
}