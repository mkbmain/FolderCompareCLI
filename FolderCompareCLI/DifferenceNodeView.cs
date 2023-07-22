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
        switch (differenceNode.Differences)
        {
            case Differences.DirInSourceNotInDest:
                Options = DirectoryActions(differenceNode.Source.AsT0.FullPath,
                    Path.Combine(destinationPath, FileAndIoUtils.GetRelativePath(differenceNode.Source.AsT0.FullPath, sourcePath)));

                DisplayText = $"Directory in source but not in destination\n\t{differenceNode.Source.AsT0.FullPath}";
                ConsoleColor = ConsoleColor.Green;
                break;
            case Differences.DirInDestNotInSource:
                Options = DirectoryActions(differenceNode.Destination.AsT0.FullPath,
                    Path.Combine(sourcePath, FileAndIoUtils.GetRelativePath(differenceNode.Destination.AsT0.FullPath, destinationPath)));
                DisplayText = $"Directory in destination but not in source\n\t{differenceNode.Destination.AsT0.FullPath}";
                ConsoleColor = ConsoleColor.Red;
                break;
            case Differences.FileInSourceNotInDest:
                Options = MissingFileActions(differenceNode.Source.AsT1.FullPath,
                    Path.Combine(destinationPath, FileAndIoUtils.GetRelativePath(differenceNode.Source.AsT1.FullPath, sourcePath)));

                DisplayText = $"File in source but not in destination\n\t{differenceNode.Source.AsT1.FullPath}";
                ConsoleColor = ConsoleColor.Green;
                break;
            case Differences.FileInDestNotInSource:
                Options = MissingFileActions(differenceNode.Destination.AsT1.FullPath,
                    Path.Combine(sourcePath, FileAndIoUtils.GetRelativePath(differenceNode.Destination.AsT1.FullPath, destinationPath)));

                DisplayText = $"File in destination but not in source\n\t{differenceNode.Destination.AsT1.FullPath}";
                ConsoleColor = ConsoleColor.Red;
                break;
            case Differences.FileMissMatch:
                DisplayText = $"File miss match {FileAndIoUtils.GetRelativePath(differenceNode.Destination.AsT1.FullPath, destinationPath)}\n" +
                              $"\tSource:{FileAndIoUtils.BytesToString(differenceNode.Source.AsT1.Size)}\n" +
                              $"\tDestination:{FileAndIoUtils.BytesToString(differenceNode.Destination.AsT1.Size)}\n";
                ConsoleColor = ConsoleColor.Yellow;
                Options = new List<KeyValuePair<string, Action>>
                {
                    new("Delete in both", () =>
                    {
                        File.Delete(differenceNode.Destination.AsT1.FullPath);
                        File.Delete(differenceNode.Source.AsT1.FullPath);
                    }),
                    new("Copy source to destination", () =>
                    {
                        File.Delete(differenceNode.Destination.AsT1.FullPath);
                        File.Copy(differenceNode.Source.AsT1.FullPath, differenceNode.Destination.AsT1.FullPath);
                    }),
                    new("Copy destination to source", () =>
                    {
                        File.Delete(differenceNode.Source.AsT1.FullPath);
                        File.Copy(differenceNode.Destination.AsT1.FullPath, differenceNode.Source.AsT1.FullPath);
                    }),
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(differenceNode.Differences), differenceNode.Differences, null);
        }
    }

    public Guid Id { get; } = Guid.NewGuid();

    public DifferenceNode Node { get; }

    public List<KeyValuePair<string, Action>> Options { get; }

    public string DisplayText { get; }

    public ConsoleColor ConsoleColor { get; }


    private static List<KeyValuePair<string, Action>> DirectoryActions(string path, string dest)
    {
        return new List<KeyValuePair<string, Action>>
        {
            new("Delete", () => Directory.Delete(path, true)),
            new("Copy over", () => FileAndIoUtils.DirectoryCopy(path, dest))
        };
    }

    private static List<KeyValuePair<string, Action>> MissingFileActions(string path, string dest)
    {
        return new List<KeyValuePair<string, Action>>
        {
            new("Delete", () => File.Delete(path)),
            new("Copy over", () => File.Copy(path, dest))
        };
    }
}