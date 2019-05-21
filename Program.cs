using Microsoft.DocAsCode.Common;
using System;
using System.IO;

namespace TocGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
                return 2;
            }

            try
            {
                var rootFolder = new DirectoryInfo(args[0]);
                var rootName = rootFolder.Name;
                var rootPath = PathUtility.MakeRelativePath(AppDomain.CurrentDomain.BaseDirectory, rootFolder.FullName);
                var tocPath = args.Length > 1 ? args[1] : Path.Combine(rootPath, "toc.yml");
                var tocViewModel = TocHelper.GenerateTocFromDirectory(rootPath, rootName);

                if (tocViewModel == null)
                {
                    Console.WriteLine($"no content under {rootFolder.FullName}");
                    return 2;
                }

                TocHelper.ResolveHref(tocPath, tocViewModel);
                TocHelper.SortTocItems(tocViewModel);
                TocHelper.DistinctTocItems(tocViewModel);
                TocHelper.SaveToc(tocPath, tocViewModel);

                Console.WriteLine($"toc.yml is generated at {tocPath}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"\t{AppDomain.CurrentDomain.FriendlyName} <path_of_folder> [path_of_toc]");
        }
    }
}
