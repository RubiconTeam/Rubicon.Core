using Rubicon.Core.Chart;

namespace Rubicon.Core.Meta;

[GlobalClass] public partial class SongDifficulty : Resource
{
    [Export] public string Name = "Normal";

    [Export] public RubiChart Chart;
    
    [Export] public string RuleSet = ProjectSettings.GetSetting("rubicon/rulesets/default_ruleset").AsString();

    [Export] public Color Color = Colors.MediumPurple;
}