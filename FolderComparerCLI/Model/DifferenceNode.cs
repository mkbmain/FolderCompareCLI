using FolderComparerCLI.Enums;
using OneOf;

namespace FolderComparerCLI.Model;

internal sealed record DifferenceNode(OneOf<FolderNode, FileNode> Source, OneOf<FolderNode, FileNode> Destination, Differences Differences);