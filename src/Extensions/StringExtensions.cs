using System;

namespace ExtendedStorage.Extensions
{
	internal static class StringExtensions
	{
		internal static string FormatPath(this string path)
		{
			string finalPath = path;
			finalPath.TrimEnd(Convert.ToChar(@"\"));
			finalPath = finalPath.Replace(@"/", @"\");
			finalPath = finalPath.Replace(@"\\", @"\");
			return finalPath;
		}
	}
}