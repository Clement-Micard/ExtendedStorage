using ExtendedStorage.Enums;
using ExtendedStorage.Extensions;
using ExtendedStorage.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using static ExtendedStorage.Win32;
using FileAttributes = System.IO.FileAttributes;

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
            bool exists = GetFileAttributesExFromApp(path, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out WIN32_FILE_ATTRIBUTE_DATA fileData);
            if (!exists)
            {
                return null;
            }

            if (fileData.dwFileAttributes.HasFlag(FileAttributes.Directory))
            {
                return new EStorageFolder(fileData.dwFileAttributes, DiskHelpers.GetCreationDate(fileData.ftCreationTime.dwHighDateTime, fileData.ftCreationTime.dwHighDateTime), new DirectoryInfo(path).Name, path.FormatPath());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Move the ExtendedStorageFolder to another destination
        /// </summary>
        /// <param name="destination">The destination (Ex: C:\MyFolder\myNewFolder)</param>
        /// <param name="creationCollision">The action executed if the folder already exists (Default: FailIfExists))</param>
        /// <returns>Returns the new destination as ExtendedStorageFolder if successful, returns null by default if NameCollisionOption was not defined.</returns>
        public void MoveTo(EStorageFolder destination, string newName = "")
        {
            if (newName == "")
            {
                destination = destination.CreateFolder(Name);
            }
            else
            {
                destination = destination.CreateFolder(newName);
            }

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
                        MoveFileFromApp($"{Path}\\{findData.cFileName}", $"{destination.Path}\\{findData.cFileName}");
                        CopyFilePermission(destination.Path);
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
        public IEnumerable<EStorageFolder> GetFolders()
        {
            IntPtr hFile = FindFirstFileExFromApp(Path + "\\*.*", FINDEX_INFO_LEVELS.FindExInfoBasic, out WIN32_FIND_DATA findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);
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
                        yield return GetFromPath((Path + @"\" + findData.cFileName).FormatPath());
                    }
                } while (FindNextFile(hFile, out findData));
                FindClose(hFile);
            }
        }

        /// <summary>
        /// Get all files in an EStorageFolder
        /// </summary>
        /// <returns>Returns the files as IReadOnlyList<EStorageFile></returns>
        public IEnumerable<EStorageFile> GetFiles(string filter = "*.*")
        {
            IntPtr hFile = FindFirstFileExFromApp($"{Path}\\{filter}", FINDEX_INFO_LEVELS.FindExInfoBasic, out WIN32_FIND_DATA findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero,
                FIND_FIRST_EX_LARGE_FETCH);
            do
            {
                if (hFile.ToInt64() != -1)
                {
                    if (!((FileAttributes)findData.dwFileAttributes & FileAttributes.Directory).HasFlag(FileAttributes.Directory))
                    {
                        yield return EStorageFile.GetFromPath($"{Path}\\{findData.cFileName}"); // Return the file.
                    }
                }
            } while (FindNextFile(hFile, out findData));
            FindClose(hFile);
        }

        /// <summary>
        /// Get all files and folders in a EStorageFolder.
        /// </summary>
        /// <param name="source">Source folder.</param>
        /// <returns>Returns a list of all found items.</returns>
        public IEnumerable<EStorageItem> GetItems()
        {
            foreach (EStorageFile file in GetFiles())
            {
                yield return file;
            }

            foreach (EStorageFolder folder in GetFolders())
            {
                yield return folder;
            }
        }

        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <returns>Returns true if the item was deleted, else false.</returns>
        public bool Delete()
        {
            return DeleteDirectory(Path);
        }

        /// <summary>
        /// Deletes an extended file from its path.
        /// </summary>
        /// <param name="itemPath">The item path to delete.</param>
        /// <returns>Returns true if the item was deleted, else false.</returns>
        private bool DeleteDirectory(string itemPath)
        {
            bool isDeleted = false;
            bool isFailed = false;

            FINDEX_INFO_LEVELS findInfoLevel = FINDEX_INFO_LEVELS.FindExInfoBasic;
            int additionalFlags = FIND_FIRST_EX_LARGE_FETCH;
            IntPtr hFile = FindFirstFileExFromApp(itemPath.TrimEnd(new char[] { '\\' }) + @"\*", findInfoLevel, out WIN32_FIND_DATA findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero,
                additionalFlags);

            if (hFile.ToInt64() != -1)
            {
                do
                {
                    string path = string.Format("{0}\\{1}", itemPath.TrimEnd(new char[] { '\\' }), findData.cFileName);

                    if (((FileAttributes)findData.dwFileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        if (findData.cFileName != "." && findData.cFileName != "..")
                        {
                            if (!DeleteDirectory(path))
                            {
                                isFailed = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (!DeleteFileFromApp(path))
                        {
                            isFailed = true;
                            break;
                        }
                    }
                }
                while (FindNextFile(hFile, out findData));
                FindClose(hFile);
                if (!isFailed)
                {
                    isDeleted = RemoveDirectoryFromApp(itemPath);
                }
            }

            return isDeleted;
        }
    }
}