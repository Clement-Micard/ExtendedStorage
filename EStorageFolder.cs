using ExtendedStorage.Enums;
using ExtendedStorage.Extensions;
using ExtendedStorage.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using FileAttributes = System.IO.FileAttributes;
using static ExtendedStorage.Win32;

namespace ExtendedStorage
{
    public class EStorageFolder : EStorageItem
    {
        internal EStorageFolder(FileAttributes attributes, DateTimeOffset dateCreated, string name, string path)
        {
            Attributes = attributes;
            DateCreated = dateCreated;
            Name = name;
            Path = path;
        }

        /// <summary>
        /// Get an ExtendedStorageFolder from path
        /// </summary>
        /// <param name="path">The path of the folder (Ex: C:\MyFolder)</param>
        /// <returns>Returns the folder as ExtendedStorageFolder if the folder exists, else it returns null</returns>
        public static EStorageFolder GetFromPath(string path)
        {
            EStorageFolder folderToReturn;
            if (IsFolder(path))
            {
                DateTimeOffset cdate = DateTimeOffset.Now;
                if (GetFileAttributesExFromApp(path, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out WIN32_FILE_ATTRIBUTE_DATA fileData))
                {
                    cdate = DiskHelpers.GetCreationDate(fileData.ftCreationTime.dwHighDateTime, fileData.ftCreationTime.dwHighDateTime);
                }

                FileAttributes attributes = fileData.dwFileAttributes;
                folderToReturn = new EStorageFolder(attributes, cdate, new DirectoryInfo(path).Name, path.FormatPath());
            }
            else
            {
                folderToReturn = null;
            }
            return folderToReturn;
        }

        /// <summary>
        /// Move the ExtendedStorageFolder to another destination
        /// </summary>
        /// <param name="destination">The destination (Ex: C:\MyFolder\myNewFolder)</param>
        /// <param name="creationCollision">The action executed if the folder already exists (Default: FailIfExists))</param>
        /// <returns>Returns the new destination as ExtendedStorageFolder if successful, returns null by default if NameCollisionOption was not defined.</returns>
        public void MoveTo(EStorageFolder destination, string newName = null)
        {
            if (newName == null)
            {
                destination = destination.CreateFolder(Name);
            }
            else
            {
                destination = destination.CreateFolder(newName);
            }

            // Avoid using EStorageFile and EStorageFolder, for less memory consumption and quicker process.
            IntPtr hFile = FindFirstFileExFromApp($"{Path}\\*.*", FINDEX_INFO_LEVELS.FindExInfoBasic, out WIN32_FIND_DATA findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);
            do
            {
                if (hFile.ToInt64() != -1)
                {
                    // Skip root folders
                    if (findData.cFileName.Equals(".") || findData.cFileName.Equals(".."))
                    {
                        continue;
                    }

                    // Files
                    if (!((FileAttributes)findData.dwFileAttributes & FileAttributes.Directory).HasFlag(FileAttributes.Directory))
                    {
                        CopyFileFromApp($"{Path}\\{findData.cFileName}", $"{destination.Path}\\{findData.cFileName}", false);
                        CopyFilePermission(destination.Path);
                        DeleteFileFromApp(Path);
                    }

                    // Folders
                    if (((FileAttributes)findData.dwFileAttributes).HasFlag(FileAttributes.Directory))
                    {
                        GetFromPath($"{Path}\\{findData.cFileName}").MoveTo(destination);
                    }
                }
            } while (FindNextFile(hFile, out findData));
            FindClose(hFile);
        }

        /// <summary>
        /// Copy the ExtendedStorageFolder to another destination
        /// </summary>
        /// <param name="destination">The destination as EStorageFolder</param>
        public void CopyTo(EStorageFolder destination, string name)
        {
            if (this == null)
            {
                return;
            }

            EStorageFolder root = destination.CreateFolder(name);
            destination = root;

            FINDEX_INFO_LEVELS findInfoLevel = FINDEX_INFO_LEVELS.FindExInfoBasic;
            int additionalFlags = FIND_FIRST_EX_LARGE_FETCH;
            IntPtr hFile = FindFirstFileExFromApp(Path + "\\*.*", findInfoLevel, out WIN32_FIND_DATA findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, additionalFlags);
            do
            {
                if (hFile.ToInt64() != -1)
                {
                    if (!((FileAttributes)findData.dwFileAttributes & FileAttributes.Directory).HasFlag(FileAttributes.Directory))
                    {
                        CopyFileFromApp($"{Path}\\{findData.cFileName}", $"{destination.Path}\\{name}".FormatPath(), false);
                        CopyFilePermission(destination.Path);
                    }

                    if (((FileAttributes)findData.dwFileAttributes).HasFlag(FileAttributes.Directory))
                    {
                        EStorageFolder folder = GetFromPath($"{Path}\\{findData.cFileName}");
                        folder.CopyTo(destination, folder.Name);
                    }
                }
            } while (FindNextFile(hFile, out findData));
            FindClose(hFile);
        }

