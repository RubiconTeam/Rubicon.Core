namespace Rubicon.Core.Events;

/// <summary>
/// <see cref="Resource"/> that stores multiple instances of <see cref="EventData"/>.
/// </summary>
[GlobalClass] public partial class EventMeta : Resource
{
    /// <summary>
    /// A container storing all of its events.
    /// </summary>
    [Export] public EventData[] Events;

    /// <summary>
    /// The current index.
    /// </summary>
    public int Index = 0;
    
    /// <summary>
    /// Gets the current event.
    /// </summary>
    /// <returns>The current event.</returns>
    public EventData Current() => Events[Index];

    /// <summary>
    /// Moves the index up by one and gives the corresponding event.
    /// </summary>
    /// <returns>An EventData if there is a next one, null if not.</returns>
    public EventData Next()
    {
        if (!HasNext())
            return null;
        
        Index++;
        return Events[Index];
    }
    
    /// <summary>
    /// Whether there is a next event in line.
    /// </summary>
    /// <returns>True if there is a next event, false if not.</returns>
    public bool HasNext() => Index < Events.Length;
}