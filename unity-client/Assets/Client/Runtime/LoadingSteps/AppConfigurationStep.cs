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
        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            SetAppSettings();
            SetCulture("en-US");

            return UniTask.CompletedTask;
        }

        private static void SetAppSettings()
        {
            Application.runInBackground = false;
            Screen.sleepTimeout = 2 * 60;
            Input.multiTouchEnabled = false;
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
