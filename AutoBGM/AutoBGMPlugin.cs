using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoBGM
{
  public sealed class AutoBGMPlugin : IDalamudPlugin
  {
    public string Name => "Auto BGM";

    private const string commandName = "/autobgm";

    public IDalamudPluginInterface PluginInterface { get; init; }
    public ICommandManager CommandManager { get; init; }
    public ConfigurationMKI Configuration { get; init; }
    public WindowSystem WindowSystem { get; init; }
    public AutoBGMUI Window { get; init; }

    public AutoBGM AutoBGM { get; init; }

    public AutoBGMPlugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager)
    {
      pluginInterface.Create<Service>();

      PluginInterface = pluginInterface;
      CommandManager = commandManager;

      WindowSystem = new("AutoBGMPlugin");

      Configuration = LoadConfiguration();
      Configuration.EnableConditions.RemoveAll(x => Enum.GetName(x.Condition) == null);
      Configuration.DisableConditions.RemoveAll(x => Enum.GetName(x.Condition) == null);
      Configuration.Initialize(SaveConfiguration);

      Window = new AutoBGMUI(Configuration)
      {
        IsOpen = Configuration.IsVisible
      };

      WindowSystem.AddWindow(Window);

      CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "opens the configuration window"
      });

      PluginInterface.UiBuilder.Draw += DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
      PluginInterface.UiBuilder.OpenMainUi += DrawConfigUI;

      AutoBGM = new AutoBGM(Configuration);

      Service.Condition.ConditionChange += AutoBGM.OnCondition;
    }

    public void Dispose()
    {
      AutoBGM.Dispose();

      Service.Condition.ConditionChange -= AutoBGM.OnCondition;

      PluginInterface.UiBuilder.Draw -= DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

      CommandManager.RemoveHandler(commandName);

      WindowSystem.RemoveAllWindows();
    }

    private ConfigurationMKI LoadConfiguration()
    {
      JObject? baseConfig = null;
      if (File.Exists(PluginInterface.ConfigFile.FullName))
      {
        var configJson = File.ReadAllText(PluginInterface.ConfigFile.FullName);
        baseConfig = JObject.Parse(configJson);
      }

      if (baseConfig != null)
      {
        if ((int?)baseConfig["Version"] == 0)
        {
          var configmki = baseConfig.ToObject<ConfigurationMKI>();
          if (configmki != null)
          {
            return configmki;
          }
        }
      }

      return new ConfigurationMKI();
    }

    public void SaveConfiguration()
    {
      var configJson = JsonConvert.SerializeObject(Configuration, Formatting.Indented);
      File.WriteAllText(PluginInterface.ConfigFile.FullName, configJson);
    }

    private void SetVisible(bool isVisible)
    {
      Configuration.IsVisible = isVisible;
      Configuration.Save();

      Window.IsOpen = Configuration.IsVisible;
    }

    private void OnCommand(string command, string args)
    {
      SetVisible(!Configuration.IsVisible);
    }

    private void DrawUI()
    {
      WindowSystem.Draw();
    }

    private void DrawConfigUI()
    {
      SetVisible(!Configuration.IsVisible);
    }
  }
}
