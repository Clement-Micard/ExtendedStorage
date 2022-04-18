using System;

namespace ExtendedStorage.Helpers
{
	public static class DiskHelpers
	{
		public static bool IsRootDisk(string path)
		{
			int length = path.Length;
			if ((length <= 3 && (path[1] == Convert.ToChar(":"))))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static long CalculateSize(uint fileSizeLow, uint fileSizeHigh)
		{
			const long MAXDWORD = 4294967295;
			return (fileSizeHigh * (MAXDWORD + 1)) + fileSizeLow;
		}

		public static DateTimeOffset GetCreationDate(long highDateTime, int lowDateTime)
		{
			long dateTime = (highDateTime << 32) + lowDateTime;
			return DateTimeOffset.FromFileTime(dateTime);
		}
	}
}
