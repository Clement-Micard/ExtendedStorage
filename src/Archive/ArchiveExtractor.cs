using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ExtendedStorage.Win32;

namespace ExtendedStorage.Archive
{
    public class ArchiveExtractor
	{
        public static void ExtractArchive(EStorageFile archive, EStorageFolder destinationFolder)
        {
            using (ZipFile zipFile = new ZipFile(archive.OpenAsStream(FileAccess.Read)))
            {
                zipFile.IsStreamOwner = true;
                List<ZipEntry> directoryEntries = new List<ZipEntry>();
                List<ZipEntry> fileEntries = new List<ZipEntry>();
                foreach (ZipEntry entry in zipFile)
                {
                    if (entry.IsFile)
                    {
                        fileEntries.Add(entry);
                    }
                    else
                    {
                        directoryEntries.Add(entry);
                    }
                }

                WindowsNameTransform wnt = new WindowsNameTransform(destinationFolder.Path);

                List<string> directories = new List<string>();
                directories.AddRange(directoryEntries.Select((item) => wnt.TransformDirectory(item.Name)));
                directories.AddRange(fileEntries.Select((item) => Path.GetDirectoryName(wnt.TransformFile(item.Name))));
                foreach (string dir in directories.Distinct().OrderBy(x => x.Length))
                {
                    if (!CreateDirectoryFromApp(dir, IntPtr.Zero))
                    {
                        string dirName = destinationFolder.Path;
                        foreach (string component in dir.Substring(destinationFolder.Path.Length).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
                        {
                            dirName = Path.Combine(dirName, component);
                            CreateDirectoryFromApp(dirName, IntPtr.Zero);
                        }
                    }
                }

                // Fill files
                foreach (ZipEntry entry in fileEntries)
                {
                    string filePath = wnt.TransformFile(entry.Name);

                    SafeFileHandle hFile = CreateFileForWrite(filePath);
                    if (hFile.IsInvalid)
                    {
                        continue; // Skip the current entry [Workaround, might cause issues]
                    }

                    using (FileStream destinationStream = new FileStream(hFile, FileAccess.Write))
                    {
                        using (Stream entryStream = zipFile.GetInputStream(entry))
                        {
                            entryStream.CopyTo(destinationStream);
                        }
                    }
                }
            }
        }
    }
}