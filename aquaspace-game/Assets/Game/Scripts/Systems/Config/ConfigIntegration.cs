using UnityEngine;
using System;

/// <summary>
/// Handles integration with the ConfigSystem, including initialization and event subscription.
/// </summary>
public class ConfigIntegration
{
    public event Action<ConfigData> ConfigChanged;

    private float _scanInterval;

    public float ScanInterval => _scanInterval;

    public ConfigIntegration()
    {
        Initialize();
    }

    private void Initialize()
    {
        ConfigSystem.Initialize();
        ConfigSystem.ConfigChanged += OnConfigSystemChanged;

        if (ConfigSystem.Data != null && ConfigSystem.Data.scanInterval > 0f)
        {
            _scanInterval = ConfigSystem.Data.scanInterval;
        }
    }

    private void OnConfigSystemChanged(ConfigData config)
    {
        if (config != null && config.scanInterval > 0f)
        {
            _scanInterval = config.scanInterval;
        }

        ConfigChanged?.Invoke(config);
    }

    public void Reload()
    {
        ConfigSystem.Reload();
    }

    public void Cleanup()
    {
        ConfigSystem.ConfigChanged -= OnConfigSystemChanged;
    }
}
