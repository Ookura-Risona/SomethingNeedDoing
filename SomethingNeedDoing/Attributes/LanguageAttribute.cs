namespace SomethingNeedDoing.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class LanguageAttribute(string extension) : Attribute
{
    public string Extension { get; } = extension;
}