        /// <summary>
        /// Create a file in the EStorageFolder
        /// </summary>
        /// <param name="name">Name of the file</param>
        /// <returns>Returns the created file as EStorageFile</returns>
        public EStorageFile CreateFile(string name, FileAlreadyExists alreadyExists = FileAlreadyExists.DoNothing)
        {
            return EStorageFile.GetFromPath(Win32.CreateFile($"{Path}\\{name}", alreadyExists));
        }

        /// <summary>
        /// Create an empty folder on the EStorageFolder
        /// </summary>
        /// <param name="source">The source folder</param>
        /// <param name="name">The name of the folder to create</param>
        /// <param name="alreadyExists">What to do if the folder already exists</param>
        /// <returns>Returns a EStorageFolder if success, else null.</returns>
        public EStorageFolder CreateFolder(string name, FileAlreadyExists alreadyExists = FileAlreadyExists.DoNothing)
        {
            return GetFromPath(Win32.CreateFolder($"{Path}\\{name}", alreadyExists));
        }

        /// <summary>
        /// Get a file from the ExtendedStorageFolder
        /// </summary>
        /// <param name="name">The name of the file to get.</param>
        /// <returns>Returns the file as EStorageFile if exists, else null.</returns>
        public EStorageFile GetFile(string name)
        {
            return EStorageFile.GetFromPath($"{Path}\\{name}");
        }

        /// <summary>
        /// Get a folder from EStorageFolder
        /// </summary>
        /// <param name="name">The name of the folder to get.</param>
        /// <returns>Returns an EStorageFolder, else null.</returns>
        public EStorageFolder GetFolder(string name)
        {
            return GetFromPath($"{Path}\\{name}");
        }

        /// <summary>
        /// Get all folders in an EStorageFolder
        /// </summary>
        /// <returns>Returns the found folders as IReadonlyList<EStorageFolder></returns>
        public IReadOnlyList<EStorageFolder> GetFolders()
        {
            if (this == null)
            {
                return null;
            }

            List<EStorageFolder> folders = new List<EStorageFolder>();
            FINDEX_INFO_LEVELS findInfoLevel = FINDEX_INFO_LEVELS.FindExInfoBasic;
            int additionalFlags = FIND_FIRST_EX_LARGE_FETCH;
            IntPtr hFile = FindFirstFileExFromApp(Path + "\\*.*", findInfoLevel, out WIN32_FIND_DATA findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, additionalFlags);
            if (hFile.ToInt64() != -1)
            {
                do
                {
                    if (findData.cFileName == @"." || findData.cFileName == @"..")
                    {
                        continue;
                    }

                    if (((FileAttributes)findData.dwFileAttributes).HasFlag(FileAttributes.Directory))
                    {
                        folders.Add(GetFromPath((Path + @"\" + findData.cFileName).FormatPath())); // Add the folder to the list
                    }
                } while (FindNextFile(hFile, out findData));
                FindClose(hFile);
            }
            return folders;
        }

        /// <summary>
        /// Get all files in an EStorageFolder
        /// </summary>
        /// <returns>Returns the files as IReadOnlyList<EStorageFile></returns>
        public IReadOnlyList<EStorageFile> GetFiles()
        {
            List<EStorageFile> files = new List<EStorageFile>();
            FINDEX_INFO_LEVELS findInfoLevel = FINDEX_INFO_LEVELS.FindExInfoBasic;
            int additionalFlags = FIND_FIRST_EX_LARGE_FETCH;
            IntPtr hFile = FindFirstFileExFromApp(Path + "\\*.*", findInfoLevel, out WIN32_FIND_DATA findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero,
                additionalFlags);
            do
            {
                if (hFile.ToInt64() != -1)
                {
                    if (!((FileAttributes)findData.dwFileAttributes & FileAttributes.Directory).HasFlag(FileAttributes.Directory))
                    {
                        files.Add(EStorageFile.GetFromPath((Path + @"\" + findData.cFileName).FormatPath())); // Add the file to the list
                    }
                }
            } while (FindNextFile(hFile, out findData));
            FindClose(hFile);
            return files;
        }

        /// <summary>
        /// Get all files and folders in a EStorageFolder.
        /// </summary>
        /// <param name="source">Source folder.</param>
        /// <returns>Returns a list of all found items.</returns>
        public IReadOnlyList<EStorageItem> GetItems()
        {
            List<EStorageItem> items = new List<EStorageItem>(GetFiles());
            items.AddRange(GetFolders());
            return items;
        }
    }
}