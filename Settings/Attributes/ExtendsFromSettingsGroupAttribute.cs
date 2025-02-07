namespace Rubicon.Core.Settings.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ExtendsFromSettingsGroup(Type type) : Attribute
{
    public Type Type { get; } = type;
}