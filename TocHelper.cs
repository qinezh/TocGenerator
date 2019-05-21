using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TocGenerator
{
    public static class TocHelper
    {
        public static TocViewModel GenerateTocFromDirectory(string folderPath, string name)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return null;
            }

            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            var item = ConvertToTree(folderPath);
            if (!string.IsNullOrEmpty(name))
            {
                item.Name = name;
            }

            return new TocViewModel(new[] { item });
        }

        public static TocViewModel GenerateTocFromDirectory(List<string> folders, string name)
        {
            var rootItem = new TocItemViewModel
            {
                Name = name,
                Items = new TocViewModel()
            };

            foreach (var folder in folders)
            {
                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                {
                    var item = ConvertToTree(folder);
                    rootItem.Items.Add(item);
                }
            }

            if (rootItem.Items.Count == 0)
            {
                return null;
            }

            return new TocViewModel(new[] { rootItem });
        }

        public static void DistinctTocItems(TocViewModel model)
        {
            foreach (var item in model)
            {
                if (item.Items != null)
                {
                    DistinctTocItems(item.Items);
                }
            }

            UniqueTocItemsCore(model);
        }

        public static void SortTocItems(TocViewModel model)
        {
            foreach (var item in model)
            {
                if (item.Items != null)
                {
                    SortTocItems(item.Items);
                }
            }

            model.Sort(SortByItemsAndName);
        }

        public static void SaveToc(string tocPath, TocViewModel model)
        {
            YamlUtility.Serialize(tocPath, model);
        }

        public static void ResolveHref(string tocPath, TocViewModel model)
        {
            if (model == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(tocPath))
            {
                return;
            }

            var baseDir = Path.GetDirectoryName(tocPath);

            foreach (var item in model)
            {
                ResolveHref(baseDir, item);
            }
        }

        private static void ResolveHref(string baseDir, TocItemViewModel item)
        {
            if (item == null)
            {
                return;
            }

            if (!PathUtility.IsRelativePath(item.Href))
            {
                item.Href = PathUtility.MakeRelativePath(baseDir, item.Href);
            }

            if (item.Items != null && item.Items.Count > 0)
            {
                foreach (var child in item.Items)
                {
                    ResolveHref(baseDir, child);
                }
            }
        }

        private static int SortByItemsAndName(TocItemViewModel first, TocItemViewModel second)
        {
            if (first.Items != null && second.Items != null)
            {
                return first.Name.CompareTo(second.Name);
            }
            else if (first.Items != null)
            {
                return -1;
            }
            else if (second.Items != null)
            {
                return 1;
            }
            else
            {
                return first.Name.CompareTo(second.Name);
            }
        }

        private static void UniqueTocItemsCore(TocViewModel model)
        {
            var count = model.Count();
            var i = 0;
            var j = 1;

            while (j < count)
            {
                if (string.Equals(model[i].Name, model[j].Name))
                {
                    model[i].Items.AddRange(model[j].Items);
                    model[i].Items.Sort(SortByItemsAndName);
                    j++;
                }
                else
                {
                    i++;
                    model[i] = model[j];
                    j++;
                }
            }

            if (i < count)
            {
                model.RemoveRange(i + 1, count - i - 1);
            }
        }

        private static TocItemViewModel ConvertToTree(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var name = string.Empty;
            if (Directory.Exists(filePath))
            {
                var folderPath = filePath;
                name = Path.GetFileName(folderPath);

                var node = new TocItemViewModel
                {
                    Name = name
                };

                var indexPage = Path.Combine(folderPath, "index.md");

                if (File.Exists(indexPage))
                {
                    node.Href = indexPage;
                }

                var items = (from file in Directory.EnumerateFileSystemEntries(folderPath)
                             let fileName = Path.GetFileNameWithoutExtension(file)
                             let ext = Path.GetExtension(file)
                             where !string.Equals(fileName, "index", StringComparison.OrdinalIgnoreCase)
                             where ext == "" || string.Equals(".md", ext, StringComparison.OrdinalIgnoreCase)
                             let item = ConvertToTree(file)
                             where item != null
                             select item).ToList();

                if (items.Count == 0)
                {
                    return null;
                }

                node.Items = new TocViewModel(items);
                return node;
            }

            if (File.Exists(filePath))
            {
                name = Path.GetFileNameWithoutExtension(filePath);
                return new TocItemViewModel
                {
                    Name = name,
                    Href = filePath
                };
            }

            return null;
        }
    }
}
