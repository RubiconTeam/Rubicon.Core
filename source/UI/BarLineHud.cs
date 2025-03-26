using System.Linq;
using Godot.Collections;
using Rubicon.Core.API;
using Rubicon.Core.Rulesets;

namespace Rubicon.Core.UI;

/// <summary>
/// Defines a <see cref="Control"/> node that is attached to a <see cref="BarLine"/>.
/// There are pre-defined references to certain common HUD elements for use in song scripting, however if needs be, you can use the built-in metadata to define anything else you may need.
/// </summary>
[GlobalClass] public partial class BarLineHud : Control
{
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

    private bool _flipped = false;
    
    public void Setup(BarLine barLine, PlayField playField)
    {
        barLine.AddChild(this);
        InitializeChildren(GetChildren(), playField);
    }
    
    /// <summary>
    /// Flips its own children's anchors.
    /// </summary>
    /// <param name="flip">Whether to flip the elements on this HUD.</param>
    public void Flip(bool flip)
    {
        if (_flipped == flip)
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

        _flipped = flip;
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