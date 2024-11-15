﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32.TaskScheduler;
using Microsoft.WindowsAPICodePack.Taskbar;
using Rebound.Defrag.Helpers;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System;
using WinUIEx;
using WinUIEx.Messaging;
using Task = System.Threading.Tasks.Task;

#nullable enable
#pragma warning disable CA1854 // Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method

namespace Rebound.Defrag;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        this?.InitializeComponent();
        // Set window backdrop to Mica for a modern translucent effect
        SystemBackdrop = new MicaBackdrop();

        // Set the window title
        Title = "优化驱动器";

        // Window customization
        IsMaximizable = false;
        this.SetWindowSize(800, 670);
        IsResizable = false;
        AppWindow.DefaultTitleBarShouldMatchAppModeTheme = true;
        this.CenterOnScreen();
        this.SetIcon(@$"{AppContext.BaseDirectory}/Assets/Rebound.Defrag.ico");
    }

    public async Task LoadAppAsync()
    {
        AdvancedView.IsOn = GetBoolFromLocalSettings("Advanced");

        // Load data based on the current state of 'AdvancedView'
        await LoadData(AdvancedView.IsOn);

        // Begin monitoring window messages (such as device changes)
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowMessageMonitor mon = new(hWnd);

        // Subscribe to the WindowMessageReceived event
        mon.WindowMessageReceived += MessageReceived;

        // Set up a timer to periodically refresh the message listener every 5 seconds
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5) // Timer interval set to 5 seconds
        };

        // Timer event handler
        timer.Tick += (sender, e) =>
        {
            mon.WindowMessageReceived += MessageReceived; // Re-subscribe to the event to keep monitoring
        };

        // Start the timer
        timer.Start();

        // Check if the application is running with administrator privileges
        IsAdministrator();
        CheckTask();
    }

    public static void SetProgressState(TaskbarProgressBarState state)
    {
        TaskbarManager.Instance.SetProgressState(state);
    }

    public static void SetProgressValue(int completed, int total)
    {
        TaskbarManager.Instance.SetProgressValue(completed, total);
    }

    private async void MessageReceived(object? sender, WindowMessageEventArgs e)
    {
        // Handle incoming messages
        switch (e.Message.MessageId)
        {
            default:
                {
                    // No relevant message, break
                    break;
                }
            case Win32Helper.WM_DEVICECHANGE:
                {
                    // Handle specific device changes
                    switch ((int)e.Message.WParam)
                    {
                        case Win32Helper.DBT_DEVICEARRIVAL:
                            {
                                // Device or partition inserted
                                MyListView.ItemsSource = null; // Clear existing list
                                await LoadData(AdvancedView.IsOn); // Reload data based on AdvancedView state
                                break;
                            }
                        case Win32Helper.DBT_DEVICEREMOVECOMPLETE:
                            {
                                // Device or partition removed
                                MyListView.ItemsSource = null; // Clear existing list
                                await LoadData(AdvancedView.IsOn); // Reload data based on AdvancedView state
                                break;
                            }
                        default:
                            {
                                // Handle any other device action
                                break;
                            }
                    }
                    break;
                }
        }
    }

    public class DiskItem : Item
    {
        public string? DriveLetter
        {
            get; set;
        }
        public string? MediaType
        {
            get; set;
        }
        public int ProgressValue
        {
            get; set;
        }
        public bool IsChecked
        {
            get; set;
        }
    }

    public bool IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        if (principal.IsInRole(WindowsBuiltInRole.Administrator))
        {
            Admin1.Visibility = Visibility.Collapsed;
            Admin2.Visibility = Visibility.Collapsed;
            Admin3.Visibility = Visibility.Collapsed;
            Admin4.Visibility = Visibility.Collapsed;
        }
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private void ListviewSelectionChange(object sender, SelectionChangedEventArgs args)
    {
        LoadSelectedItemInfo(GetStatus());
    }

    public string GetLastOptimizeDate()
    {
        if (MyListView.SelectedItem != null)
        {
            var selectedItem = MyListView.SelectedItem as DiskItem;
            // Handle the selection change event

            try
            {
                var i = DefragInfo.GetEventLogEntriesForID(258);
                return i.Last(s => s.Contains($"({selectedItem?.DriveLetter?.ToString().Remove(2, 1)})"));
            }
            catch
            {
                return "从未运行....";
            }
        }
        else
        {
            return "未知....";
        }
    }

    public void LoadSelectedItemInfo(string status, string info = "....")
    {
        if (info == "....")
        {
            info = GetLastOptimizeDate();
        }
        else
        {
            info = "未知....";
        }
        if (MyListView.SelectedItem != null)
        {
            var selectedItem = MyListView.SelectedItem as DiskItem;
            DetailsBar.Title = selectedItem?.Name;
            DetailsBar.Message = $"设备类型：{selectedItem?.MediaType}\n上次分析：{info[..^4]}\n当前状态：{status}";
            DetailsBar.Severity = InfoBarSeverity.Informational;
            OptimizeButton.IsEnabled = AdvancedView.IsEnabled;
            if (status.Contains("需要优化"))
            {
                DetailsBar.Severity = InfoBarSeverity.Warning;
            }
            if (selectedItem?.MediaType == "光盘")
            {
                DetailsBar.Message = "设备类型：光盘\n上次分析：从未运行\n当前状态：不能优化光盘";
                DetailsBar.Severity = InfoBarSeverity.Error;
                VisualStateManager.GoToState(OptimizeButton, "Disabled", true);
                OptimizeButton.IsEnabled = false;
            }
            if (selectedItem?.Name == "EFI 系统分区")
            {
                DetailsBar.Message = $"设备类型：{selectedItem.MediaType}\n上次分析：从未运行\n当前状态：不能优化 EFI 系统分区";
                DetailsBar.Severity = InfoBarSeverity.Informational;
            }
            if (selectedItem?.Name == "恢复分区")
            {
                DetailsBar.Message = $"设备类型：{selectedItem.MediaType}\n上次分析：从未运行\n当前状态：不能优化恢复分区";
                DetailsBar.Severity = InfoBarSeverity.Error;
                VisualStateManager.GoToState(OptimizeButton, "Disabled", true);
                OptimizeButton.IsEnabled = false;
            }
        }
    }

    public string GetStatus()
    {
        var status = string.Empty;

        try
        {
            if (MyListView.SelectedItem != null)
            {
                var i = DefragInfo.GetEventLogEntriesForID(258);
                var selectedItem = MyListView.SelectedItem as DiskItem;

                var selI = i.Last(s => s.Contains($"({selectedItem?.DriveLetter?.ToString().Remove(2, 1)})"));

                var localDate = DateTime.Parse(selI[..^4]);

                // Get the current local date and time
                var currentDate = DateTime.Now;

                // Calculate the days passed
                var timeSpan = currentDate - localDate;
                var daysPassed = timeSpan.Days;

                if (daysPassed == 0)
                {
                    //return $"OK (Last optimized: today)";
                    return $"正常（今日已优化）";
                }

                if (daysPassed == 1)
                {
                    //return $"OK (Last optimized: yesterday)";
                    return $"正常（1 天前已优化）";
                }

                if (daysPassed < 50)
                {
                    //return $"OK (Last optimized: {daysPassed} days ago)";
                    return $"正常（{daysPassed} 天前已优化）";
                }

                if (daysPassed >= 50)
                {
                    //return $"Needs optimization (Last optimized: {daysPassed} days ago)";
                    return $"需要优化（距上次优化已有 {daysPassed} 天）";
                }

                else
                {
                    return "未知";
                }
            }
            else
            {
                return "请选择一项";
            }
        }
        catch
        {
            return "需要优化";
        }
    }

    public void Lock(bool areItemsEnabled, string message = "", bool indeterminate = true)
    {
        AnalyzeButton.IsEnabled = areItemsEnabled;
        OptimizeButton.IsEnabled = areItemsEnabled;
        MyListView.IsEnabled = areItemsEnabled;
        AdvancedView.IsEnabled = areItemsEnabled;
        CurrentDisk.Visibility = areItemsEnabled == true ? Visibility.Collapsed : Visibility.Visible;
        if (!string.IsNullOrWhiteSpace(message))
        {
            CurrentDisk.Text = message;
        }

        CurrentProgress.IsIndeterminate = indeterminate == true && !areItemsEnabled;
        VisualStateManager.GoToState(OptimizeButton, areItemsEnabled == true ? "Normal" : "Disabled", true);
    }

    public async Task LoadData(bool loadSystemPartitions)
    {
        Lock(false, "正在加载驱动器信息...");

        // Initial delay
        // Essential for ensuring the UI loads before starting tasks
        await Task.Delay(100);

        List<DiskItem> items = [];

        // Get the logical drives bitmask
        var drivesBitMask = Win32Helper.GetLogicalDrives();
        if (drivesBitMask == 0)
        {
            return;
        }

        for (var driveLetter = 'A'; driveLetter <= 'Z'; driveLetter++)
        {
            var mask = 1u << (driveLetter - 'A');
            if ((drivesBitMask & mask) != 0)
            {
                var drive = $"{driveLetter}:\\";

                StringBuilder volumeName = new(261);
                StringBuilder fileSystemName = new(261);
                if (Win32Helper.GetVolumeInformation(drive, volumeName, volumeName.Capacity, out _, out _, out _, fileSystemName, fileSystemName.Capacity))
                {
                    var newDriveLetter = drive.ToString().Remove(2, 1);
                    var mediaType = GetDriveTypeDescriptionAsync(drive);

                    if (volumeName.ToString() != string.Empty)
                    {
                        var item = new DiskItem
                        {
                            Name = $"{volumeName} ({newDriveLetter})",
                            ImagePath = "ms-appx:///Assets/Drive.png",
                            MediaType = mediaType,
                            DriveLetter = drive,
                        };
                        if (item.MediaType == "可移动存储")
                        {
                            item.ImagePath = "ms-appx:///Assets/DriveRemovable.png";
                        }
                        if (item.MediaType == "未知")
                        {
                            item.ImagePath = "ms-appx:///Assets/DriveUnknown.png";
                        }
                        if (item.MediaType == "光盘")
                        {
                            item.ImagePath = "ms-appx:///Assets/DriveOptical.png";
                        }
                        if (item.DriveLetter.Contains('C'))
                        {
                            item.ImagePath = "ms-appx:///Assets/DriveWindows.png";
                        }
                        item.IsChecked = GetBoolFromLocalSettings(ConvertStringToNumericRepresentation(drive));
                        items.Add(item);
                    }
                    else
                    {
                        var item = new DiskItem
                        {
                            Name = $"({newDriveLetter})",
                            ImagePath = "ms-appx:///Assets/Drive.png",
                            MediaType = mediaType,
                            DriveLetter = drive,
                        };
                        if (item.MediaType == "可移动存储")
                        {
                            item.ImagePath = "ms-appx:///Assets/DriveRemovable.png";
                        }
                        if (item.MediaType == "未知")
                        {
                            item.ImagePath = "ms-appx:///Assets/DriveUnknown.png";
                        }
                        if (item.MediaType == "光盘")
                        {
                            item.ImagePath = "ms-appx:///Assets/DriveOptical.png";
                        }
                        if (item.DriveLetter.Contains('C'))
                        {
                            item.ImagePath = "ms-appx:///Assets/DriveWindows.png";
                        }
                        item.IsChecked = GetBoolFromLocalSettings(ConvertStringToNumericRepresentation(drive));
                        items.Add(item);
                    }
                }
                else
                {
                    Debug.WriteLine($"  Failed to get volume information for {drive}");
                }
            }
        }

        if (loadSystemPartitions)
        {
            var syspart = SystemVolumes.GetSystemVolumes();

            // Add system partitions to the items list
            foreach (var result in syspart)
            {
                var driveType = string.Empty;
                foreach (var diskitem in items)
                {
                    if (diskitem.DriveLetter != null && diskitem.DriveLetter.Contains('C'))
                    {
                        driveType = diskitem.MediaType;
                    }
                }
                var item = new DiskItem
                {
                    Name = result.FriendlyName,
                    ImagePath = "ms-appx:///Assets/DriveSystem.png",
                    MediaType = driveType,
                    DriveLetter = result.GUID,
                };
                if (result.GUID != null)
                {
                    item.IsChecked = GetBoolFromLocalSettings(ConvertStringToNumericRepresentation(result.GUID));
                }

                items.Add(item);
            }
        }

        var selIndex = MyListView.SelectedIndex is not -1 ? MyListView.SelectedIndex : 0;

        // Set the list view's item source
        MyListView.ItemsSource = items;

        MyListView.SelectedIndex = selIndex >= items.Count ? items.Count - 1 : selIndex;

        Lock(true);
    }

    public static string ConvertStringToNumericRepresentation(string input)
    {
        // Create a StringBuilder to store the numeric representation
        StringBuilder numericRepresentation = new();

        // Iterate over each character in the string
        foreach (var c in input)
        {
            // Convert the character to its ASCII value and append it
            numericRepresentation.Append((int)c);
        }

        // Return the numeric representation as a string
        return numericRepresentation.ToString();
    }

    // Method to read a bool value from LocalSettings
    public static bool GetBoolFromLocalSettings(string name)
    {
        // Access the local settings
        var localSettings = ApplicationData.Current.LocalSettings;

        // Check if the key exists and return the bool value if it does
        if (localSettings.Values.ContainsKey(name))
        {
            // Try to get the value and cast it to bool
            if (localSettings.Values[name] is bool value)
            {
                return value;
            }
        }

        // Return false if the key does not exist or is not a bool
        return false;
    }

    // Method to write a bool value to LocalSettings
    public static bool WriteBoolToLocalSettings(string name, bool value)
    {
        try
        {
            // Access the local settings
            var localSettings = ApplicationData.Current.LocalSettings;

            // Write the bool value to the settings with the given key
            localSettings.Values[name] = value;

            return true; // Return true if write is successful
        }
        catch
        {
            return false; // Return false if an error occurs
        }
    }

    public static string GetDiskDriveFromLetter(string driveLetter)
    {
        try
        {
            var FULLSEARCHER = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_PhysicalDisk");
            foreach (var queryObj in FULLSEARCHER.Get().Cast<ManagementObject>())
            {
                var MEDIATYPE = queryObj["MediaType"].ToString();
                var DEVICEID = queryObj["DeviceID"].ToString();

                // Create a ManagementObjectSearcher to query the Win32_DiskDrive class
                ManagementObjectSearcher searcher2 = new("SELECT * FROM Win32_DiskDrive");

                foreach (var disk in searcher2.Get().Cast<ManagementObject>())
                {
                    // Get the device ID of the disk
                    var deviceID2 = disk["DeviceID"].ToString();

                    // Create a ManagementObjectSearcher to query the Win32_LogicalDiskToPartition class
                    ManagementObjectSearcher partitionSearcher = new(
                        $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{deviceID2}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition");

                    foreach (var partition in partitionSearcher.Get().Cast<ManagementObject>())
                    {
                        // Create a ManagementObjectSearcher to query the Win32_LogicalDisk class
                        ManagementObjectSearcher logicalDiskSearcher = new(
                            $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_LogicalDiskToPartition");

                        foreach (var logicalDisk in logicalDiskSearcher.Get().Cast<ManagementObject>())
                        {
                            // Check if the logical disk is drive C
                            if (logicalDisk["DeviceID"].ToString() == driveLetter.Replace(@"\", ""))
                            {
                                if (deviceID2 != null && DEVICEID != null && deviceID2.Contains(DEVICEID))
                                {
                                    var driveType = MEDIATYPE switch
                                    {
                                        "3" => "HDD（机械硬盘）",
                                        "4" => "SSD（固态硬盘）",
                                        _ => "未知"
                                    };

                                    return driveType;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {

        }
        return "错误";
    }

    private static async Task<string?> GetDeviceIdFromDriveAsync(string driveRoot)
    {
        var driveLetter = driveRoot.TrimEnd('\\'); // Clean the input

        return await Task.Run(async () =>
        {
            var query = $"SELECT * FROM Win32_DiskDrive";
            using var searcher = new ManagementObjectSearcher(query);
            foreach (var logicalDisk in searcher.Get().Cast<ManagementObject>())
            {
                if ((string)logicalDisk["DeviceID"] == await GetDeviceIdFromDriveAsync(driveRoot))
                {
                    return logicalDisk["DeviceID"]?.ToString(); // Return the DeviceID of the disk drive
                }
            }
            return "未知"; // Fallback if not found
        });
    }

    public static string GetDriveTypeDescriptionAsync(string driveRoot)
    {
        var driveType = Win32Helper.GetDriveType(driveRoot);

        return driveType switch
        {
            Win32Helper.DriveType.DRIVE_REMOVABLE => "可移动存储",
            Win32Helper.DriveType.DRIVE_FIXED => GetDiskDriveFromLetter(driveRoot),
            Win32Helper.DriveType.DRIVE_REMOTE => "网络存储",
            Win32Helper.DriveType.DRIVE_CDROM => "光盘",
            Win32Helper.DriveType.DRIVE_RAMDISK => "内存盘",
            Win32Helper.DriveType.DRIVE_NO_ROOT_DIR => "缺少根目录",
            _ => "未知",
        };
    }

    private void Button_Click(object sender, SplitButtonClickEventArgs e)
    {
        OptimizeSelected(AdvancedView.IsOn);
    }

    public static void RestartAsAdmin(string args)
    {
        var packageName = Package.Current.Id.FamilyName;
        var appId = CoreApplication.Id;

        // Request elevation
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"Start-Process 'shell:AppsFolder\\{packageName}!{appId}' -ArgumentList @('{args}') -Verb RunAs\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(startInfo);
            App.Current.Exit();
        }
        catch
        {

        }
    }

    public async void OptimizeSelected(bool systemPartitions)
    {
        AdvancedView.IsOn = systemPartitions;

        if (!IsAdministrator())
        {
            RestartAsAdmin($"SELECTED{(systemPartitions ? "-SYSTEM" : "")} {MyListView.SelectedIndex}");
            return;
        }

        foreach (var item in (List<DiskItem?>)MyListView.ItemsSource)
        {
            if (item?.IsChecked == true)
            {
                Lock(false, "运行中...", true);

                MyListView.SelectedIndex = ((List<DiskItem?>)MyListView.ItemsSource).IndexOf(item);

                var volume = item?.DriveLetter?.ToString().Remove(1, 2);
                var arguments = $@"
$global:OutputLines = @()

Optimize-Volume -DriveLetter {volume} {(item?.MediaType?.Contains("HDD") == true ? "-Defrag" : "-Retrim")} -Verbose | ForEach-Object {{
    if ($_ -like '*Progress*') {{
        Write-Output ""Progress: $_""
    }} else {{
        $global:OutputLines += $_
        Write-Output $_
    }}
}}

$global:OutputLines

";

                try
                {
                    if (DetailsBar.Severity == InfoBarSeverity.Error)
                    {
                        LoadSelectedItemInfo("不可优化，跳过...");
                        await Task.Delay(500);
                        continue;
                    }
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -Command \"{arguments}\"",  // Use -File to execute the script
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"  // Run as administrator
                    };

                    using var process = new Process { StartInfo = processInfo };

                    var outputData = "0";
                    var updateData = true;

                    var alreadyUsed = new List<string>();

                    process.OutputDataReceived += UpdateOutput;

                    void UpdateOutput(object sender, DataReceivedEventArgs args)
                    {
                        // Only process if there's data
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            // Use the dispatcher to update the UI
                            DispatcherQueue.TryEnqueue(() => { UpdateIO(args.Data); });

                            // Store the output data
                            outputData = "\n" + args.Data;
                        }
                    }

                    process.Start();
                    process.BeginOutputReadLine();

                    Lock(false, "", true);
                    LoadSelectedItemInfo("正在优化...");

                    void UpdateIO(string data)
                    {
                        if (!updateData)
                        {
                            return;
                        }

                        if (data.Contains("VERBOSE: ") && (data.Contains(" complete.") || data.Contains(" 完成。")))
                        {
                            var a = data[data.LastIndexOf("VERBOSE: ")..].Replace("VERBOSE: ", string.Empty);

                            var dataToReplace = data.Contains(" 完成。") ? " 完成。" : " complete.";

                            DispatcherQueue.TryEnqueue(() => { RunUpdate(a, dataToReplace); });
                        }
                    }

                    void RunUpdate(string a, string dataToReplace)
                    {
                        if (alreadyUsed.Contains(a) != true)
                        {
                            alreadyUsed.Add(a);
                            CurrentProgress.Value = GetMaxPercentage(a);
                            CurrentProgress.IsIndeterminate = false;
                            SetProgressState(TaskbarProgressBarState.Normal);
                            SetProgressValue((int)CurrentProgress.Value, (int)CurrentProgress.Maximum);
                            if (a.Contains(" complete...") || a.Contains(" 完成..."))
                            {
                                dataToReplace = a.Contains(" 完成...") ? " 完成..." : " complete...";
                                CurrentDisk.Text = item?.DriveLetter?.ToString().Contains('}') != true
                                    ? $"驱动器 {volume}: - {a.Remove(a.IndexOf(dataToReplace))}"
                                    : $"{((DiskItem)MyListView.SelectedItem).Name} - {a.Remove(a.IndexOf(dataToReplace))}";
                            }
                            else if (a.Contains(" complete.") || a.Contains(" 完成。"))
                            {
                                dataToReplace = a.Contains(" 完成。") ? " 完成。" : " complete.";
                                CurrentDisk.Text = item?.DriveLetter?.ToString().Contains('}') != true
                                    ? $"驱动器 {volume}: - {a.Remove(a.IndexOf(dataToReplace))}"
                                    : $"{((DiskItem)MyListView.SelectedItem).Name} - {a.Remove(a.IndexOf(dataToReplace))}";

                            }
                        }
                    }

                    await process.WaitForExitAsync();

                    updateData = false;
                    Lock(true);
                    LoadSelectedItemInfo(GetStatus());
                    alreadyUsed.Clear();
                    CurrentProgress.Value = 0;
                    SetProgressState(TaskbarProgressBarState.NoProgress);
                }
                catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
                {
                    ShowMessage("用户取消了碎片整理。");
                }
                catch (Exception ex)
                {
                    ShowMessage($"错误：{ex.Message}");
                }
            }
        }
    }

    private static int GetMaxPercentage(string data)
    {
        static string KeepOnlyNumbers(string input)
        {
            StringBuilder sb = new();

            foreach (var c in input)
            {
                if (char.IsDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
        var match = KeepOnlyNumbers(data);
        return int.Parse(match);
    }

    public async void OptimizeAll(bool close, bool systemPartitions)
    {
        AdvancedView.IsOn = systemPartitions;

        await LoadData(systemPartitions);

        MyListView.IsEnabled = false;

        if (!IsAdministrator())
        {
            if (close == true)
            {
                RestartAsAdmin($"OPTIMIZEALLANDCLOSE{(systemPartitions ? "-SYSTEM" : "")}");
            }
            else
            {
                RestartAsAdmin($"OPTIMIZEALL{(systemPartitions ? "-SYSTEM" : "")}");
            }

            return;
        }

        var i = 0;
        var j = ((List<DiskItem>)MyListView.ItemsSource).Count;

        MyListView.SelectedIndex = 0;

        foreach (var item in (List<DiskItem>)MyListView.ItemsSource)
        {
            Lock(false, "正在优化...", true);

            var volume = item.DriveLetter?.ToString().Remove(1, 2);
            var arguments = $@"
$global:OutputLines = @()

Optimize-Volume -DriveLetter {volume} {(item?.MediaType?.Contains("HDD") == true ? "-Defrag" : "-Retrim")} -Verbose | ForEach-Object {{
    if ($_ -like '*Progress*') {{
        Write-Output ""Progress: $_""
    }} else {{
        $global:OutputLines += $_
        Write-Output $_
    }}
}}

$global:OutputLines

";

            try
            {
                i++;

                if (DetailsBar.Severity == InfoBarSeverity.Error)
                {
                    LoadSelectedItemInfo("不可优化，跳过...");
                    await Task.Delay(500);
                    if (MyListView.SelectedIndex + 1 != j)
                    {
                        MyListView.SelectedIndex++;
                    }

                    continue;
                }
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -Command \"{arguments}\"",  // Use -File to execute the script
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"  // Run as administrator
                };

                using var process = new Process { StartInfo = processInfo };

                var outputData = "0";
                var updateData = true;

                var alreadyUsed = new List<string>();

                process.OutputDataReceived += UpdateOutput;

                void UpdateOutput(object sender, DataReceivedEventArgs args)
                {
                    // Only process if there's data
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        // Use the dispatcher to update the UI
                        DispatcherQueue.TryEnqueue(() => { UpdateIO(args.Data); });

                        // Store the output data
                        outputData = "\n" + args.Data;
                    }
                }

                process.Start();
                process.BeginOutputReadLine();

                Lock(false, "", true);
                LoadSelectedItemInfo("正在优化...");

                void UpdateIO(string data)
                {
                    if (!updateData)
                    {
                        return;
                    }

                    if (data.Contains("VERBOSE: ") && data.Contains(" complete.") || data.Contains(" 完成。"))
                    {
                        var a = data[data.LastIndexOf("VERBOSE: ")..].Replace("VERBOSE: ", string.Empty);

                        var dataToReplace = data.Contains(" 完成。") ? " 完成。" : " complete.";

                        DispatcherQueue.TryEnqueue(() => { RunUpdate(a, dataToReplace); });
                    }
                }

                void RunUpdate(string a, string dataToReplace)
                {
                    if (alreadyUsed.Contains(a) != true)
                    {
                        alreadyUsed.Add(a);
                        CurrentProgress.IsIndeterminate = false;
                        CurrentProgress.Value = GetMaxPercentage(a);
                        SetProgressState(TaskbarProgressBarState.Normal);
                        SetProgressValue((int)CurrentProgress.Value, (int)CurrentProgress.Maximum);
                        if (a.Contains(" complete...") || a.Contains(" 完成..."))
                        {
                            dataToReplace = a.Contains(" 完成...") ? " 完成..." : " complete...";
                            CurrentDisk.Text = item?.DriveLetter?.ToString().Contains('}') != true
                                ? $"驱动器 {i}/{j} ({volume}:) - {a.Remove(a.IndexOf(dataToReplace))}"
                                : $"{((DiskItem)MyListView.SelectedItem).Name} ({i}/{j}) - {a.Remove(a.IndexOf(dataToReplace))}";
                        }
                        else if (a.Contains(" complete.") || a.Contains(" 完成。"))
                        {
                            dataToReplace = a.Contains(" 完成。") ? " 完成。" : " complete.";
                            CurrentDisk.Text = item?.DriveLetter?.ToString().Contains('}') != true
                                ? $"驱动器 {i}/{j} ({volume}:) - {a.Remove(a.IndexOf(dataToReplace))}"
                                : $"{((DiskItem)MyListView.SelectedItem).Name} ({i}/{j}) - {a.Remove(a.IndexOf(dataToReplace))}";

                        }
                    }
                }

                await process.WaitForExitAsync();

                updateData = false;
                Lock(true, "", false);
                LoadSelectedItemInfo(GetStatus());
                alreadyUsed.Clear();
                CurrentProgress.Value = 0;
                SetProgressState(TaskbarProgressBarState.NoProgress);
                if (MyListView.SelectedIndex + 1 != j)
                {
                    MyListView.SelectedIndex++;
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                ShowMessage("用户取消了碎片整理。");
            }
            catch (Exception ex)
            {
                ShowMessage($"错误：{ex.Message}");
            }
        }

        i = 0;

        MyListView.IsEnabled = true;

        if (close == true)
        {
            Close();
        }
    }

    private async void ShowMessage(string message)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "碎片整理",
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = Content.XamlRoot
            };
            _ = await dialog.ShowAsync();
        }
        catch
        {

        }
    }

    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        OptimizeSelected(AdvancedView.IsOn);
    }

    private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
    {
        OptimizeAll(false, AdvancedView.IsOn);
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        if (IsAdministrator() != true)
        {
            RestartAsAdmin($"TASK");
            return;
        }

        OpenTaskWindow();
    }

    public async void CheckTask()
    {
        await Task.Delay(100);
        try
        {
            ScheduledOptimizationDetails.Text = GetTaskFrequency();
            if (GetTaskFrequency() is not "关闭")
            {
                ScheduledTaskText.Text = "配置";
            }
            else
            {
                ScheduledTaskText.Text = "开启";
            }

            CheckTask();
        }
        catch
        {

        }
    }

    public void OpenTaskWindow()
    {
        var win = new ScheduledOptimization(AppWindow.Position.X, AppWindow.Position.Y);
        Win32Helper.CreateModalWindow(this, win, true, true);
    }

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {
        LoadSelectedItemInfo(GetStatus());
    }

    private async void AdvancedView_Toggled(object sender, RoutedEventArgs e)
    {
        WriteBoolToLocalSettings("Advanced", AdvancedView.IsOn);
        await LoadData(AdvancedView.IsOn);
    }

    private async void MenuFlyoutItem_Click_2(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://ivirius.vercel.app/docs/rebound11/defragment-and-optimize-drives/"));
    }

    private void CheckBox_Click(object? sender, RoutedEventArgs e)
    {
        var name = ((DiskItem?)((CheckBox?)sender)?.DataContext)?.DriveLetter;
        var isChecked = ((CheckBox?)sender)?.IsChecked;
        if (name != null && isChecked != null)
        {
            WriteBoolToLocalSettings(ConvertStringToNumericRepresentation(name), (bool)isChecked);
        }
    }

    public static string GetTaskFrequency()
    {
        using TaskService ts = new();
        // Specify the path to the task in Task Scheduler
        var defragFolder = ts.GetFolder(@"Microsoft\Windows\Defrag");

        // Retrieve the scheduled task
        var task = defragFolder.GetTasks()["ScheduledDefrag"];

        if (task.Enabled != true)
        {
            return $"关闭";
        }

        if (task != null)
        {
            // Check the triggers for their type
            foreach (var trigger in task.Definition.Triggers)
            {
                switch (trigger)
                {
                    case DailyTrigger _:
                        return "开启（频率：每日）";
                    case WeeklyTrigger _:
                        return "开启（频率：每周）";
                    case MonthlyTrigger _:
                        return "开启（频率：每月）";
                }
            }

            return "开启（频率：未知）";
        }
        else
        {
            return $"关闭";
        }
    }

    public static string GetTaskCommand()
    {
        using TaskService ts = new();
        // Specify the path to the task in Task Scheduler
        var defragFolder = ts.GetFolder(@"Microsoft\Windows\Defrag");

        // Retrieve the scheduled task
        var task = defragFolder.GetTasks()["ScheduledDefrag"];

        if (task != null)
        {
            // Check the triggers for their type
            foreach (var action in task.Definition.Actions)
            {
                if (action is ExecAction ex)
                {
                    return ex.Arguments;
                }
            }

            return "None";
        }
        else
        {
            return $"None";
        }
    }

    private void MenuFlyoutItem_Click_3(object sender, RoutedEventArgs e)
    {
        Process.Start("dfrgui.exe");
        Close();
    }
}

public class DefragInfo
{
    public static List<string> GetEventLogEntriesForID(int eventID)
    {
        List<string> eventMessages = [];

        // Define the query
        var logName = "Application"; // Windows Logs > Application
        var queryStr = "*[System/EventID=" + eventID + "]";

        EventLogQuery query = new(logName, PathType.LogName, queryStr);

        // Create the reader
        using (EventLogReader reader = new(query))
        {
            for (var eventInstance = reader.ReadEvent(); eventInstance != null; eventInstance = reader.ReadEvent())
            {
                // Extract the message from the event
                var sb = string.Concat(eventInstance.TimeCreated.ToString(), eventInstance.FormatDescription().ToString().AsSpan(eventInstance.FormatDescription().ToString().Length - 4));

                eventMessages.Add(sb.ToString());
            }
        }

        return eventMessages;
    }
}
