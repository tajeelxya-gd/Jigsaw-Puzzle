using System;
using UnityEngine;

public class MaxSdkLogger
{
    private const string SdkTag = "AppLovin MAX";
    public const string KeyVerboseLoggingEnabled = "com.applovin.verbose_logging_enabled";

    /// <summary>
    /// Log debug messages.
    /// </summary>
    [System.Diagnostics.Conditional("TEST_ADS")]
    public static void UserDebug(string message)
    {
        if (MaxSdk.DisableAllLogs) return;

        Debug.Log("Debug [" + SdkTag + "] " + message);
    }

    /// <summary>
    /// Log debug messages when verbose logging is enabled.
    ///
    /// Verbose logging can be enabled by calling <see cref="MaxSdk.SetVerboseLogging"/> or via the Integration Manager for build time logs.
    /// </summary>
    [System.Diagnostics.Conditional("TEST_ADS")]
    public static void D(string message)
    {
        if (MaxSdk.DisableAllLogs && !MaxSdk.IsVerboseLoggingEnabled()) return;

        Debug.Log("Debug [" + SdkTag + "] " + message);
    }

    /// <summary>
    /// Log warning messages.
    /// </summary>
    [System.Diagnostics.Conditional("TEST_ADS")]
    public static void UserWarning(string message)
    {
        if (MaxSdk.DisableAllLogs) return;

        Debug.LogWarning("Warning [" + SdkTag + "] " + message);
    }

    /// <summary>
    /// Log warning messages when verbose logging is enabled.
    ///
    /// Verbose logging can be enabled by calling <see cref="MaxSdk.SetVerboseLogging"/> or via the Integration Manager for build time logs.
    /// </summary>
    [System.Diagnostics.Conditional("TEST_ADS")]
    public static void W(string message)
    {
        if (MaxSdk.DisableAllLogs && !MaxSdk.IsVerboseLoggingEnabled()) return;

        Debug.LogWarning("Warning [" + SdkTag + "] " + message);
    }

    /// <summary>
    /// Log error messages.
    /// </summary>
    [System.Diagnostics.Conditional("TEST_ADS")]
    public static void UserError(string message)
    {
        if (MaxSdk.DisableAllLogs) return;

        Debug.LogError("Error [" + SdkTag + "] " + message);
    }

    /// <summary>
    /// Log error messages when verbose logging is enabled.
    ///
    /// Verbose logging can be enabled by calling <see cref="MaxSdk.SetVerboseLogging"/> or via the Integration Manager for build time logs.
    /// </summary>
    [System.Diagnostics.Conditional("TEST_ADS")]
    public static void E(string message)
    {
        if (MaxSdk.DisableAllLogs && !MaxSdk.IsVerboseLoggingEnabled()) return;

        Debug.LogError("Error [" + SdkTag + "] " + message);
    }

    /// <summary>
    /// Log exceptions.
    /// </summary>
    [System.Diagnostics.Conditional("TEST_ADS")]
    public static void LogException(Exception exception)
    {
        if (MaxSdk.DisableAllLogs) return;
        
        Debug.LogException(exception);
    }
}
