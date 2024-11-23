namespace Rubicon.Core.Events;

/// <summary>
/// <see cref="Resource"/> that stores multiple instances of <see cref="EventData"/>.
/// </summary>
[GlobalClass] public partial class EventMeta : Resource
{
    /// <summary>
    /// A container storing all of its events.
    /// </summary>
    [Export] public EventData[] Events = [];

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

    /// <summary>
    /// Makes any corrections to this EventMeta that are necessary.
    /// </summary>
    public void Format()
    {
        Array.Sort(Events, (x, y) =>
        {
            if (x.Time < y.Time)
                return -1;
            if (x.Time > y.Time)
                return 1;

            return 0;
        });
    }
    
    /// <summary>
    /// Gets the current index of this EventMeta.
    /// </summary>
    /// <returns>The current index</returns>
    public int GetIndex() => Index;
    
    /// <summary>
    /// Sets the current index.
    /// </summary>
    /// <param name="idx">The index to set</param>
    public void SetIndex(int idx) => Index = idx;
}