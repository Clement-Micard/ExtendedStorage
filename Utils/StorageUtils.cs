using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace ExtendedStorage.Utils
{
    public static class StorageUtils
	{
		public static Enums.DriveLetters GetLogicalDrives()
		{
			// Get all logical drives and parse it via the enum
			return (Enums.DriveLetters)Win32.GetLogicalDrives();
		}

		public static async Task<IReadOnlyList<string>> GetRemovableDrives()
		{
			List<string> devicesList = new List<string>();
			foreach (StorageFolder device in await KnownFolders.RemovableDevices.GetFoldersAsync())
			{
				devicesList.Add(device.Name);
			}
			return devicesList;
		}

		public static async Task<IReadOnlyList<string>> GetPermanentDrives()
		{
			// Create empty list of drives
			List<string> drives = new List<string>();

			// Get all logical drives
			Enums.DriveLetters driverLetters = GetLogicalDrives();

			// Loop through all drive letters
			foreach (Enums.DriveLetters value in Enum.GetValues(driverLetters.GetType()))
				// Check if the enum of the drive letter is present
				if (driverLetters.HasFlag(value))
				{
					//add the drive letter to the list
					drives.Add(value.ToString() + ":\\");
				}
			// Remove the removable drives
			drives = drives.Except(await GetRemovableDrives()).ToList();
			// Return the drives
			return drives;
		}
	}
}
