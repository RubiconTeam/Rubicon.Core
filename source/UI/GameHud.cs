using System.Linq;
using Godot.Collections;
using Rubicon.Core.Rulesets;
using Node = Godot.Node;

namespace Rubicon.Core.UI;

/// <summary>
/// Defines a <see cref="Control"/> node with certain settings for the heads-up-display.
/// There are pre-defined references to certain common HUD elements for use in song scripting, however if needs be, you can use the built-in metadata to define anything else you may need.
/// </summary>
[GlobalClass] public partial class GameHud : Control
{
    /// <summary>
    /// The node representing the health bar. This isn't necessary, but it's nice to have a reference when scripting.
    /// </summary>
    [Export] public Node HealthBar;
    
    /// <summary>
    /// The node representing the score panel. This isn't necessary, but it's nice to have a reference when scripting.
    /// </summary>
    [Export] public Node ScorePanel;

    /// <summary>
    /// The node representing the timer bar. This isn't necessary, but it's nice to have a reference when scripting.
    /// </summary>
    [Export] public Node TimerBar;

    /// <summary>
    /// The node representing the judgment. This isn't necessary, but it's nice to have a reference when scripting.
    /// </summary>
    [Export] public Node Judgment;
    
    /// <summary>
    /// The node representing the combo. This isn't necessary, but it's nice to have a reference when scripting.
    /// </summary>
    [Export] public Node Combo;
    
    /// <summary>
    /// The node representing the hit distance. This isn't necessary, but it's nice to have a reference when scripting.
    /// </summary>
    [Export] public Node HitDistance;
    
    /// <summary>
    /// If a child is placed in this array, it is ignored in <see cref="Flip"/>.
    /// </summary>
    [Export] public Node[] UpdateExceptions = [];

    private bool _flipVertical = false;

    public void Setup(PlayField playField)
    {
        InitializeChildren(GetChildren(), playField);
    }

    /// <summary>
    /// Flips its own children's anchors vertically.
    /// </summary>
    /// <param name="flip">Whether to flip the elements on this HUD.</param>
    public void FlipVertical(bool flip)
    {
        if (_flipVertical == flip)
            return;
        
        Array<Node> children = GetChildren();
        for (int i = 0; i < children.Count; i++)
        {
            Node curChild = children[i];
            if (UpdateExceptions.Contains(curChild))
                continue;
            
            if (curChild is not Control control)
                continue;

            float anchorBottom = control.AnchorBottom;
            float anchorTop = control.AnchorTop;

            control.AnchorBottom = 1f - anchorTop;
            control.AnchorTop = 1f - anchorBottom;

            float offsetBottom = control.OffsetBottom;
            float offsetTop = control.OffsetTop;

            control.OffsetBottom = offsetTop * -1f;
            control.OffsetTop = offsetBottom * -1f;
        }

        _flipVertical = flip;
    }

    private void InitializeChildren(Array<Node> children, PlayField playField)
    {
        for (int i = 0; i < children.Count; i++)
        {
            Node curChild = children[i];
            
            InitializeChildren(curChild.GetChildren(), playField);
            playField.InitializeGodotScript(curChild);
        }
    }
}