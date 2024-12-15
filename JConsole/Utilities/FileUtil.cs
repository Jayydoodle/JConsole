using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using Spectre.Console;
using System.IO.Compression;
using System.Text;
using ZipArchive = SharpCompress.Archives.Zip.ZipArchive;

namespace JConsole
{
    public static class FileUtil
    {
        public static void ArchiveDirectory(DirectoryInfo sourceDir, DirectoryInfo destinationDir = null, bool zip = false)
        {
            List<DirectoryInfo> directories = sourceDir.GetDirectories().Where(x => !x.Name.StartsWith("_Archive")).ToList();
            List<FileInfo> files = sourceDir.GetFiles().Where(x => !x.Name.StartsWith("_Archive")).ToList();

            if (!directories.Any() && !files.Any())
                return;

            if (destinationDir == null)
                destinationDir = sourceDir;

            DirectoryInfo archive = destinationDir.CreateSubdirectory(string.Format("_Archive__{0}", DateTime.Now.ToString("yyyy-MM-dd__h-mm-ss-tt")));

            directories.ForEach(directory => directory.MoveTo(Path.Combine(archive.FullName, directory.Name)));
            files.ForEach(file => file.MoveTo(Path.Combine(archive.FullName, file.Name)));

            string archiveName = archive.FullName;

            if (zip)
            {
                ZipFile.CreateFromDirectory(archive.FullName, string.Format("{0}{1}", archive.FullName, ".zip"));
                archive.Delete(true);
                archiveName = string.Format("{0}{1}", archiveName, ".zip");
            }

            AnsiConsole.MarkupLine("The previous versions of all files in the directory [orange1]{0}[/]\nhave been archived to [teal]{1}[/]\n", sourceDir.FullName, archiveName);
        }

        public static void ArchiveFiles(DirectoryInfo dir, string searchPattern, List<string> fileNames = null)
        {
            List<FileInfo> files = dir.GetFiles(searchPattern).ToList();

            if (fileNames != null)
                files = files.Where(x => fileNames.Any(y => x.Name == y)).ToList();

            if (!files.Any())
                return;

            DirectoryInfo archive = dir.CreateSubdirectory(string.Format("_Archive__{0}", DateTime.Now.ToString("yyyy-MM-dd__h-mm-ss-tt")));

            files.ForEach(file => file.MoveTo(Path.Combine(archive.FullName, file.Name)));

            AnsiConsole.MarkupLine("The previous versions of the [red]{0}[/] files in the directory [red]{1}[/]\nhave been archived to [teal]{2}[/]\n",
                                  searchPattern.Replace("*", string.Empty), dir.Name, archive.Name);
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo newDir = Directory.CreateDirectory(Path.Combine(destinationDir, dir.Name));

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(newDir.FullName, file.Name);
                file.CopyTo(targetFilePath);
            }

