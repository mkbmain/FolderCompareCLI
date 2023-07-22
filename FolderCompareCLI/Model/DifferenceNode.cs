using FolderCompareCLI.Enums;
using OneOf;

namespace FolderCompareCLI.Model;

internal sealed record DifferenceNode(OneOf<FolderNode, FileNode> Source, OneOf<FolderNode, FileNode> Destination, Differences Differences);