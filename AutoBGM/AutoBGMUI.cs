using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
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
      var toRemove = new List<ConditionAction>();

      ImGui.PushID(headerText);

      var isOpen = ImGui.CollapsingHeader(headerText);
      ImGui.SameLine();
      ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 30);
      // right align:
      // Define the size and position of the circle
      Vector2 circlePos = ImGui.GetCursorScreenPos() + new Vector2(0, 1); // Get the current cursor position
      float radius = 10.0f;  // Set the circle radius
      uint circleColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.6f, 0.6f, 0.6f, 1.0f)); // Gray color

      // Get the current draw list for custom drawing
      var drawList = ImGui.GetWindowDrawList();

      // Draw the filled circle
      drawList.AddCircleFilled(new Vector2(circlePos.X + radius, circlePos.Y + radius), radius, circleColor);

      // Move the cursor inside the circle to draw the "?"
      ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 23); // Offset a bit to center the "?"
      ImGui.Text("?"); // Draw the "?" inside the circle
      ImGui.SameLine();
      ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 30); // Offset a bit to center the "?"

      // Check if the mouse is hovering over the circle and display the tooltip
      ImGui.InvisibleButton("help_icon", new Vector2(radius * 2, radius * 2)); // Create an invisible button for hover detection
      if (ImGui.IsItemHovered())
      {
        ImGui.BeginTooltip();

        ImGui.PushTextWrapPos(300f); // Tooltip Width
        var text = $"BGM will be {(defaultValue ? "enabled" : "disabled")} when:\n";
        var and = false;
        foreach (var condtion in actions)
        {
          if (and)
          {
            text += " or ";
          }
          else
          {
            and = true;
          }
          text += $"{(condtion.Value ? "" : "not ")}{condtion.Condition}";
        }
        ImGui.TextWrapped(text);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
      }

      if (isOpen)
      {
        ImGui.Indent();
        ImGui.BeginTable(headerText, 3);

        ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed, 30);
        ImGui.TableSetupColumn("Remove", ImGuiTableColumnFlags.WidthFixed, 20);

        foreach (var cav in actions)
        {
          ImGui.PushID($"{cav.Condition}");
          ImGui.TableNextColumn();
          ImGui.Text($"{cav.Condition}");
          ImGui.TableNextColumn();
          var value = cav.Value;
          if (ImGui.Checkbox("##value", ref value))
          {
            cav.Value = value;
            configuration.Save();
          }
          ImGui.TableNextColumn();
          if (ImGui.Button("X"))
          {
            toRemove.Add(cav);
          }
          ImGui.PopID();
        }
        ImGui.EndTable();

        ImGui.Text("Add new condition:");
        var selected = 0;
        if (ImGui.Combo("##ConditionFlag", ref selected, conditionNames, conditionNames.Length))
        {
          var newCondition = (ConditionFlag)Enum.Parse(typeof(ConditionFlag), conditionNames[selected]);
          if (!actions.Any(c => c.Condition == newCondition))
          {
            actions.Add(new ConditionAction { Condition = newCondition, Value = defaultValue });
            configuration.Save();
          }
        }

      }

      var mustSave = false;
      foreach (var cav in toRemove)
      {
        mustSave |= actions.Remove(cav);
      }

      if (mustSave)
      {
        configuration.Save();
      }

      ImGui.Unindent();
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
