using Dalamud.Game.ClientState.Conditions;
using System;

namespace AutoBGM
{
  public class AutoBGM : IDisposable
  {
    private readonly ConfigurationMKI configuration;

    public AutoBGM(ConfigurationMKI configuration)
    {
      this.configuration = configuration;
    }

    public void Dispose()
    {
    }

    public void OnCondition(ConditionFlag flag, bool value)
    {
      if (configuration.Enabled)
      {
        if (configuration.EnableConditions.Exists(x => x.Condition == flag && x.Value == value))
        {
          Service.PluginLog.Debug("Condition: " + Enum.GetName(flag) + " => " + value);
          Service.GameConfig.TryGet(Dalamud.Game.Config.SystemConfigOption.IsSndBgm, out uint IsSndBgmMuted);

          if (IsSndBgmMuted == 1)
          {
            Service.PluginLog.Debug("Enabling BGM because of Condition: " + Enum.GetName(flag) + " => " + value);
            Service.GameConfig.Set(Dalamud.Game.Config.SystemConfigOption.IsSndBgm, 0);
          }
        }
        else if (configuration.DisableConditions.Exists(x => x.Condition == flag && x.Value == value))
        {
          Service.PluginLog.Debug("Condition: " + Enum.GetName(flag) + " => " + value);
          Service.GameConfig.TryGet(Dalamud.Game.Config.SystemConfigOption.IsSndBgm, out uint IsSndBgmMuted);

          if (IsSndBgmMuted == 0)
          {
            Service.PluginLog.Debug("Disabling BGM because of Condition: " + Enum.GetName(flag) + " => " + value);
            Service.GameConfig.Set(Dalamud.Game.Config.SystemConfigOption.IsSndBgm, 1);
          }
        }
      }
    }
  }
}
