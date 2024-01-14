using FolderCompareCLI.Enums;
using FolderCompareCLI.Model;

namespace FolderCompareCLI.Utils;

internal static class BuildNodeUtils
{
    public static (FolderNode src, FolderNode des) BuildFolderPaths(string src, string destination) => (GetFolderNode(src), GetFolderNode(destination));

    private static long _checked;

    public static string CheckedSoFar => _checked == 0 ? "Building Tree" : FileAndIoUtils.BytesToString(_checked);

    private static FolderNode GetFolderNode(string path) =>
        new(
            path.TrimEnd(FileAndIoUtils.DirectorySeparator).Split(FileAndIoUtils.DirectorySeparator).Last(),
            path,
            Directory.GetDirectories(path).Select(GetFolderNode).ToList(), GetFiles(path));


    private static List<FileNode> GetFiles(string path) => Directory.GetFiles(path)
        .Select(w => new FileInfo(w))
        .Select(q => new FileNode(q.Name, q.FullName, q.Length)).ToList();


    public static IEnumerable<DifferenceNode> CalculateDifferences(FolderNode source, FolderNode dest, bool deepCheck, bool hash)
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

            list.AddRange(CalculateDifferences(subDirectory, matchDirectory.Node, deepCheck, hash));
            lookupDirectories[subDirectory.Name] = (true, matchDirectory.Node);
        }

        list.AddRange(lookupDirectories.Where(w => !w.Value.Used).Select(w => new DifferenceNode((FolderNode)null, w.Value.Node, Differences.DirInDestNotInSource)));

        list.AddRange(CalcDifferencesInFiles(source.Files, dest.Files, deepCheck, hash));
        return list;
    }

    private const int ReportValue = 25 * 1000 * 1000; // don't report files under 25MB approx they should be so quick not worth printing out
    private static int _lastStringLen;

    private static IEnumerable<DifferenceNode> CalcDifferencesInFiles(IEnumerable<FileNode> source, IEnumerable<FileNode> dest, bool deepCheck, bool hash)
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

            if (hash || deepCheck && file.Size > ReportValue)
            {
                Console.SetCursorPosition(0, 3);
                var line = $"{file.Name} -- {FileAndIoUtils.BytesToString(file.Size)}";
                Console.Write(line);
                if (line.Length < _lastStringLen)
                {
                    Console.Write(new string(' ', _lastStringLen - line.Length));
                }

                _lastStringLen = line.Length;
            }

            if (file.Size != matchFile.Node.Size ||
                (deepCheck && FileAndIoUtils.DeepFileCheck(matchFile.Node.FullPath, file.FullPath)) ||
                (hash && FileAndIoUtils.CalculateMd5(matchFile.Node.FullPath) != FileAndIoUtils.CalculateMd5(file.FullPath)))
            {
                list.Add(new DifferenceNode(file, matchFile.Node, Differences.FileMissMatch));
            }

            _checked += file.Size;
            lookupFiles[file.Name] = (true, matchFile.Node);
        }

        list.AddRange(lookupFiles.Where(w => !w.Value.Used).Select(w => new DifferenceNode((FileNode)null, w.Value.Node, Differences.FileInDestNotInSource)));
        return list;
    }
}