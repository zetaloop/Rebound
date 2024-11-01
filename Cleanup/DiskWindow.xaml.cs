using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Rebound.Cleanup
{
    public class CleanItem
    {
        public string Name
        {
            get; set;
        }
        public string ImagePath
        {
            get; set;
        }
        public string ItemPath
        {
            get; set;
        }
        public string Description
        {
            get; set;
        }
        public string DisplaySize
        {
            get; set;
        }
        public long Size
        {
            get; set;
        }
        public bool IsChecked
        {
            get; set;
        }
    }

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DiskWindow : WindowEx
    {
        public long GetFolderLongSize(string folderPath)
        {
            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                return 0;
            }

            long totalSize = 0;

            try
            {
                // Enumerate files and directories
                foreach (var file in EnumerateFiles(folderPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we cannot access
                    }
                    catch (IOException ex)
                    {
                        // Handle other IO exceptions
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return 0;
            }
            catch (IOException ex)
            {
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }

            // Format the size into appropriate units
            return totalSize;
        }

        private void CleanupDriverStore()
        {
            try
            {
                string driverStorePath = @"C:\Windows\System32\DriverStore\FileRepository";

                // Fetch all directories in the DriverStore
                var directories = Directory.GetDirectories(driverStorePath);

                foreach (var dir in directories)
                {
                    // Check if the directory is unused or old
                    // In this example, we assume that if a directory was last accessed over 30 days ago, it's unnecessary.
                    var lastAccessTime = Directory.GetLastAccessTime(dir);
                    if (lastAccessTime < DateTime.Now.AddDays(-30))
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                            Debug.WriteLine($"Deleted: {dir}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error deleting {dir}: {ex.Message}");
                        }
                    }
                }

                Debug.WriteLine("Driver Store Cleanup Completed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred during cleanup: {ex.Message}");
            }
        }

        public long GetFolderLongSizeDrivers(string folderPath)
        {
            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                return 0;
            }

            long totalSize = 0;

            try
            {
                // Enumerate files and directories
                foreach (var file in EnumerateFiles(folderPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.LastAccessTime < DateTime.Now.AddDays(-30)) totalSize += fileInfo.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we cannot access
                    }
                    catch (IOException ex)
                    {
                        // Handle other IO exceptions
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return 0;
            }
            catch (IOException ex)
            {
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }

            // Format the size into appropriate units
            return totalSize;
        }

        public string GetFolderSizeDrivers(string folderPath)
        {
            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                return "0 B";
            }

            long totalSize = 0;

            try
            {
                // Enumerate files and directories
                foreach (var file in EnumerateFiles(folderPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.LastAccessTime < DateTime.Now.AddDays(-30)) totalSize += fileInfo.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we cannot access
                    }
                    catch (IOException ex)
                    {
                        // Handle other IO exceptions
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return "0 B";
            }
            catch (IOException ex)
            {
                return "0 B";
            }
            catch (Exception ex)
            {
                return "0 B";
            }

            // Format the size into appropriate units
            return FormatSize(totalSize);
        }

        public long GetFolderLongSizeDB(string folderPath)
        {
            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                return 0;
            }

            long totalSize = 0;

            try
            {
                // Enumerate files and directories
                foreach (var file in EnumerateFiles(folderPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.Extension.Contains("db") && fileInfo.Name.Contains("thumbcache") == true) totalSize += fileInfo.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we cannot access
                    }
                    catch (IOException ex)
                    {
                        // Handle other IO exceptions
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return 0;
            }
            catch (IOException ex)
            {
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }

            // Format the size into appropriate units
            return totalSize;
        }

        public string GetFolderSize(string folderPath)
        {
            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                return "0 B";
            }

            long totalSize = 0;

            try
            {
                // Enumerate files and directories
                foreach (var file in EnumerateFiles(folderPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we cannot access
                    }
                    catch (IOException ex)
                    {
                        // Handle other IO exceptions
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return "无权访问";
            }
            catch (IOException ex)
            {
                return "读写错误";
            }
            catch (Exception ex)
            {
                return "未知错误";
            }

            // Format the size into appropriate units
            return FormatSize(totalSize);
        }

        public void DeleteFiles(string folderPath)
        {
            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            try
            {
                // Enumerate files and directories
                foreach (var file in EnumerateFiles(folderPath))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we cannot access
                    }
                    catch (IOException ex)
                    {
                        // Handle other IO exceptions
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (IOException ex)
            {
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void DeleteFilesDB(string folderPath)
        {
            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            try
            {
                // Enumerate files and directories
                foreach (var file in EnumerateFiles(folderPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.Extension.Contains("db") && fileInfo.Name.Contains("thumbcache") == true) File.Delete(file);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we cannot access
                    }
                    catch (IOException ex)
                    {
                        // Handle other IO exceptions
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (IOException ex)
            {
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public string GetFolderSizeDB(string folderPath)
        {
            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                return "0 B";
            }

            long totalSize = 0;

            try
            {
                // Enumerate files and directories
                foreach (var file in EnumerateFiles(folderPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        Debug.WriteLine($"NAME: {fileInfo.Name} || EXTENSION: {fileInfo.Extension}");
                        if (fileInfo.Extension.Contains("db") && fileInfo.Name.Contains("thumbcache") == true) totalSize += fileInfo.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Debug.WriteLine("ERR");
                        // Skip files we cannot access
                    }
                    catch (IOException ex)
                    {
                        Debug.WriteLine("ERR");
                        // Handle other IO exceptions
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return "无权访问";
            }
            catch (IOException ex)
            {
                return "读写错误";
            }
            catch (Exception ex)
            {
                return "未知错误";
            }

            // Format the size into appropriate units
            return FormatSize(totalSize);
        }

        private IEnumerable<string> EnumerateFiles(string folderPath)
        {
            var directoriesToProcess = new Stack<string>(new[] { folderPath });

            while (directoriesToProcess.Count > 0)
            {
                string currentDir = directoriesToProcess.Pop();

                IEnumerable<string> files = GetFilesSafe(currentDir);
                foreach (string file in files)
                {
                    yield return file;
                }

                IEnumerable<string> subDirs = GetDirectoriesSafe(currentDir);
                foreach (string subDir in subDirs)
                {
                    directoriesToProcess.Push(subDir);
                }
            }
        }

        private IEnumerable<string> GetFilesSafe(string directory)
        {
            try
            {
                return Directory.EnumerateFiles(directory);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we cannot access
                return Array.Empty<string>();
            }
            catch (IOException ex)
            {
                // Handle IO exceptions for directory operations
                return Array.Empty<string>();
            }
        }

        private IEnumerable<string> GetDirectoriesSafe(string directory)
        {
            try
            {
                return Directory.EnumerateDirectories(directory);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we cannot access
                return Array.Empty<string>();
            }
            catch (IOException ex)
            {
                // Handle IO exceptions for directory operations
                return Array.Empty<string>();
            }
        }

        private string FormatSize(long sizeInBytes)
        {
            if (sizeInBytes < 1024)
                return $"{sizeInBytes} B";
            else if (sizeInBytes < 1024 * 1024)
                return $"{sizeInBytes / 1024.0:F2} KB";
            else if (sizeInBytes < 1024 * 1024 * 1024)
                return $"{sizeInBytes / (1024.0 * 1024):F2} MB";
            else if (sizeInBytes < 1024L * 1024 * 1024 * 1024)
                return $"{sizeInBytes / (1024.0 * 1024 * 1024):F2} GB";
            else if (sizeInBytes < 1024L * 1024 * 1024 * 1024 * 1024)
                return $"{sizeInBytes / (1024.0 * 1024 * 1024 * 1024):F2} TB";
            else
                return $"{sizeInBytes / (1024.0 * 1024 * 1024 * 1024 * 1024):F2} PB";
        }

        public List<CleanItem> items = new List<CleanItem>();

        public string Disk = "";

        public DiskWindow(string disk)
        {
            Disk = disk;
            this.InitializeComponent();
            if (IsAdministrator() == true) SysFilesButton.Visibility = Visibility.Collapsed;
            if (disk == "C:")
            {
                items.Add(new CleanItem
                {
                    Name = $"Internet 临时文件",
                    ItemPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Microsoft\Windows\INetCache",
                    ImagePath = "ms-appx:///Assets/imageres_59.ico",
                    Size = GetFolderLongSize($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Microsoft\Windows\INetCache"),
                    DisplaySize = GetFolderSize($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Microsoft\Windows\INetCache"),
                    Description = "这些文件是浏览网页时自动缓存的页面、图片等，能让网页加载更快。删除这些文件可以释放空间，但网页首次加载可能比较慢，重新缓存完就快了。",
                    IsChecked = false,
                });
            }
            if (disk == "C:")
            {
                items.Add(new CleanItem
                {
                    Name = $"已下载的程序文件",
                    ItemPath = @"C:\Windows\Downloaded Program Files",
                    ImagePath = "ms-appx:///Assets/imageres_3.ico",
                    Size = GetFolderLongSize(@"C:\Windows\Downloaded Program Files"),
                    DisplaySize = GetFolderSize(@"C:\Windows\Downloaded Program Files"),
                    Description = "这些文件是浏览网页时自动下载的 ActiveX 控件和 Java 小程序，能让网页加载更快。如果用不到了可以删除。",
                    IsChecked = true,
                });
            }
            if (disk == "C:")
            {
                items.Add(new CleanItem
                {
                    Name = $"Rebound 11 临时文件",
                    ItemPath = @"C:\Rebound11\Temp",
                    ImagePath = "ms-appx:///Assets/r11imageres_101.ico",
                    Size = GetFolderLongSize(@"C:\Rebound11\Temp"),
                    DisplaySize = GetFolderSize(@"C:\Rebound11\Temp"),
                    Description = "安装 Rebound 11 时，可能会将安装包和相关文件复制到这个临时文件夹中，方便调用 PowerShell 读取路径。安装完成之后可以删除。",
                    IsChecked = true,
                });
            }
            items.Add(new CleanItem
            {
                Name = $"回收站",
                ItemPath = $@"{disk}\$Recycle.Bin",
                ImagePath = "ms-appx:///Assets/imageres_54.ico",
                Size = GetFolderLongSize($@"{disk}\$Recycle.Bin"),
                DisplaySize = GetFolderSize($@"{disk}\$Recycle.Bin"),
                Description = "回收站里存放着你删除的文件，这些删掉的文件可以还原。清空回收站释放空间后，将会永久删除这些文件。",
                IsChecked = true,
            });
            if (disk == "C:")
            {
                items.Add(new CleanItem
                {
                    Name = $"临时文件",
                    ItemPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp",
                    ImagePath = "ms-appx:///Assets/imageres_2.ico",
                    Size = GetFolderLongSize($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp"),
                    DisplaySize = GetFolderSize($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp"),
                    Description = "这些文件是系统和软件在安装、运行的时候，在 AppData 文件夹中临时创建的文件。系统或软件使用完成后，通常可以安全删除。",
                    IsChecked = false,
                });
            }
            if (disk == "C:")
            {
                items.Add(new CleanItem
                {
                    Name = $"缩略图",
                    ItemPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Microsoft\Windows\Explorer",
                    ImagePath = "ms-appx:///Assets/imageres_2.ico",
                    Size = GetFolderLongSizeDB($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Microsoft\Windows\Explorer"),
                    DisplaySize = GetFolderSizeDB($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Microsoft\Windows\Explorer"),
                    Description = "缩略图是图片、视频等文件的图标上显示的预览小图片。删除缩略图缓存可以释放空间，系统会在下次查看文件时重新生成缩略图。",
                    IsChecked = false,
                });
            }
            if (disk == "C:")
            {
                items.Add(new CleanItem
                {
                    Name = $"系统创建的 Windows 错误报告",
                    ItemPath = $@"C:\ProgramData\Microsoft\Windows\WER",
                    ImagePath = "ms-appx:///Assets/EventViewer.png",
                    Size = GetFolderLongSize($@"C:\ProgramData\Microsoft\Windows\WER"),
                    DisplaySize = GetFolderSize($@"C:\ProgramData\Microsoft\Windows\WER"),
                    Description = "当系统遇到错误时，会生成这些用于排查问题的数据报告。如果已经发送报告或者用不到了，可以删除它们来释放空间。",
                    IsChecked = false,
                });
            }
            if (disk == "C:")
            {
                items.Add(new CleanItem
                {
                    Name = $"下载文件夹（当前用户）",
                    ItemPath = $@"{KnownFolders.GetPath(KnownFolder.Downloads)}",
                    ImagePath = "ms-appx:///Assets/imageres_184.ico",
                    Size = GetFolderLongSize($@"{KnownFolders.GetPath(KnownFolder.Downloads)}"),
                    DisplaySize = GetFolderSize($@"{KnownFolders.GetPath(KnownFolder.Downloads)}"),
                    Description = "这是你的 “下载” 文件夹，网上下载的文件通常会存放在这里。这些文件如果用不到了，可以删掉。",
                    IsChecked = true,
                });
            }
            if (disk == "C:" && IsAdministrator() == true)
            {
                items.Add(new CleanItem
                {
                    Name = $"系统缓存文件",
                    ItemPath = $@"C:\Windows\Prefetch\",
                    ImagePath = "ms-appx:///Assets/imageres_2.ico",
                    Size = GetFolderLongSize($@"C:\Windows\Prefetch\"),
                    DisplaySize = GetFolderSize($@"C:\Windows\Prefetch\"),
                    Description = "这些文件让系统运行更快，包括加速软件启动的预取文件、加速字体显示的字体缓存文件等。删除缓存可以释放空间，但一些操作可能暂时会变慢。",
                    IsChecked = false,
                });
            }
            if (disk == "C:" && IsAdministrator() == true)
            {
                items.Add(new CleanItem
                {
                    Name = $"Windows 更新缓存文件",
                    ItemPath = $@"C:\Windows\SoftwareDistribution",
                    ImagePath = "ms-appx:///Assets/imageres_2.ico",
                    Size = GetFolderLongSize($@"C:\Windows\SoftwareDistribution"),
                    DisplaySize = GetFolderSize($@"C:\Windows\SoftwareDistribution"),
                    Description = "这些是 Windows 安装更新之后留下的无用文件，包括更新安装包、旧版本文件等。这些文件可能占用大量空间，但删除后可能会让一些更新无法卸载。",
                    IsChecked = true,
                });
            }
            if (disk == "C:" && IsAdministrator() == true)
            {
                items.Add(new CleanItem
                {
                    Name = $"旧版本 Windows 系统",
                    ItemPath = $@"C:\Windows.old",
                    ImagePath = "ms-appx:///Assets/imageres_2.ico",
                    Size = GetFolderLongSize($@"C:\Windows.old"),
                    DisplaySize = GetFolderSize($@"C:\Windows.old"),
                    Description = "升级新版本 Windows 后，旧版本文件会保留在 Windows.old 文件夹中。删除这些文件将无法还原旧版本。",
                    IsChecked = false,
                });
            }
            if (disk == "C:" && IsAdministrator() == true)
            {
                items.Add(new CleanItem
                {
                    Name = $"系统错误内存转储文件",
                    ItemPath = $@"C:\Windows\MEMORY.DMP",
                    ImagePath = "ms-appx:///Assets/EventViewer.png",
                    Size = GetFolderLongSize($@"C:\Windows\MEMORY.DMP"),
                    DisplaySize = GetFolderSize($@"C:\Windows\MEMORY.DMP"),
                    Description = "这些文件记录了系统崩溃时的内存状态，用于排查问题。完整的转储文件会占用较多空间，如果用不到了可以删除。",
                    IsChecked = false,
                });
            }
            if (disk == "C:" && IsAdministrator() == true)
            {
                items.Add(new CleanItem
                {
                    Name = $"系统错误小型转储文件",
                    ItemPath = $@"C:\Windows\Minidump",
                    ImagePath = "ms-appx:///Assets/EventViewer.png",
                    Size = GetFolderLongSize($@"C:\Windows\Minidump"),
                    DisplaySize = GetFolderSize($@"C:\Windows\Minidump"),
                    Description = "小型转储文件（Minidumps）与完整内存转储相比更加精简，也用于排查问题。如果用不到了也可删除。",
                    IsChecked = false,
                });
            }
            if (disk == "C:" && IsAdministrator() == true)
            {
                items.Add(new CleanItem
                {
                    Name = $"临时 Windows 安装文件",
                    ItemPath = $@"C:\Windows\Temp",
                    ImagePath = "ms-appx:///Assets/imageres_2.ico",
                    Size = GetFolderLongSize($@"C:\Windows\Temp"),
                    DisplaySize = GetFolderSize($@"C:\Windows\Temp"),
                    Description = "这些文件是 Windows 安装和更新过程中生成的，安装完成后通常会自动删除。手动清理也不会影响系统稳定性。",
                    IsChecked = false,
                });
            }
            if (disk == "C:" && IsAdministrator() == true)
            {
                items.Add(new CleanItem
                {
                    Name = $"设备驱动程序包",
                    ItemPath = @"C:\Windows\System32\DriverStore\FileRepository",
                    ImagePath = "ms-appx:///Assets/DDORes_2001.ico",
                    Size = GetFolderLongSizeDrivers(@"C:\Windows\System32\DriverStore\FileRepository"),
                    DisplaySize = GetFolderSizeDrivers(@"C:\Windows\System32\DriverStore\FileRepository"),
                    Description = "驱动程序用于控制和管理硬件。随着版本更新，旧的驱动会积累起来，清理旧的或没用的驱动可以释放空间，不会影响设备运行。",
                    IsChecked = false,
                });
            }

            long size = 0;

            foreach (var item in items)
            {
                size += item.Size;
            }

            // Sort the list alphabetically by the Name property
            items.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            CleanItems.ItemsSource = items;

            Title.Title = $"可以释放 ({disk}) 上 {FormatSize(size)} 的磁盘空间。";

            CleanItems.SelectedIndex = 0;

            CheckItems();

            string commandArgs = string.Join(" ", Environment.GetCommandLineArgs().Skip(1));

            if (commandArgs.Contains("CLEANALL"))
            {
                RunFullOptimization();
            }
        }

        public async void RunFullOptimization()
        {
            SelectAllBox.IsChecked = true;

            await Task.Delay(500);

            CleanItems.IsEnabled = false;
            CleanButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            SelectAllBox.IsEnabled = false;
            MoreOptions.IsEnabled = false;
            ViewFiles.IsEnabled = false;
            Working.IsIndeterminate = true;
            (this as WindowEx).Title = $"磁盘清理：正在清理 ({Disk})...（要点时间）";

            await Task.Delay(100);

            foreach (var item in items)
            {
                if (item.Name == "缩略图")
                {
                    DeleteFilesDB(item.ItemPath);
                }
                else if (item.Name == "设备驱动程序包")
                {
                    CleanupDriverStore();
                }
                else
                {
                    DeleteFiles(item.ItemPath);
                }
            }

            Close();
        }

        public enum KnownFolder
        {
            Contacts,
            Downloads,
            Favorites,
            Links,
            SavedGames,
            SavedSearches
        }

        public static class KnownFolders
        {
            private static readonly Dictionary<KnownFolder, Guid> _guids = new()
            {
                [KnownFolder.Contacts] = new("56784854-C6CB-462B-8169-88E350ACB882"),
                [KnownFolder.Downloads] = new("374DE290-123F-4565-9164-39C4925E467B"),
                [KnownFolder.Favorites] = new("1777F761-68AD-4D8A-87BD-30B759FA33DD"),
                [KnownFolder.Links] = new("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968"),
                [KnownFolder.SavedGames] = new("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
                [KnownFolder.SavedSearches] = new("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA")
            };

            public static string GetPath(KnownFolder knownFolder)
            {
                return SHGetKnownFolderPath(_guids[knownFolder], 0);
            }

            [DllImport("shell32",
                CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
            private static extern string SHGetKnownFolderPath(
                [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
                nint hToken = 0);
        }

        public async void CheckItems()
        {
            try
            {
                await Task.Delay(10);

                int totalItems = 0;
                int selectedItems = 0;

                foreach (var item in items)
                {
                    totalItems++;
                    if (item.IsChecked == true) selectedItems++;
                }

                if (CleanItems.SelectedIndex >= 0) ItemDetails.Text = items[CleanItems.SelectedIndex].Description;

                if (selectedItems == 0)
                {
                    SelectAllBox.IsChecked = false;
                }
                else if (selectedItems == totalItems)
                {
                    SelectAllBox.IsChecked = true;
                }
                else
                {
                    SelectAllBox.IsChecked = null;
                }

                CheckItems();
            }
            catch
            {

            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            string customDefragPath = @"C:\Rebound11\rdfrgui.exe";
            string systemDefragPath = @"C:\Windows\System32\dfrgui.exe";

            try
            {
                if (File.Exists(customDefragPath))
                {
                    // Launch the custom defrag tool
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = customDefragPath,
                        UseShellExecute = true,
                        Verb = "runas" // Ensure it runs with admin rights
                    });
                }
                else
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    startInfo.Arguments = $"Start-Process -FilePath \"dfrgui\"";

                    try
                    {
                        var res = Process.Start(startInfo);
                        await res.WaitForExitAsync();
                        if (res.ExitCode == 0) return;
                        else throw new Exception();
                    }
                    catch (Exception ex)
                    {
                        await this.ShowMessageDialogAsync($"系统找不到指定的文件或命令行参数无效。", "错误");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
                ContentDialog noWifiDialog = new ContentDialog
                {
                    Title = "错误",
                    Content = $"无法启动磁盘碎片整理工具: {ex.Message}",
                    CloseButtonText = "确定"
                };

                await noWifiDialog.ShowAsync(); // Showing the error dialog
            }
        }

        private async void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.Arguments = $"Start-Process -FilePath \"cleanmgr\"";

            try
            {
                var res = Process.Start(startInfo);
                await res.WaitForExitAsync();
                if (res.ExitCode == 0) Close();
                else throw new Exception();
            }
            catch (Exception ex)
            {
                await this.ShowMessageDialogAsync($"系统找不到指定的文件或命令行参数无效。", "错误");
            }
        }

        private async void MenuFlyoutItem_Click_2(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures"));
        }

        private void CleanItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (SelectAllBox.IsChecked == true)
            {
                CleanItems.ItemsSource = null;
                foreach (var item in items)
                {
                    item.IsChecked = true;
                }
                CleanItems.ItemsSource = items;
                CleanItems.SelectedIndex = 0;
            }
            if (SelectAllBox.IsChecked == false)
            {
                CleanItems.ItemsSource = null;
                foreach (var item in items)
                {
                    item.IsChecked = false;
                }
                CleanItems.ItemsSource = items;
                CleanItems.SelectedIndex = 0;
            }
        }

        public bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas",
                Arguments = @$"Start-Process ""shell:AppsFolder\e8dfd11c-954d-46a2-b700-9cbc6201f056_59a6r38835q7a!App"" -ArgumentList ""{Disk}"" -Verb RunAs"
            });
            Close();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CleanItems.IsEnabled = false;
            CleanButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            SelectAllBox.IsEnabled = false;
            MoreOptions.IsEnabled = false;
            ViewFiles.IsEnabled = false;
            Working.IsIndeterminate = true;
            (this as WindowEx).Title = $"磁盘清理：正在清理 ({Disk})...（要点时间）";

            await Task.Delay(100);

            foreach (var item in items)
            {
                if (item.IsChecked == true)
                {
                    if (item.Name == "缩略图")
                    {
                        DeleteFilesDB(item.ItemPath);
                    }
                    else if (item.Name == "设备驱动程序包")
                    {
                        CleanupDriverStore();
                    }
                    else
                    {
                        DeleteFiles(item.ItemPath);
                    }
                }
            }

            Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ViewFiles_Click(object sender, RoutedEventArgs e)
        {
            if (items[CleanItems.SelectedIndex].ItemPath.Contains("Recycle.Bin"))
            {
                await Launcher.LaunchFolderPathAsync($"shell:RecycleBinFolder");
                return;
            }
            await Launcher.LaunchFolderPathAsync($"{items[CleanItems.SelectedIndex].ItemPath}");
        }
    }
}
