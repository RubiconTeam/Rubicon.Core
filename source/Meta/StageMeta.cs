namespace Rubicon.Core.Meta;

[GlobalClass] public partial class StageMeta : Resource
{
    [Export] public string Stage = "";

    /// <summary>
    /// Determines what type of backend the engine will use when loading this stage.
    /// </summary>
    [Export] public GameEnvironment Environment = GameEnvironment.CanvasItem;
}