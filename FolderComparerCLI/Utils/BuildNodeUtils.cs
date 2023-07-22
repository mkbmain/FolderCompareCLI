using FolderComparerCLI.Enums;
using FolderComparerCLI.Model;

namespace FolderComparerCLI.Utils;

internal static class BuildNodeUtils
{
    public static (FolderNode src, FolderNode des) BuildFolderPaths(string src, string destination) => (GetFolderNode(src), GetFolderNode(destination));

    public static FolderNode GetFolderNode(string path) =>
        new(
            path.TrimEnd(FileAndIoUtils.DirectorySeparator).Split(FileAndIoUtils.DirectorySeparator).Last(),
            path,
            Directory.GetDirectories(path).Select(GetFolderNode).ToList(), GetFiles(path));


    private static IList<FileNode> GetFiles(string path) => Directory.GetFiles(path)
        .Select(w => new FileInfo(w))
        .Select(q => new FileNode(q.Name, q.FullName, q.Length)).ToList();


    public static IEnumerable<DifferenceNode> CalculateDifferences(FolderNode source, FolderNode dest, bool calcHash)
    {
        var list = new List<DifferenceNode>();
        var lookupDirectories = dest.SubDirectories.ToDictionary(w => w.Name, w => (Used: false, Node: w));

        foreach (var subDirectory in source.SubDirectories)
        {
            if (!lookupDirectories.TryGetValue(subDirectory.Name, out var matchDirectory))
            {
                list.Add(new DifferenceNode(subDirectory, (FolderNode)null, Differences.DirInSourceNotInDest));
                continue;
            }

            list.AddRange(CalculateDifferences(subDirectory, matchDirectory.Node, calcHash));
            lookupDirectories[subDirectory.Name] = (true, matchDirectory.Node);
        }

        list.AddRange(lookupDirectories.Where(w => !w.Value.Used).Select(w => new DifferenceNode((FolderNode)null, w.Value.Node, Differences.DirInDestNotInSource)));

        list.AddRange(CalcDifferencesInFiles(source.Files, dest.Files, calcHash));
        return list;
    }

    private static IEnumerable<DifferenceNode> CalcDifferencesInFiles(IEnumerable<FileNode> source, IEnumerable<FileNode> dest, bool calcHash)
    {
        var list = new List<DifferenceNode>();
        var lookupFiles = dest.ToDictionary(w => w.Name, w => (Used: false, Node: w));
        foreach (var file in source)
        {
            if (!lookupFiles.TryGetValue(file.Name, out var matchFile))
            {
                list.Add(new DifferenceNode(file, (FileNode)null, Differences.FileInSourceNotInDest));
                continue;
            }

            if (file.Size != matchFile.Node.Size || (calcHash && FileAndIoUtils.CalculateMd5(matchFile.Node.FullPath) != FileAndIoUtils.CalculateMd5(file.FullPath)))
            {
                list.Add(new DifferenceNode(file, matchFile.Node, Differences.FileMissMatch));
            }

            lookupFiles[file.Name] = (true, matchFile.Node);
        }

        list.AddRange(lookupFiles.Where(w => !w.Value.Used).Select(w => new DifferenceNode((FileNode)null, w.Value.Node, Differences.FileInDestNotInSource)));
        return list;
    }
}