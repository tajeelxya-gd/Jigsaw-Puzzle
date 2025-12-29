using System;
using UnityEngine;

namespace UniTx.Runtime
{
    [CreateAssetMenu(fileName = "NewUniTxConfig", menuName = "UniTx/Config")]
    [Serializable]
    internal sealed class UniTxConfig : ScriptableObject
    {
        [SerializeField] private string _widgetsAssetDataKey = default;
        [SerializeField] private string _widgetsParentTag = default;
        [SerializeField] private string _timeServerUrl = default;
        [SerializeField] private float _saveInterval = 5f;

        public string WidgetsAssetDataKey => _widgetsAssetDataKey;
        public string WidgetsParentTag => _widgetsParentTag;
        public string TimeServerUrl => _timeServerUrl;
        public float SaveInterval => _saveInterval;
    }
}