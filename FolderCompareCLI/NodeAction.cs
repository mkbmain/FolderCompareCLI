using FolderCompareCLI.Enums;

namespace FolderCompareCLI;

public class NodeAction
{
    public Action Action { get; set; }

    public string Text { get; set; }

    public ActionDetails ActionDetails { get; set; }
}