using SourceGenerator.Helper.CopyCode;

namespace TwitchDownloader.SourceGenerators.Attributes;

[Copy]
[AttributeUsage(AttributeTargets.Class)]
public sealed class StaticViewLocatorAttribute : Attribute;
