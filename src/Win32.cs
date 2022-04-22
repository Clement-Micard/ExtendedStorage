using ExtendedStorage.Enums;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;

namespace ExtendedStorage
{
    internal class Win32
    {
        public enum FINDEX_INFO_LEVELS
        {
            FindExInfoStandard = 0,
            FindExInfoBasic = 1
        }

        public enum FINDEX_SEARCH_OPS
        {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories = 1,
            FindExSearchLimitToDevices = 2
        }

        public enum GET_FILEEX_INFO_LEVELS
        {
            GetFileExInfoStandard,
            GetFileExMaxInfoLevel
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public uint nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        public enum SE_OBJECT_TYPE
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
            SE_REGISTRY_WOW64_32KEY,
            SE_REGISTRY_WOW64_64KEY,
        }

        /// <summary>
        /// Reads a file.
        /// </summary>
        /// <param name="hFile">File handle.</param>
        /// <param name="lpBuffer">Result buffer to put in.</param>
        /// <param name="nNumberOfBytesToRead">Number of bytes to read.</param>
        /// <param name="lpNumberOfBytesRead">Number of read bytes.</param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-l1-1-0.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern bool ReadFile(
            [In] IntPtr hFile,
            [Out] byte[] lpBuffer,
            [In] uint nNumberOfBytesToRead,
            [Out] uint lpNumberOfBytesRead,
            IntPtr lpOverlapped
            );

        [DllImport("api-ms-win-core-file-l1-1-0.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern bool WriteFile(
            [In] IntPtr hFile,
            [Out] byte[] lpBuffer,
            [In] uint nNumberOfBytesToWrite,
            [Out] uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped
            );

        /// <summary>
        /// Gets file size.
        /// </summary>
        /// <param name="hFile">File handle.</param>
        /// <param name="lpFileSize">The file size.</param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-l1-1-0.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern bool GetFileSizeEx(
            IntPtr hFile,
            out long lpFileSize
            );

        /// <summary>
        /// Gets attributes of a file or folder.
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <param name="infoLevel"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern bool GetFileAttributesExFromApp(
        string lpFileName,
        GET_FILEEX_INFO_LEVELS infoLevel,
        out WIN32_FILE_ATTRIBUTE_DATA fileData
        );

        /// <summary>
        /// Get Creation date/Last access date and last write time of a file or a folder.
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="lpCreationTime"></param>
        /// <param name="lpLastAccessTime"></param>
        /// <param name="lpLastWriteTime"></param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-l1-1-0.dll", SetLastError = true)]
        internal static extern bool GetFileTime(
        IntPtr hFile,
        ref FILETIME lpCreationTime,
        ref FILETIME lpLastAccessTime,
        ref FILETIME lpLastWriteTime
        );

        /// <summary>
        /// Closes a file handle.
        /// </summary>
        /// <param name="handle">File Handle</param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-handle-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Copy a file from a destination to another.
        /// </summary>
        /// <param name="lpExistingFileName"></param>
        /// <param name="lpNewFileName"></param>
        /// <param name="bFailIfExists"></param>
        /// <returns></returns>
        // Copy
        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern bool CopyFileFromApp(
        [In] string lpExistingFileName,
        [In] string lpNewFileName,
        [In] bool bFailIfExists
        );

