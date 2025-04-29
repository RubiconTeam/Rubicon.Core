namespace Rubicon.Core.Data;

/// <summary>
/// A resource for version identifying.
/// </summary>
[GlobalClass]
public partial class VersionInfo : Resource // Would be super useful if custom structs were supported in Godot
{
    /// <summary>
    /// The major version. Example: (<see cref="Major"/>.<see cref="Minor"/>.<see cref="Patch"/>.<see cref="Build"/>)
    /// </summary>
    [Export] public byte Major
    {
        get => (byte)((_version & 0xFF000000) >> 24);
        set
        {
            if (!Engine.IsEditorHint() && _majorInitialized)
                return;
            
            _version = ((uint)value << 24) | ((uint)Minor << 16) | ((uint)Patch << 8) | Build;
            _majorInitialized = true;
        }
    }
    
    /// <summary>
    /// The minor version. Example: (<see cref="Major"/>.<see cref="Minor"/>.<see cref="Patch"/>.<see cref="Build"/>)
    /// </summary>
    [Export] public byte Minor
    {
        get => (byte)((_version & 0x00FF0000) >> 16);
        set
        {
            if (!Engine.IsEditorHint() && _minorInitialized)
                return;
            
            _version = ((uint)Major << 24) | ((uint)value << 16) | ((uint)Patch << 8) | Build;
            _minorInitialized = true;
        }
    }
    
    /// <summary>
    /// The patch version. Example: (<see cref="Major"/>.<see cref="Minor"/>.<see cref="Patch"/>.<see cref="Build"/>)
    /// </summary>
    [Export] public byte Patch
    {
        get => (byte)((_version & 0x0000FF00) >> 8);
        set
        {
            if (!Engine.IsEditorHint() && _patchInitialized)
                return;
            
            _version = ((uint)Major << 24) | ((uint)Minor << 16) | ((uint)value << 8) | Build;
            _patchInitialized = true;
        }
    }
    
    /// <summary>
    /// The build version. Example: (<see cref="Major"/>.<see cref="Minor"/>.<see cref="Patch"/>.<see cref="Build"/>)
    /// </summary>
    [Export] public byte Build
    {
        get => (byte)(_version & 0x000000FF);
        set
        {
            if (!Engine.IsEditorHint() && _buildInitialized)
                return;

            _version = ((uint)Major << 24) | ((uint)Minor << 16) | ((uint)Patch << 8) | value;
            _buildInitialized = true;
        }
    }

    /// <summary>
    /// The tag added on towards the end of the version.
    /// </summary>
    [Export] public string Tag;

    /// <summary>
    /// The raw version. Mainly useful for comparison's sake.
    /// </summary>
    public uint Raw
    {
        get => _version;
        set
        {
            if (!Engine.IsEditorHint())
                return;
            
            _version = value;

            Major = (byte)((_version & 0xFF000000) >> 24);
            Minor = (byte)((_version & 0x00FF0000) >> 16);
            Patch = (byte)((_version & 0x0000FF00) >> 8);
            Build = (byte)(_version & 0x000000FF);
        }
    }

    private uint _version = 0;
    
    private bool _majorInitialized = false;
    private bool _minorInitialized = false;
    private bool _patchInitialized = false;
    private bool _buildInitialized = false;

    /// <summary>
    /// Creates an instance with all variables zeroed or empty.
    /// </summary>
    public VersionInfo()
    {
        Raw = 0;
        Tag = string.Empty;
    }

    /// <summary>
    /// Creates an instance with the specified raw version and tag.
    /// </summary>
    /// <param name="version">The raw version</param>
    /// <param name="tag">The tag</param>
    public VersionInfo(uint version, string tag = null)
    {
        Raw = version;
        Tag = tag ?? string.Empty;
    }

    /// <summary>
    /// Creates an instance with the specified versions and tag.
    /// </summary>
    /// <param name="major">The first number</param>
    /// <param name="minor">The second number</param>
    /// <param name="patch">The third number</param>
    /// <param name="build">The fourth number</param>
    /// <param name="tag">The tag added on</param>
    public VersionInfo(byte major, byte minor, byte patch, byte build, string tag = null)
    {
        Raw = ((uint)major << 24) | ((uint)minor << 16) | ((uint)patch << 8) | build;
        Tag = tag ?? string.Empty;
    }

    public uint GetRaw() => Raw;

    public override string ToString() => $"{Major}.{Minor}.{Patch}.{Build}{Tag}";
}