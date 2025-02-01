using Rubicon.Core.Rulesets;

namespace Rubicon.Core.API;

public interface IPlayElement
{
    /// <summary>
    /// A reference to the current PlayField. Automatically set by PlayField.
    /// </summary>
    public PlayField PlayField { get; set; }

    /// <summary>
    /// Triggers when added to <see cref="PlayField"/>.
    /// </summary>
    public void Initialize();
}