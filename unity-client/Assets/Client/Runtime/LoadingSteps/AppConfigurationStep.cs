using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Bootstrap;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class AppConfigurationStep : LoadingStepBase
    {
        [SerializeField] private bool _runInBackground = false;
        [SerializeField] private int _sleepTimeout = 2 * 60;
        [SerializeField] private bool _multiTouchEnabled = false;
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private string _culture = "en-US";

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            SetAppSettings();
            SetCulture(_culture);

            return UniTask.CompletedTask;
        }

        private void SetAppSettings()
        {
            Application.runInBackground = _runInBackground;
            Screen.sleepTimeout = _sleepTimeout;
            Input.multiTouchEnabled = _multiTouchEnabled;
            Application.targetFrameRate = _targetFrameRate;
        }

        private void SetCulture(string cultureName)
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch (Exception e)
            {
                UniStatics.LogInfo($"Failed to set culture: {cultureName}. {e.Message}", this, Color.red);
            }
        }
    }
}
