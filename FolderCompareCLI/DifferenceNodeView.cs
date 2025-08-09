using FolderCompareCLI.Enums;
using FolderCompareCLI.Model;
using FolderCompareCLI.Utils;

namespace FolderCompareCLI;

/// <summary>
/// This is tightly bound to console implementation currently I am not a huge fan but YAGNI for the moment of refactoring it out
/// </summary>
internal class DifferenceNodeView
{
    public DifferenceNodeView(DifferenceNode differenceNode, string sourcePath, string destinationPath)
    {
        Differences = differenceNode.Differences;
        switch (differenceNode.Differences)
        {
            case Differences.DirInSourceNotInDest:
                Options = DirectoryActions(differenceNode.Source.AsT0.FullPath,
                    Path.Combine(destinationPath,
                        FileAndIoUtils.GetRelativePath(differenceNode.Source.AsT0.FullPath, sourcePath)));

                DisplayText = $"Directory in source but not in destination\n\t{differenceNode.Source.AsT0.FullPath}";
                ConsoleColor = ConsoleColor.Green;
                break;
            case Differences.DirInDestNotInSource:
                Options = DirectoryActions(differenceNode.Destination.AsT0.FullPath,
                    Path.Combine(sourcePath,
                        FileAndIoUtils.GetRelativePath(differenceNode.Destination.AsT0.FullPath, destinationPath)));
                DisplayText =
                    $"Directory in destination but not in source\n\t{differenceNode.Destination.AsT0.FullPath}";
                ConsoleColor = ConsoleColor.Red;
                break;
            case Differences.FileInSourceNotInDest:
                Options = MissingFileActions(differenceNode.Source.AsT1.FullPath,
                    Path.Combine(destinationPath,
                        FileAndIoUtils.GetRelativePath(differenceNode.Source.AsT1.FullPath, sourcePath)));

                DisplayText = $"File in source but not in destination\n\t{differenceNode.Source.AsT1.FullPath}";
                ConsoleColor = ConsoleColor.Green;
                break;
            case Differences.FileInDestNotInSource:
                Options = MissingFileActions(differenceNode.Destination.AsT1.FullPath,
                    Path.Combine(sourcePath,
                        FileAndIoUtils.GetRelativePath(differenceNode.Destination.AsT1.FullPath, destinationPath)));

                DisplayText = $"File in destination but not in source\n\t{differenceNode.Destination.AsT1.FullPath}";
                ConsoleColor = ConsoleColor.Red;
                break;
            case Differences.FileMissMatch:
                DisplayText =
                    $"File miss match {FileAndIoUtils.GetRelativePath(differenceNode.Destination.AsT1.FullPath, destinationPath)}\n" +
                    $"\tSource:{FileAndIoUtils.BytesToString(differenceNode.Source.AsT1.Size)}\n" +
                    $"\tDestination:{FileAndIoUtils.BytesToString(differenceNode.Destination.AsT1.Size)}\n";
                ConsoleColor = ConsoleColor.Yellow;
                Options =
                [
                    new NodeAction
                    {
                        Text = "Delete in both", Action = () =>
                        {
                            File.Delete(differenceNode.Destination.AsT1.FullPath);
                            File.Delete(differenceNode.Source.AsT1.FullPath);
                        },
                        ActionDetails = ActionDetails.Delete
                    },

                    new NodeAction
                    {
                        Text = "Copy source to destination", Action = () =>
                        {
                            File.Delete(differenceNode.Destination.AsT1.FullPath);
                            File.Copy(differenceNode.Source.AsT1.FullPath, differenceNode.Destination.AsT1.FullPath);
                        },
                        ActionDetails = ActionDetails.CopyOneWay
                    },

                    new NodeAction
                    {
                        Text = "Copy destination to source", Action = () =>
                        {
                            File.Delete(differenceNode.Source.AsT1.FullPath);
                            File.Copy(differenceNode.Destination.AsT1.FullPath, differenceNode.Source.AsT1.FullPath);
                        },
                        ActionDetails = ActionDetails.CopyOneWay
                    }
                ];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(differenceNode.Differences),
                    differenceNode.Differences,
                    null);
        }
    }

    public Guid Id { get; } = Guid.NewGuid();

    public List<NodeAction> Options { get; }

    public string DisplayText { get; }

    public ConsoleColor ConsoleColor { get; }

    public Differences Differences { get; }

    private static List<NodeAction> DirectoryActions(string path, string dest) =>
    [
        new() { Text = "Delete", Action = () => Directory.Delete(path, true), ActionDetails = ActionDetails.Delete, },
        new() { Text = "Copy over", Action = () => FileAndIoUtils.DirectoryCopy(path, dest), ActionDetails = ActionDetails.CopyOver }
    ];

    private static List<NodeAction> MissingFileActions(string path, string dest) =>
    [
        new() { Text = "Delete", Action = () => File.Delete(path), ActionDetails = ActionDetails.Delete, },
        new() { Text = "Copy over", Action = () => File.Copy(path, dest), ActionDetails = ActionDetails.CopyOver }
    ];
}