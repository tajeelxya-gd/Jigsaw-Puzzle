using UnityEngine;

namespace Client.Runtime
{
    public sealed class SREventListener : MonoBehaviour
    {
        private static SREventListener _instance;
        public static SREventListener Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterEvents();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                UnRegisterEvents();
            }
        }

        private void RegisterEvents()
        {

        }

        private void UnRegisterEvents()
        {

        }
    }
}