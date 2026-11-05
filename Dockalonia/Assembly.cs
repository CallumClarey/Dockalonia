
using Avalonia.Metadata;
using Dockalonia;

[assembly: XmlnsDefinition(XmlnsConstants.NAMESPACE, "Dockalonia.Previews")]
[assembly: XmlnsDefinition(XmlnsConstants.NAMESPACE, "Dockalonia.TabDragging")]
[assembly: XmlnsDefinition(XmlnsConstants.NAMESPACE, "Dockalonia.Tabbing")]
[assembly: XmlnsDefinition(XmlnsConstants.NAMESPACE, "Dockalonia.Testing")]
namespace Dockalonia;

internal static class XmlnsConstants
{
    public const string NAMESPACE = "TheDockingProject/dockalonia";
}