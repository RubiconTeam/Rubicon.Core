using Godot.Collections;

namespace Rubicon.Core.UI;

[GlobalClass] public partial class UiStyle : Resource
{
    [Export] public Dictionary<StringName, RuleSetUiData> RuleSets = new();
    
    [Export] public PackedScene PauseMenu;
}