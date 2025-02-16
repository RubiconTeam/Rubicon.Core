using System.Collections.Generic;
using Rubicon.Core.API;
using Rubicon.Core.Rulesets;

namespace Rubicon.Core.Events;

/// <summary>
/// A class that helps to load, track and execute any song event.
/// </summary>
[GlobalClass] public partial class SongEventController : Node
{
    /// <summary>
    /// The index from the current event list.
    /// </summary>
    [Export] public int Index = 0;
    
    /// <summary>
    /// A signal that gets called everytime an event is executed.
    /// </summary>
    /// <param name="eventName">The name of the event being executed.</param>
    /// <param name="time">The time when the event has been executed.</param>
    /// <param name="args">The arguments of the event.</param>
    [Signal] public delegate void EventCalledEventHandler(StringName eventName, float time, Godot.Collections.Dictionary<StringName, Variant> args);

    [Export] private EventData[] _events = [];
    
    /// <summary>
    /// Sets up every event in the <see cref="EventMeta"/> file of the song.
    /// </summary>
    /// <param name="eventMeta">The data of every event in the song.</param>
    /// <param name="playField">The current <see cref="PlayField"/>.</param>
    public void Setup(EventMeta eventMeta, PlayField playField)
    {
        _events = eventMeta.Events;
        List<StringName> eventsInitialized = [];
        for (int i = 0; i < _events.Length; i++)
        {
            if (eventsInitialized.Contains(_events[i].Name))
                return;
            
            eventsInitialized.Add(_events[i].Name);

            string eventPath = $"res://resources/game/events/{_events[i].Name}";
            bool eventTscnExists = ResourceLoader.Exists(eventPath + ".tscn");
            bool eventScnExists = ResourceLoader.Exists(eventPath + ".scn");
            if (!eventTscnExists && !eventScnExists)
                continue;
            
            if (eventTscnExists)
                eventPath += ".tscn";
            else
                eventPath += ".scn";
            
            PackedScene eventScene = GD.Load<PackedScene>(eventPath);
            Node @event = eventScene.Instantiate();
            
            AddChild(@event);
            playField.InitializeGodotScript(@event);
        }
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Index >= _events.Length)
            return;
        
        EventData curEvent = _events[Index];
        if (Conductor.Time * 1000f >= curEvent.MsTime)
        {
            EmitSignalEventCalled(curEvent.Name, curEvent.Time, curEvent.Arguments);
            Index++;
        }
    }

    /// <summary>
    /// Resets the event list as well as its index.
    /// </summary>
    public void Reset()
    {
        Index = 0;
        _events = [];
    }
}