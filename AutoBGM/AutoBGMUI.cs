using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AutoBGM
{
  public class AutoBGMUI : Window, IDisposable
  {
    private readonly ConfigurationMKI configuration;

    private string[] conditionNames = Enum.GetNames(typeof(ConditionFlag));

    public AutoBGMUI(ConfigurationMKI configuration)
      : base(
        "AutoBGM##ConfigWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoCollapse
      )
    {
      this.configuration = configuration;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(300, 0),
        MaximumSize = new Vector2(800, 1000)
      };
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    public override void OnClose()
    {
      base.OnClose();
      configuration.IsVisible = false;
      configuration.Save();
    }

    private void DrawSection(string headerText, List<ConditionAction> actions, bool defaultValue)
    {
      ImGui.PushID(headerText);
      if (ImGui.CollapsingHeader(headerText))
      {
        ImGui.Indent();
        foreach (var condition in actions)
        {
          ImGui.PushID($"{condition.Condition}");
          ImGui.Text($"{condition.Condition}");
          ImGui.SameLine();
          var value = condition.Value;
          if (ImGui.Checkbox("##value", ref value))
          {
            condition.Value = value;
            configuration.Save();
          }
          ImGui.SameLine();
          if (ImGui.Button("X"))
          {
            if (actions.Remove(condition))
            {
              configuration.Save();
            }
          }
          ImGui.PopID();
        }

        ImGui.Separator();

        ImGui.Text("Add new condition:");
        var selected = 0;
        if (ImGui.Combo("##ConditionFlag", ref selected, conditionNames, conditionNames.Length))
        {
          var newCondition = (ConditionFlag)Enum.Parse(typeof(ConditionFlag), conditionNames[selected]);
          actions.Add(new ConditionAction { Condition = newCondition, Value = defaultValue });
          configuration.Save();
        }

        ImGui.Unindent();
      }
    }

    private void EnableSection()
    {
      DrawSection("Conditions that Enable BGM", configuration.EnableConditions, true);
    }

    private void DisableSection()
    {
      DrawSection("Conditions that Disable BGM", configuration.DisableConditions, false);
    }

    public override void Draw()
    {
      EnableSection();
      ImGui.Separator();
      DisableSection();
    }
  }
}
