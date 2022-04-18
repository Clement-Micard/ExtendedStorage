using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using static ExtendedStorage.Win32;

namespace ExtendedStorage.Archive
{
    public class ArchiveCreator
	{
		public bool ThrowException { get; set; } = false;

		/// <summary>
		/// Creates a ZIP archive file from a directory.
		/// </summary>
		/// <param name="source">The folder to compress.</param>
		/// <param name="destination">The output destination file.</param>
		/// <returns></returns>
		public async Task<bool> CreateArchiveFromDirectory(EStorageFolder source, EStorageFile destination)
		{
			if (source == null)
			{
				if (ThrowException)
				{
					throw new NullReferenceException("The source folder was null.");
				}
				else
				{
					return false;
				}
			}

			if (destination == null)
			{
				if (ThrowException)
				{
					throw new NullReferenceException("The destination file was null.");
				}
				else
				{
					return false;
				}
			}

			SafeFileHandle hFile = CreateFileForWrite(destination.Path);
			if (hFile.IsInvalid)
			{
				if (ThrowException)
				{
					throw new InvalidOperationException("The archive couldn't be created.");
				}
				else
				{
					return false;
				}
			}

			using (ZipArchive archive = new ZipArchive(new FileStream(hFile, FileAccess.ReadWrite), ZipArchiveMode.Create))
			{
				foreach (EStorageFolder folder in source.GetFolders())
				{
					try
					{
						await CreateFolderEntry(folder, archive, archive.CreateEntry(folder.Name + "/"));
					}
					catch (Exception exception)
					{
						if (ThrowException)
						{
							throw exception;
						}
						else
						{
							return false;
						}
					}
				}

				foreach (EStorageFile file in source.GetFiles())
				{
					try
					{
						ZipArchiveEntry fileEntry = archive.CreateEntry(file.Name); // Make a new entry
						using (Stream fs = file.OpenAsStream(FileAccess.Read))
						{
							using (Stream entryFs = fileEntry.Open())
							{
								await fs.CopyToAsync(entryFs); // TODO: Optimized way
							}
						}
					}
					catch (Exception exception)
					{
						if (ThrowException)
						{
							throw exception;
						}
						else
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Recursively compresses a folder structure
		private static async Task CreateFolderEntry(EStorageFolder source, ZipArchive archive, ZipArchiveEntry entry)
		{
			if (source != null)
			{
				foreach (EStorageFile file in source.GetFiles())
				{
					// Add new file entry
					ZipArchiveEntry newEntry = archive.CreateEntry(ArchiveHelper.CleanName((entry.FullName + "/" + file.Name).Replace("//", "/")));

					using (Stream fs = file.OpenAsStream(FileAccess.Read))
					{
						using (Stream entryFs = newEntry.Open())
						{
							await fs.CopyToAsync(entryFs);
						}
					}
				}

				foreach (EStorageFolder folder in source.GetFolders())
				{
					ZipArchiveEntry folderEntry = archive.CreateEntry(ArchiveHelper.CleanName((entry.FullName + "/" + folder.Name + "/").Replace("//", "/")));
					await CreateFolderEntry(folder, archive, folderEntry); // Add sub directories
				}
			}
		}
	}
}