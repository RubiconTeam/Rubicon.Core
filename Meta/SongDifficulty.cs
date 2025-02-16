using Rubicon.Core.Chart;

namespace Rubicon.Core.Meta;

[GlobalClass] public partial class SongDifficulty : Resource
{
    [Export] public string Name = "Normal";

    [Export] public RubiChart Chart;
    
    [Export] public string RuleSet = "mania";

    [Export] public Color Color = Colors.MediumPurple;
}