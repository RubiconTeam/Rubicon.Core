namespace Rubicon.Core.Rulesets;

[GlobalClass] public partial class NoteSkin : Resource
{
    /// <summary>
    /// The scale used when generating notes and lanes.
    /// </summary>
    [Export] public Vector2 Scale = Vector2.One;
	
    /// <summary>
    /// The filtering used when generating notes and lanes.
    /// </summary>
    [Export] public CanvasItem.TextureFilterEnum Filter = CanvasItem.TextureFilterEnum.Linear;
}