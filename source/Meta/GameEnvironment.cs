namespace Rubicon.Core.Meta;

/// <summary>
/// Which type of environment the song's stage will spawn in.
/// </summary>
public enum GameEnvironment : uint
{
    /// <summary>
    /// Spawns a stage that can only display <see cref="Node"/>s deriving from <see cref="Godot.CanvasItem"/>.
    /// </summary>
    CanvasItem,
    
    /// <summary>
    /// Spawns a stage that can only display <see cref="Node"/>s deriving from <see cref="Node3D"/>.
    /// </summary>
    Spatial
}