            if (recursive)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();

                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(newDir.FullName, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static void CopyFilesToDirectory(DirectoryInfo sourceDir, DirectoryInfo destinationDir,
        string searchPattern = null, bool allowOverWrite = false, bool recursive = false, bool writeToConsole = true)
        {
            if (!sourceDir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir.FullName}");

            foreach (FileInfo file in sourceDir.GetFiles(searchPattern))
            {
                string targetFilePath = Path.Combine(destinationDir.FullName, file.Name);
                file.CopyTo(targetFilePath, allowOverWrite);
            }

            if (recursive)
            {
                DirectoryInfo[] dirs = sourceDir.GetDirectories();

                foreach (DirectoryInfo subDir in dirs)
                {
                    CopyFilesToDirectory(subDir, destinationDir, searchPattern, allowOverWrite, recursive, false);
                }
            }

            if (!writeToConsole) { return; }

            AnsiConsole.MarkupLine("The [green]{0}[/] files from the source directory: [teal]{1}[/]\nHave been copied to the directory [orange1]{2}[/] successfully\n",
            searchPattern, sourceDir.FullName, destinationDir.FullName);
        }

        public static void ExtractFile(FileInfo file, string destinationPath, string searchPattern = null)
        {
            switch (file.Extension)
            {
                case GlobalConstants.FileExtension.Zip:
                    ExtractZipFile(file, destinationPath, searchPattern);
                    break;

                case GlobalConstants.FileExtension.Rar:
                    ExtractRarFile(file, destinationPath, searchPattern);
                    break;

                case GlobalConstants.FileExtension.SevenZip:
                    Extract7zFile(file, destinationPath, searchPattern);
                    break;

                default:
                    throw new Exception(string.Format("Could not extract file [red]{0}[/].  Unknown extension '[red]{1}[/]'", file.FullName, file.Extension));
            }
        }

        public static T GetFileSystemInfoFromInput<T>(string prompt, bool isRequired)
        where T : FileSystemInfo
        {
            T info = null;

            while (info == null || !info.Exists)
            {
                string path = ConsoleUtil.GetInput(prompt, x => !string.IsNullOrEmpty(x) || !isRequired);

                if (string.IsNullOrEmpty(path) && !isRequired)
                {
                    info = null;
                    break;
                }

                if (!string.IsNullOrEmpty(path))
                    path = path.Replace("\"", string.Empty);

                info = (T)Activator.CreateInstance(typeof(T), new object[] { path });

                if (!info.Exists)
                {
                    AnsiConsole.WriteLine("\nThe directory does not exist\n");
                }
            }

            AnsiConsole.WriteLine();

            return info;
        }

        public static DirectoryInfo GetOrCreateDirectory(string filePath)
        {
            // If directory does not exist, create it
            if (!Directory.Exists(filePath))
                return Directory.CreateDirectory(filePath);
            else
                return new DirectoryInfo(filePath);
        }

        public static List<string> ReadTextFromFile(DirectoryInfo dir, string fileName)
        {
            string filePath = Path.Combine(dir.FullName, fileName);
            List<string> textEntries = new List<string>();

            try
            {
                textEntries = File.ReadLines(filePath).ToList();
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(FileNotFoundException))
                    AnsiConsole.MarkupLine("The file [red]{0}[/] does not exist.", filePath);
                else
                    AnsiConsole.WriteLine(e.ToString());
            }

            return textEntries;
        }

        public static void WriteToFile(DirectoryInfo dir, string outputFileName, StringBuilder content)
        {
            if (dir == null)
                return;

            string fileName = Path.Combine(dir.FullName, outputFileName);

            try
            {
                File.WriteAllText(fileName, content.ToString());
                TextPath path = new TextPath(fileName);
                path.LeafColor(Color.Green);

                AnsiConsole.Write("Content was successfully written to the file: ");
                AnsiConsole.Write(path);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(FileNotFoundException))
                    AnsiConsole.MarkupLine("The file [red]{0}[/] does not exist.", fileName);
                else
                    AnsiConsole.WriteLine(e.ToString());
            }
        }

        #region Private API

        private static void ExtractZipFile(FileInfo file, string desintationPath, string searchPattern = null)
        {
            using (var archive = ZipArchive.Open(file.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!string.IsNullOrEmpty(searchPattern) && !entry.Key.Contains(searchPattern))
                        continue;

                    entry.WriteToDirectory(desintationPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }

        private static void ExtractRarFile(FileInfo file, string desintationPath, string searchPattern = null)
        {
            using (var archive = RarArchive.Open(file.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!string.IsNullOrEmpty(searchPattern) && !entry.Key.Contains(searchPattern))
                        continue;

                    entry.WriteToDirectory(desintationPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }

        private static void Extract7zFile(FileInfo file, string desintationPath, string searchPattern = null)
        {

            using (var archive = SevenZipArchive.Open(file.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!string.IsNullOrEmpty(searchPattern) && !entry.Key.Contains(searchPattern))
                        continue;

                    entry.WriteToDirectory(desintationPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }

        #endregion
    }
}
