using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32;
using Windows.ApplicationModel.DataTransfer;
using WinUIEx;

namespace Rebound.About
{
    public sealed partial class MainWindow : WindowEx
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AppWindow.DefaultTitleBarShouldMatchAppModeTheme = true;
            this.IsMaximizable = false;
            this.IsMinimizable = false;
            this.MinWidth = 650;
            this.MoveAndResize(25, 25, 650, 690);
            this.Title = "关于 Windows";
            this.IsResizable = false;
            this.SystemBackdrop = new MicaBackdrop();
            this.SetIcon($"{AppContext.BaseDirectory}\\Assets\\Rebound.ico");
            User.Text = GetCurrentUserName();
            Version.Text = GetDetailedWindowsVersion();
            LegalStuff.Text = GetLegalInfo();
            Load();
        }

        public async void Load()
        {
            await Task.Delay(100);

            this.SetWindowSize(WinverPanel.ActualWidth + 60, 690);
        }

        public static string GetDetailedWindowsVersion()
        {
            try
            {
                // Open the registry key
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        // Retrieve build number and revision
                        var versionName = key.GetValue("DisplayVersion", "未知") as string;
                        var buildNumber = key.GetValue("CurrentBuildNumber", "未知") as string;
                        var buildLab = key.GetValue("UBR", "未知");

                        return $"版本 {versionName} (OS 内部版本 {buildNumber}.{buildLab})";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"无法获取系统版本号: {ex.Message}";
            }

            return "未找到注册表键";
        }

        public string GetLegalInfo()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

                foreach (ManagementObject os in searcher.Get().Cast<ManagementObject>())
                {
                    var caption = os["Caption"];
                    var version = os["Version"];
                    var buildNumber = os["BuildNumber"];

                    if (caption.ToString().Contains("10")) windowsVer = "Windows 10";
                    else windowsVer = "Windows 11";

                    WindowsVer.Text = caption.ToString().Replace("Microsoft ", "");

                    return $"{caption.ToString().Replace("Microsoft ", "")}操作系统及其用户界面受美国和其他国家/地区的商标法和其他待颁布或已颁布的知识产权法保护。";
                }
            }
            catch (Exception ex)
            {
                return $"无法获取系统版本类别: {ex.Message}";
            }

            return "WMI 查询未返回结果";
        }

        public static string GetCurrentUserName()
        {
            try
            {
                // Open the registry key
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        // Retrieve current username
                        var owner = key.GetValue("RegisteredOwner", "未知") as string;

                        return owner;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"无法获取用户名: {ex.Message}";
            }

            return "未找到注册表键";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var info = new ProcessStartInfo()
            {
                FileName = "powershell",
                Arguments = "winver",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var proc = Process.Start(info);

            await proc.WaitForExitAsync();

            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        string windowsVer = "Windows";

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string content = $@"==========================
---Microsoft {windowsVer}---
==========================

{GetDetailedWindowsVersion()}
� Microsoft Corporation。保留所有权利。

{GetLegalInfo()}

根据 [Microsoft 软件许可条款] (https://support.microsoft.com/zh-cn/windows/microsoft-software-license-terms-e26eedad-97a2-5250-2670-aad156b654bd)，许可以下用户使用本产品: {GetCurrentUserName()}

==========================
--------Rebound 11--------
==========================

{ReboundVer.Text}

Rebound 11 是一款不会干扰系统运行的 Windows 美化模组。此 Windows 系统已安装 Rebound 11 的附带组件。";
            var package = new DataPackage();
            package.SetText(content);
            Clipboard.SetContent(package);
        }
    }
}
