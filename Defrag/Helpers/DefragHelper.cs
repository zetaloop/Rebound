using System.Collections.Generic;
using System.Linq;
using System.Management;

#nullable enable

namespace Rebound.Defrag.Helpers;

public class VolumeInfo
{
    public string? GUID
    {
        get; set;
    }
    public string? FileSystem
    {
        get; set;
    }
    public ulong Size
    {
        get; set;
    }
    public string? FriendlyName
    {
        get; set;
    }
}

public class SystemVolumes
{
    public static List<VolumeInfo> GetSystemVolumes()
    {
        List<VolumeInfo> volumes = [];

        var query = "SELECT * FROM Win32_Volume WHERE DriveLetter IS NULL";
        using (ManagementObjectSearcher searcher = new(query))
        {
            foreach (var volume in searcher.Get().Cast<ManagementObject>())
            {
                var volumePath = volume["DeviceID"].ToString(); // This gives the \\?\Volume{GUID} path
                var fileSystem = volume["FileSystem"]?.ToString() ?? "未知";
                var size = (ulong)volume["Capacity"];

                // We can further refine this by querying for EFI, Recovery, etc., based on size and file system
                string friendlyName;
                if (fileSystem == "FAT32" && size < 512 * 1024 * 1024)
                {
                    friendlyName = "EFI 系统分区";
                }
                else if (fileSystem == "NTFS" && size > 500 * 1024 * 1024)
                {
                    friendlyName = "恢复分区";
                }
                else if (fileSystem == "NTFS" && size < 500 * 1024 * 1024)
                {
                    friendlyName = "系统保留分区";
                }
                else
                {
                    friendlyName = "未知系统分区";
                }

                volumes.Add(new VolumeInfo
                {
                    GUID = volumePath,
                    FileSystem = fileSystem,
                    Size = size,
                    FriendlyName = friendlyName
                });
            }
        }

        return volumes;
    }
}
