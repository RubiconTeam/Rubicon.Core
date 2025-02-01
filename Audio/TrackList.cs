using System.Linq;
using Godot.Collections;

namespace Rubicon.Core.Audio;

[GlobalClass] public partial class TrackList : Node
{
    [Export] public AudioStreamPlayer Player;

    [Export] public float Time
    {
        get => Player.GetPlaybackPosition();
        set => Player.Seek(value);
    }
    
    [Export] public StringName Bus;

    [Export] public Array<AudioStreamPlayer> SubPlayers;

    private Dictionary<AudioStreamPlayer, bool> _autoDestroyMap;

    public void Setup(StringName bus)
    {
        Bus = bus;
        
        Player = new AudioStreamPlayer();
        Player.Autoplay = false;
        Player.Bus = bus;
        AddChild(Player);
        
        SubPlayers = new Array<AudioStreamPlayer>();
        _autoDestroyMap = new Dictionary<AudioStreamPlayer, bool>();
    }

    public AudioStreamPlayer Play(AudioStream stream, float time = 0f)
    {
        Player.Stream = stream;
        Player.Play(time);
        return Player;
    }

    public void Pause(bool includeSubPlayers = false)
    {
        Player.StreamPaused = true;
        if (!includeSubPlayers)
            return;
        
        for (int i = 0; i < SubPlayers.Count; i++)
            SubPlayers[i].StreamPaused = true;
    }
    
    public void Resume(bool includeSubPlayers = false)
    {
        Player.StreamPaused = false;
        if (!includeSubPlayers)
            return;
        
        for (int i = 0; i < SubPlayers.Count; i++)
            SubPlayers[i].StreamPaused = false;
    }

    public void Stop()
    {
        Player.Stop();
        for (int i = 0; i < SubPlayers.Count; i++)
            SubPlayers[i].Stop();
    }

    public AudioStreamPlayer AddSubTrack(AudioStream stream, bool autoStart = false, bool autoDestroy = true)
    {
        AudioStreamPlayer player = new AudioStreamPlayer();
        player.Stream = stream;
        return AddSubPlayer(player, autoStart, autoDestroy);
    }

    public AudioStreamPlayer AddSubPlayer(AudioStreamPlayer player, bool autoStart = false, bool autoDestroy = true)
    {
        if (SubPlayers.Contains(player))
            return player;

        player.Bus = Bus;
        player.Autoplay = false;
        SubPlayers.Add(player);
        AddChild(player);
        
        if (autoStart)
            player.Play();

        if (autoDestroy)
        {
            _autoDestroyMap.Add(player, true);
            player.Finished += () => AutoDestroyPlayer(player);
        }

        return player;
    }

    public void RemoveSubTrack(AudioStream stream, bool free = false)
    {
        AudioStreamPlayer player = SubPlayers.FirstOrDefault(x => x.Stream == stream);
        if (player == null)
            return;
        
        RemoveSubPlayer(player, free);
    }

    public void RemoveSubPlayer(AudioStreamPlayer player, bool free = false)
    {
        _autoDestroyMap.Remove(player);
        SubPlayers.Remove(player);
        
        if (free)
            player.QueueFree();
    }

    private void AutoDestroyPlayer(AudioStreamPlayer player)
    {
        if (!_autoDestroyMap[player])
            return;

        RemoveSubPlayer(player, true);
    }
}