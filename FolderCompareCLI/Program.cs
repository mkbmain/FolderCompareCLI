using FolderCompareCLI.Model;
using FolderCompareCLI.Utils;

namespace FolderCompareCLI;

public class Program
{
    private const string HelpText = @"Arguments required expected args: {Source}  {Destination} {flags}
    Source : source path for comparison
    Destination: destination path for comparison
    
    Flags
        -hash enables hash check lot slower off by default
";

    private static Dictionary<Guid, DifferenceNodeView> _differenceNodeViews = null;

    private static readonly ConsoleColor ConsoleColor = Console.ForegroundColor;

    public static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine(HelpText);
            return;
        }

        var sourcePath = args[0].TrimEnd(FileAndIoUtils.DirectorySeparator);
        var destinationPath = args[1].TrimEnd(FileAndIoUtils.DirectorySeparator);
        var calcHash = args.Length > 2 && args[2] == "-hash";
        var sourcePathWithDirSeparator = $"{sourcePath}{FileAndIoUtils.DirectorySeparator}";
        var destinationPathWithDirSeparator = $"{destinationPath}{FileAndIoUtils.DirectorySeparator}";
        if (!StrIsPath(sourcePath) || !StrIsPath(destinationPath)) return;
        if (sourcePathWithDirSeparator.Contains(destinationPathWithDirSeparator) ||
            destinationPathWithDirSeparator.Contains(sourcePathWithDirSeparator))
        {
            Console.WriteLine("Recursive check.. Directories are the same or one is child\\subdirectory of the other");
            return;
        }

        Console.Clear();

        Exception exception = null;
        Task.Run(() =>
        {
            try
            {
                var (sourceFolderNodes, destinationFolderNodes) =
                    BuildNodeUtils.BuildFolderPaths(sourcePath, destinationPath);
                _differenceNodeViews = BuildNodeUtils
                    .CalculateDifferences(sourceFolderNodes, destinationFolderNodes, calcHash)
                    .Select(w => new DifferenceNodeView(w, sourcePath, destinationPath))
                    .ToDictionary(q => q.Id);
            }
            catch (Exception e)
            {
                exception = e;
            }
        });

        var previous = '/';
        while (_differenceNodeViews == null)
        {
            Console.SetCursorPosition(0, 0);
            previous = previous == '/' ? '\\' : '/';
            Console.Write($"{previous} Building data");
            await Task.Delay(200);
        }

        if (exception != null) throw exception;
        await DrawMainUi();
    }


    private static bool StrIsPath(string path)
    {
        if (Directory.Exists(path)) return true;
        Console.WriteLine(path + " is not a valid path");
        return false;
    }


    private static async Task DrawMainUi()
    {
        int pageNumber = 0;
        while (true)
        {
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            if (_differenceNodeViews.Any() == false)
            {
                Console.WriteLine("No differences");
                Console.ReadLine();
                return;
            }

            Console.Clear();
            var allowedNodes = (Console.BufferHeight - 2) / 4;
            allowedNodes = allowedNodes < 1 ? 1 : allowedNodes;
            var differenceNodes = _differenceNodeViews.Chunk(allowedNodes).ToArray();
            pageNumber = pageNumber < differenceNodes.Length ? pageNumber : differenceNodes.Length - 1;
            var page = pageNumber > differenceNodes.Length
                ? differenceNodes.Last()
                : differenceNodes.Skip(pageNumber).First();
            Console.WriteLine("Option -- details");
            for (var index = 0; index < page.Length; index++)
            {
                var item = page[index];
                Console.SetCursorPosition(0, (index * 3) + index);
                Console.ForegroundColor = item.Value.ConsoleColor;
                Console.Write($"{index} -- {item.Value.DisplayText}");
            }

            Console.WriteLine("\n");
            Console.ForegroundColor = ConsoleColor;
            if (pageNumber != 0)
            {
                Console.WriteLine($"p  -- Previous Page");
            }

            if (pageNumber < differenceNodes.Length - 1)
            {
                Console.WriteLine($"n -- Next Page");
            }

            Console.WriteLine("e -- Exit");

            Console.Write("Enter Option:");
            var entry = Console.ReadLine();

            switch (entry)
            {
                case "p" when pageNumber > 0:
                    pageNumber -= 1;
                    continue;
                case "n" when pageNumber < differenceNodes.Length - 1:
                    pageNumber += 1;
                    continue;
                case "e":
                    return;
            }

            if (!int.TryParse(entry, out var option) || option > page.Length || option < 0)
            {
                continue;
            }

            if (await DisplayOptions(page[option].Value))
            {
                _differenceNodeViews.Remove(page[option].Key);
            }
        }
    }

    private static async Task<bool> DisplayOptions(DifferenceNodeView view)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(view.DisplayText + Environment.NewLine);
            for (var index = 0; index < view.Options.Count; index++)
            {
                var item = view.Options[index];
                Console.WriteLine($"{index} -- " + item.Key);
            }

            Console.WriteLine("b -- back to previous menu");
            Console.Write("Enter Option:");
            var entry = Console.ReadLine();
            if (entry == "b")
            {
                return false;
            }

            if (!int.TryParse(entry, out var option) || option > view.Options.Count || option < 0)
            {
                continue;
            }

            bool done = false;
            Exception exception = null;
            Task.Run(() =>
            {
                try
                {
                    view.Options[option].Value();
                }
                catch (Exception e)
                {
                    exception = e;
                }

                done = true;
            });
            var pos = Console.GetCursorPosition();
            int dots = 0;
            while (!done)
            {
                Console.SetCursorPosition(pos.Left, pos.Top);
                dots++;
                Console.WriteLine("Executing {0}   ", string.Join("", Enumerable.Range(0, dots).Select(_ => ".")));
                dots = dots > 3 ? 0 : dots;
                await Task.Delay(200);
            }

            if (exception != null) throw exception;
            return true;
        }
    }
}