        /// <summary>
        /// Creates a file.
        /// </summary>
        /// <param name="lpFileName">The path to the file</param>
        /// <param name="dwDesiredAccess">FileAccess</param>
        /// <param name="dwShareMode">FileShare</param>
        /// <param name="SecurityAttributes">Security attributes (IntPtr.Zero for default)</param>
        /// <param name="dwCreationDisposition">FileMode</param>
        /// <param name="dwFlagsAndAttributes">Flags and attributes (0 for default)</param>
        /// <param name="hTemplateFile">Template file (IntPtr.Zero for default)</param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
        internal static extern IntPtr CreateFileFromApp(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            [MarshalAs(UnmanagedType.U4)] uint dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] uint dwShareMode,
            IntPtr SecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] uint dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        /// <summary>
        /// Creates a file for write.
        /// </summary>
        /// <param name="filePath">The path of the file to create.</param>
        /// <returns>Returns a SafeFileHandle of the created file.</returns>
        public static SafeFileHandle CreateFileForWrite(string filePath)
        {
            return new SafeFileHandle(CreateFileFromApp(filePath, 0x40000000, 0, IntPtr.Zero, 2, 0x02000000, IntPtr.Zero), true);
        }

        /// <summary>
        /// Open the oath as Stream with specified parameters
        /// </summary>
        /// <param name="source">The source filepath</param>
        /// <param name="accessMode">File access mode</param>
        /// <param name="shareMode">File share mode</param>
        /// <returns>Returns an Stream if success, else null.</returns>
        internal static Stream OpenAsStream(string source, FileAccess accessMode)
        {
            IntPtr hFile = CreateFileFromApp(source, (uint)accessMode, (uint)FileShare.ReadWrite, IntPtr.Zero, (uint)FileMode.Open, 0, IntPtr.Zero);
            if (hFile.ToInt64() != -1)
            {
                return new FileStream(new SafeFileHandle(hFile, true), accessMode);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Create a file in the EStorageFolder
        /// </summary>
        /// <param name="path">path of the file</param>
        /// <returns>Returns the created file as EStorageFile</returns>
        internal static string CreateFile(string path, FileAlreadyExists alreadyExists = FileAlreadyExists.DoNothing)
        {
            string newFilePath = path;
            IntPtr hFile = CreateFileFromApp(path, (uint)FileAccess.Read, (uint)FileShare.ReadWrite, IntPtr.Zero, (uint)FileMode.OpenOrCreate, 0, IntPtr.Zero);
            if (hFile.ToInt64() != -1)
            {
                CloseHandle(hFile);
                return newFilePath;
            }
            else
            {
                if (Marshal.GetLastWin32Error() == 183)
                {
                    if (alreadyExists == FileAlreadyExists.DoNothing)
                    {
                        return newFilePath;
                    }
                    else if (alreadyExists == FileAlreadyExists.GenerateUniqueName)
                    {
                        for (int i = 2; i < int.MaxValue; i++)
                        {
                            if (CreateFileFromApp(path, (uint)FileAccess.Read, (uint)FileShare.ReadWrite, IntPtr.Zero, (uint)FileMode.OpenOrCreate, 0, IntPtr.Zero).ToInt64() != -1)
                            {
                                return $"{path} ({i})";
                            }
                        }
                    }
                    else if (alreadyExists == FileAlreadyExists.ReplaceExisting)
                    {
                        EStorageFile.GetFromPath(newFilePath).Delete();
                        if (CreateFileFromApp(path, (uint)FileAccess.Read, (uint)FileShare.ReadWrite, IntPtr.Zero, (uint)FileMode.OpenOrCreate, 0, IntPtr.Zero).ToInt64() != -1)
                        {
                            return newFilePath;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            return newFilePath;
        }

        /// <summary>
        /// Create an empty folder on the EStorageFolder
        /// </summary>
        /// <param name="path">The path of the folder to create</param>
        /// <param name="alreadyExists">What to do if the folder already exists</param>
        /// <returns>Returns a EStorageFolder if success, else null.</returns>
        internal static string CreateFolder(string path, FileAlreadyExists alreadyExists = FileAlreadyExists.DoNothing)
        {
            string newFolderPath = path;
            if (CreateDirectoryFromApp(newFolderPath, IntPtr.Zero))
            {
                return newFolderPath;
            }
            else
            {
                if (Marshal.GetLastWin32Error() == 183)
                {
                    if (alreadyExists == FileAlreadyExists.DoNothing)
                    {
                        return newFolderPath;
                    }
                    else if (alreadyExists == FileAlreadyExists.GenerateUniqueName)
                    {
                        for (int i = 2; i < int.MaxValue; i++)
                        {
                            if (CreateDirectoryFromApp($@"{path} ({i})", IntPtr.Zero))
                            {
                                return $"{path} ({i})";
                            }
                        }
                    }
                    else if (alreadyExists == FileAlreadyExists.ReplaceExisting)
                    {
                        EStorageFolder.GetFromPath(newFolderPath).Delete();
                        if (CreateDirectoryFromApp(newFolderPath, IntPtr.Zero))
                        {
                            return newFolderPath;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            return newFolderPath;
        }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        /// <param name="lpPathName"></param>
        /// <param name="lpSecurityAttributes"></param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
        internal static extern bool CreateDirectoryFromApp(
            string lpPathName,
            IntPtr lpSecurityAttributes
        );

        /// <summary>
        /// Delete a single file
        /// </summary>
        /// <param name="lpFileName">The file path</param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
        internal static extern bool DeleteFileFromApp(
            string lpFileName
        );

        /// <summary>
        /// Remove an empty directory (Use findData.GetPath().Delete() to delete a folder recursively)
        /// </summary>
        /// <param name="lpPathName">Path of the empty folder</param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
        internal static extern bool RemoveDirectoryFromApp(
            string lpPathName
        );

        /// <summary>
        /// List file/folder (even if hidden) as IntPtr with filters
        /// </summary>
        /// <param name="lpFileName">Filename filter (Use a path to get the first file of a path)</param>
        /// <param name="fInfoLevelId">Info level (FindExInfoStandard or FindExInfoBasic)</param>
        /// <param name="lpFindFileData">out WIN32_FIND_DATA -> Get file data (name, attributes, creation, ...)</param>
        /// <param name="fSearchOp"></param>
        /// <param name="lpSearchFilter"></param>
        /// <param name="dwAdditionalFlags"></param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindFirstFileExFromApp(
            string lpFileName,
            FINDEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FIND_DATA lpFindFileData,
            FINDEX_SEARCH_OPS fSearchOp,
            IntPtr lpSearchFilter,
            int dwAdditionalFlags);

        internal const int FIND_FIRST_EX_CASE_SENSITIVE = 1;
        internal const int FIND_FIRST_EX_LARGE_FETCH = 2;

        /// <summary>
        /// Get the next file
        /// </summary>
        /// <param name="hFindFile">The IntPtr of the first file (Got from FindFirstFileExFromApp)</param>
        /// <param name="lpFindFileData">out WIN32_FIND_DATA -> Get file data (name, attributes, creation, ...)</param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-l1-1-0.dll", CharSet = CharSet.Unicode)]
        internal static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        /// <summary>
        /// Close a file
        /// </summary>
        /// <param name="hFindFile">The file to close (as IntPtr)</param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-l1-1-0.dll")]
        internal static extern bool FindClose(IntPtr hFindFile);

        /// <summary>
        /// Move a file or a folder to another destination
        /// </summary>
        /// <param name="lpExistingFileName">The file/folder to move path (Hidden path allowed))</param>
        /// <param name="lpNewFileName">The new destination path (Hidden path allowed)</param>
        /// <returns></returns>
        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
        internal static extern bool MoveFileFromApp(
            string lpExistingFileName,
            string lpNewFileName
        );

        [DllImport("api-ms-win-security-provider-l1-1-0.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetNamedSecurityInfo(
            string objectName,
            SE_OBJECT_TYPE objectType,
            SecurityInfos securityInfo,
            out IntPtr sidOwner,
            out IntPtr sidGroup,
            out IntPtr dacl,
            out IntPtr sacl,
            out IntPtr securityDescriptor);

        [DllImport("api-ms-win-security-provider-l1-1-0.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetSecurityInfo(
            IntPtr handle,
            SE_OBJECT_TYPE objectType,
            SecurityInfos securityInfo,
            out IntPtr sidOwner,
            out IntPtr sidGroup,
            out IntPtr dacl,
            out IntPtr sacl,
            out IntPtr securityDescriptor);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="ObjectType">A value of the SE_OBJECT_TYPE enum that indicates the type of object named by pObjectName</param>
        /// <param name="SecurityInfo">A value of the SECURITY_INFORMATION enum utilising bit flags that indicate the type of security information to set.</param>
        /// <param name="psidOwner">A pointer to a SID structure that identifies the owner of the object. May be NUll if unset</param>
        /// <param name="psidGroup">A pointer to a SID that identifies the primary group of the object. May be NULL if unset</param>
        /// <param name="pDacl">A pointer to the new DACL for the object. </param>
        /// <param name="pSacl">A pointer to the new SACL for the object. May be NULL if unset</param>
        /// <returns></returns>
        [DllImport("api-ms-win-security-provider-l1-1-0.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SetNamedSecurityInfo(
            string objectName,
            SE_OBJECT_TYPE objectType,
            SecurityInfos securityInfo,
            IntPtr sidOwner,
            IntPtr sidGroup,
            IntPtr dacl,
            IntPtr sacl);

        [DllImport("api-ms-win-security-provider-l1-1-0.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SetSecurityInfo(
            IntPtr handle,
            SE_OBJECT_TYPE objectType,
            SecurityInfos securityInfo,
            IntPtr sidOwner,
            IntPtr sidGroup,
            IntPtr dacl,
            IntPtr sacl);

        [DllImport("api-ms-win-core-heap-l2-1-0.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        /// <summary>
        /// Copies the security attributes from a source file object to a recipient file object
        /// </summary>
        /// <param name="Source">Filepath to source object</param>
        /// <returns>True if successful, false if not</returns>
        internal static bool CopyFilePermission(string source)
        {
            //Use the method to let the file inherit the security attributes from parent object
            uint BackupSemantics = 0x02000000;
            uint OPEN_EXISTING = 3;
            uint GENERIC_READ = 0x80000000;
            IntPtr sidOwnerDescriptor = IntPtr.Zero;
            IntPtr sidGroupDescriptor = IntPtr.Zero;
            IntPtr daclDescriptor = IntPtr.Zero;
            bool err = true;
            try
            {
                int result;
                IntPtr hfolder = CreateFileFromApp(source, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, BackupSemantics, IntPtr.Zero);
                IntPtr sidOwner;
                IntPtr sidGroup;
                IntPtr dacl;
                IntPtr sacl;
                if (hfolder.ToInt64() != -1)
                {
                    result = GetSecurityInfo(hfolder, SE_OBJECT_TYPE.SE_FILE_OBJECT, SecurityInfos.DiscretionaryAcl, out sidOwner, out sidGroup, out dacl, out sacl, out daclDescriptor);
                    if (result != 0)
                    {
                        err = false;
                    }
                    result = GetSecurityInfo(hfolder, SE_OBJECT_TYPE.SE_FILE_OBJECT, SecurityInfos.Owner, out sidOwner, out sidGroup, out dacl, out sacl, out sidGroupDescriptor);
                    if (result != 0)
                    {
                        err = false;
                    }
                    result = GetSecurityInfo(hfolder, SE_OBJECT_TYPE.SE_FILE_OBJECT, SecurityInfos.Group, out sidOwner, out sidGroup, out dacl, out sacl, out sidGroupDescriptor);
                    if (result != 0)
                    {
                        err = false;
                    }
                    CloseHandle(hfolder);
                }
                else
                {
                    result = GetNamedSecurityInfo(source, SE_OBJECT_TYPE.SE_FILE_OBJECT, SecurityInfos.DiscretionaryAcl, out sidOwner, out sidGroup, out dacl, out sacl, out daclDescriptor);
                    {
                        err = false;
                    }
                    result = GetNamedSecurityInfo(source, SE_OBJECT_TYPE.SE_FILE_OBJECT, SecurityInfos.Owner, out sidOwner, out sidGroup, out dacl, out sacl, out sidGroupDescriptor);
                    {
                        err = false;
                    }
                    result = GetNamedSecurityInfo(source, SE_OBJECT_TYPE.SE_FILE_OBJECT, SecurityInfos.Group, out sidOwner, out sidGroup, out dacl, out sacl, out sidGroupDescriptor);
                    {
                        err = false;
                    }
                }
                SecurityInfos info = SecurityInfos.DiscretionaryAcl | SecurityInfos.Group | SecurityInfos.Owner;
                IntPtr hfile = CreateFileFromApp(source, 3, (uint)FileShare.ReadWrite, IntPtr.Zero, (uint)FileMode.Open, 0, IntPtr.Zero);
                if (hfile.ToInt64() != -1)
                {
                    result = SetSecurityInfo(hfile, SE_OBJECT_TYPE.SE_FILE_OBJECT, info, sidOwner, sidGroup, dacl, sacl);
                    CloseHandle(hfile);
                    if (result != 0)
                    {
                        err = false;
                    }
                }
                else
                {
                    if (result != 0)
                    {
                        err = false;
                    }
                }
            }
            finally
            {
                if (sidOwnerDescriptor != IntPtr.Zero && LocalFree(sidOwnerDescriptor) != IntPtr.Zero)
                {
                    err = false;
                }
                if (sidGroupDescriptor != IntPtr.Zero && LocalFree(sidGroupDescriptor) != IntPtr.Zero)
                {
                    err = false;
                }
                if (daclDescriptor != IntPtr.Zero && LocalFree(daclDescriptor) != IntPtr.Zero)
                {
                    err = false;
                }
            }
            return err;
        }

        [DllImport("api-ms-win-core-file-l1-1-0.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint SetFilePointer(IntPtr hFile, long lDistanceToMove, IntPtr lpdistanceToMoveHigh, uint dwMoveMethod);

        [DllImport("api-ms-win-core-file-l1-1-0.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetEndOfFile(IntPtr hFile);

        //import function to get drives
        [DllImport("api-ms-win-core-file-l1-1-0.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint GetLogicalDrives();
    }
}