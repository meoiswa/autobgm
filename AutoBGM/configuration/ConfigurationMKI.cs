using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoBGM
{
  [Serializable]
  public class ConfigurationMKI : ConfigurationBase
  {
    public override int Version { get; set; } = 0;

    public bool IsVisible { get; set; } = true;

    public bool Enabled { get; set; } = true;

    public List<ConditionAction> EnableConditions { get; set; } = new List<ConditionAction>();

    public List<ConditionAction> DisableConditions { get; set; } = new List<ConditionAction>();
  }
}
