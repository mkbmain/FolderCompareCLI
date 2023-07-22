namespace FolderComparerCLI.Model;

internal sealed record FolderNode(string Name, string FullPath, IList<FolderNode> SubDirectories, IList<FileNode> Files) :
    FileNode(Name, FullPath, Files.Sum(w => w.Size) + Files.Sum(q => q.Size));