namespace Rubicon.Core.Settings.Attributes;

/// <summary>
/// An attribute to automatically populate a field with a value from a Godot project setting.
/// </summary>
/// <remarks>
/// Note: Enum fields will be serialized as longs due to constraints with ConfigFile.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public class ProjectSettingAttribute : Attribute
{
    /// <summary>
    /// The reference to the key in <see cref="ProjectSettings"/>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectSettingAttribute"/> class.
    /// </summary>
    /// <param name="name">The path to the Godot project setting to bind to the field.</param>
    public ProjectSettingAttribute(string name)
    {
        Name = name;
    }
}