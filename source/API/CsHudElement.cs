using Rubicon.Core.Rulesets;

namespace Rubicon.Core.API;

[GlobalClass] public abstract partial class CsHudElement : Control, IPlayElement
{
    /// <inheritdoc/>
    public PlayField PlayField { get; set; }

    public abstract void Initialize();
}