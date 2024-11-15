using System.Diagnostics;

namespace Rebound.TrustedPlatform.Models;
public class TpmReset
{
    public static async Task ResetTpmAsync(ContentDialog dial)
    {
        dial.Content = "运行中...";
        try
        {
            // Path to the PowerShell script
            string scriptPath = Path.Combine(Path.GetTempPath(), "Reset-TPM.ps1");

            // Write the PowerShell script to a temporary file
            File.WriteAllText(scriptPath, @"
            Write-Host 'Starting TPM reset process...'
            Clear-Tpm -ErrorAction Stop
            Write-Host 'TPM reset successfully completed.'
        ");

            // Set up the process to run PowerShell with elevated privileges
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                Verb = "runas", // Run as administrator
                UseShellExecute = true,
                CreateNoWindow = true
            };

            // Start the process and wait for it to exit
            var process = Process.Start(psi);
            await process.WaitForExitAsync();

            // Check the exit code
            if (process.ExitCode == 0)
            {
                // Update InfoBar for success
                dial.Content = "TPM 重置已完成。";
                dial.SecondaryButtonText = "关闭";
            }
            else
            {
                // Update InfoBar for failure
                dial.Content = "TPM 重置失败，请重试。";
                dial.IsPrimaryButtonEnabled = true;
            }
            dial.IsSecondaryButtonEnabled = true;
        }
        catch (Exception ex)
        {
            // Handle exceptions and update InfoBar for failure
            dial.Content = $"操作被用户账户控制（UAC）取消。";
            dial.IsPrimaryButtonEnabled = true;
            dial.IsSecondaryButtonEnabled = true;
        }
    }
}
