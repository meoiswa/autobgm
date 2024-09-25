using Dalamud.Configuration;
using System;

namespace AutoBGM
{
  [Serializable]
  public class ConfigurationBase : IPluginConfiguration
  {
    public virtual int Version { get; set; } = 0;

    [NonSerialized]
    private Action? saveAction;
    public void Initialize(Action saveAction) => this.saveAction = saveAction;
    public void Save()
    {
      saveAction?.Invoke();
    }
  }
}
