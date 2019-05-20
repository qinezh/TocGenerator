using System;
using System.IO;

namespace TocGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootFolder = "E:/temp/blog";
            var rootName = "TocRoot";
            var tocPath = Path.Combine("E:/temp", "toc.yml");
            var tocViewModel = TocHelper.GenerateTocFromDirectory(rootFolder, rootName);

            TocHelper.ResolveHref(tocPath, tocViewModel);
            TocHelper.SortTocItems(tocViewModel);
            TocHelper.DistinctTocItems(tocViewModel);
            TocHelper.SaveToc(tocPath, tocViewModel);

            Console.WriteLine("Finished");
        }
    }
}
