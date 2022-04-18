using System;
using System.IO;
using System.Text;
using static ExtendedStorage.Win32;

namespace ExtendedStorage.Utils
{
    public static class FileWriter
	{
		/// <summary>
		/// Write the text on a EStorageFile.
		/// </summary>
		/// <returns></returns>
		public static bool WriteText(this EStorageFile source, string text, FileShare shareMode = FileShare.None, FileMode fileMode = FileMode.Open)
        {
			IntPtr hFile = CreateFileFromApp(source.Path, (uint)FileAccess.ReadWrite, (uint)shareMode, IntPtr.Zero, (uint)fileMode, (uint)FileAttributes.Normal, IntPtr.Zero);
			if (hFile.ToInt64() != -1)
			{
				SetFilePointer(hFile, 0, IntPtr.Zero, 0);
				SetEndOfFile(hFile);

				uint bytesWritten = 0;
				byte[] buffer = Encoding.UTF8.GetBytes(text);

				if (!WriteFile(hFile, buffer, Convert.ToUInt32(buffer.Length), bytesWritten, IntPtr.Zero))
				{
					return false;
				}
			}
			CloseHandle(hFile);
			return true;
		}
	}
}