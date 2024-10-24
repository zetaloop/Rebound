﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

namespace Rebound.WindowModels;

public static class InstallationWindowModel
{
    public const string FILES_APP = "49306atecsolution.FilesUWP_et10x9a9vyk8t";
    public const string DEFRAG = "54d2a63e-e616-4159-bed6-c776b8a816e1_59a6r38835q7a";
    public const string RUN = "8ab98b2f-6dbe-4358-a752-979d011f968d_59a6r38835q7a";
    public const string WINVER = "039b9731-7b33-49de-bb09-5b81d5978d1c_59a6r38835q7a";
    public const string TPM = "0b347e39-1da3-4fc7-80c2-dbf3603118f3_59a6r38835q7a";
    public const string DISK_CLEANUP = "e8dfd11c-954d-46a2-b700-9cbc6201f056_59a6r38835q7a";

    public static void RestartPC()
    {
        var info = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = "shutdown /r /t 0",
            Verb = "runas"
        };
        _ = Process.Start(info);
    }
}

public static class SystemLock
{
    [DllImport("user32.dll")]
    private static extern void LockWorkStation();

    public static void Lock()
    {
        LockWorkStation();
    }
}

public static class TaskManager
{
    public static void StopTask(string task)
    {
        try
        {
            // Find the explorer.exe process
            var processes = Process.GetProcessesByName(task);
            foreach (var process in processes)
            {
                // Kill the process
                process.Kill();
                process.WaitForExit(); // Ensure the process has exited
            }
        }
        catch
        {

        }
    }

    public static void StartTask(string task)
    {
        try
        {
            // Start a new explorer.exe process
            Process.Start(task);
        }
        catch
        {

        }
    }
}

public class PackageChecker
{
    public async Task<bool> IsPackageInstalled(string packageFamilyName)
    {
        try
        {
            // Create the PowerShell command
            string command = $"Get-AppxPackage -Name {packageFamilyName}";

            // Create a new process to run PowerShell
            using (var process = new Process())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-Command \"{command}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                // Start the process
                process.Start();

                // Read the output
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                // Wait for the process to exit
                await process.WaitForExitAsync();

                // Check if output contains the package family name
                return !string.IsNullOrWhiteSpace(output) && output.Contains(packageFamilyName);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions if needed
            Console.WriteLine($"Error checking package: {ex.Message}");
            return false;
        }
    }
}