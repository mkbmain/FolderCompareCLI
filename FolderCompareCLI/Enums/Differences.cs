namespace FolderCompareCLI.Enums;

public enum Differences
{
    DirInSourceNotInDest,
    FileInSourceNotInDest,
    DirInDestNotInSource,
    FileInDestNotInSource,
    FileMissMatch,
}