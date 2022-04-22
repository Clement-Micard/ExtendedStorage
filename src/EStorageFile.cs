using ExtendedStorage.Enums;
using ExtendedStorage.Extensions;
using ExtendedStorage.Helpers;
using System;
using System.IO;
using System.Runtime.InteropServices;
using static ExtendedStorage.Win32;
using FileAttributes = System.IO.FileAttributes;

namespace ExtendedStorage
{
    public sealed class EStorageFile : EStorageItem
    {
        public string DisplayName { get; internal set; }
        public string FileType { get; internal set; }

        internal EStorageFile(FileAttributes attributes, DateTimeOffset dateCreated, string fileType, string name, string path)
        {
            Attributes = attributes;
            DateCreated = dateCreated;
            FileType = fileType;
            Name = name;
            Path = path;
        }

        /// <summary>
        /// Get an ExtendedStorageFile from a path
        /// </summary>
        /// <param name="path">Path where the file is</param>
        /// <returns>Returns the file as ExtendedStorageFile if exists</returns>
        public static EStorageFile GetFromPath(string path)
        {
            bool exists = GetFileAttributesExFromApp(path, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out WIN32_FILE_ATTRIBUTE_DATA fileData);
            if (!exists)
            {
                return null;
            }

            if (fileData.dwFileAttributes.HasFlag(FileAttributes.Directory) == false)
            {
                return new EStorageFile(fileData.dwFileAttributes, DiskHelpers.GetCreationDate(fileData.ftCreationTime.dwHighDateTime, fileData.ftCreationTime.dwHighDateTime), System.IO.Path.GetExtension(path), System.IO.Path.GetFileName(path), path.FormatPath());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get an ExtendedStorageFile from application installation path
        /// </summary>
        /// <param name="path">Path where the file is</param>
        /// <returns>Returns the file as ExtendedStorageFile if exists</returns>
        public static EStorageFile GetFromApplicationFolder(string path)
        {
            return GetFromPath(Windows.ApplicationModel.Package.Current.InstalledLocation.Path + path);
        }

        /// <summary>
        /// Move the EStorageFile in another destination
        /// </summary>
        /// <param name="destination">The destination as EStorageFolder</param>
        /// <param name="name">The name of the new file (must include the format, eg: myImage.png)</param>
        /// <returns></returns>
        public EStorageFile MoveTo(EStorageFolder destination, string name)
        {
            if (this != null && destination != null && name != null)
            {
                MoveFileFromApp(Path, $"{destination.Path}\\{name}");
                CopyFilePermission(destination.Path);
                return GetFromPath(destination.Path);
            }
            return null;
        }

        /// <summary>
        /// Open the EStorageFile as Stream with specified parameters
        /// </summary>
        /// <param name="source">The source EStorageFile</param>
        /// <param name="accessMode">File access mode</param>
        /// <param name="shareMode">File share mode</param>
        /// <returns>Returns an Stream if success, else null.</returns>
        public Stream OpenAsStream(FileAccess accessMode)
        {
            return Win32.OpenAsStream(Path, accessMode);
        }

        /// <summary>
        /// Copy the EStorageFile in another destination
        /// </summary>
        /// <param name="destination">The destination as EStorageFolder</param>
        /// <param name="name">The name of the new file (must include the format, eg: myImage.png)</param>
        /// <returns></returns>
        public EStorageFile CopyTo(EStorageFolder destination, string name = "", FileAlreadyExists exists = FileAlreadyExists.DoNothing)
        {
            if (this != null && destination != null & name != null)
            {
                if (name == string.Empty)
                {
                    name = Name;
                }

                if (exists == FileAlreadyExists.ReplaceExisting)
                {
                    CopyFileFromApp(Path, $"{destination.Path}\\{name}".FormatPath(), false);
                }
                else if (exists == FileAlreadyExists.DoNothing)
                {
                    CopyFileFromApp(Path, $"{destination.Path}\\{name}".FormatPath(), true); // No replacement, fails but still returns.
                }
                else
                {
                    bool success = CopyFileFromApp(Path, $"{destination.Path}\\{name}".FormatPath(), true);
                    if (success == false && Marshal.GetLastWin32Error() == 183 && exists == FileAlreadyExists.GenerateUniqueName)
                    {
                        for (int i = 2; i < int.MaxValue; i++)
                        {
                            if (CopyFileFromApp(Path, $"{destination.Path}\\{name} ({i})".FormatPath(), true))
                            {
                                CopyFilePermission($"{destination.Path}\\{name} ({i})".FormatPath());
                                return GetFromPath($"{destination.Path}\\{name} ({i})".FormatPath());
                            }
                        }
                    }
                }

                // Inherit file permissions from parent folder
                CopyFilePermission(destination.Path);
                return GetFromPath(destination.Path);
            }
            return null;
        }

        /// <summary>
        /// Delete the current EStorageFile.
        /// </summary>
        /// <returns>Returns wether the file was deleted or not.</returns>
        public bool Delete()
        {
            return DeleteFileFromApp(Path);
        }
    }
}