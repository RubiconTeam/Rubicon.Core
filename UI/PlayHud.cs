using System.Linq;
using Rubicon.Core.Data;
using Godot.Collections;

namespace Rubicon.Core.UI;

/// <summary>
/// Defines a <see cref="Control"/> node with certain settings for the heads-up-display.
/// </summary>
[GlobalClass] public partial class PlayHud : Control
{
    /// <summary>
    /// If a child is placed in this array, it is ignored in <see cref="UpdatePosition"/>.
    /// </summary>
    [Export] public Node[] UpdateExceptions = [];

    private bool _inDownScrollPositions = false;

    /// <summary>
    /// Updates the children's anchors depending on if the game is in down scroll or not.
    /// </summary>
    /// <param name="downScroll"></param>
    public void UpdatePosition(bool downScroll)
    {
        if (_inDownScrollPositions == downScroll)
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

        _inDownScrollPositions = downScroll;
    }
}