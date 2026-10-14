
using Avalonia.Metadata;
using DockaloniaMinimal;

[assembly: XmlnsDefinition(XmlnsConstants.NAMESPACE, "DockaloniaMinimal.Previews")]
[assembly: XmlnsDefinition(XmlnsConstants.NAMESPACE, "DockaloniaMinimal.TabDragging")]
[assembly: XmlnsDefinition(XmlnsConstants.NAMESPACE, "DockaloniaMinimal.Tabbing")]

namespace DockaloniaMinimal;

internal static class XmlnsConstants
{
    public const string NAMESPACE = "TheDockingProject/dockalonia";
}