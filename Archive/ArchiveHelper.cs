using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExtendedStorage.Archive
{
	public static class ArchiveHelper
	{
		/// <summary>
		/// Cleans a name making it conform to Zip file conventions.
		/// </summary>
		/// <param name="name">The name of the entry to clean.</param>
		/// <returns>Returns a valid string.</returns>
		public static string CleanName(string name)
		{
			if (name == null)
			{
				return string.Empty;
			}

			if (Path.IsPathRooted(name))
			{
				// NOTE:
				// for UNC names...  \\machine\share\zoom\beet.txt gives \zoom\beet.txt
				name = name.Substring(Path.GetPathRoot(name).Length);
			}

			name = name.Replace(@"\", "/");

			while ((name.Length > 0) && (name[0] == '/'))
			{
				name = name.Remove(0, 1);
			}
			return name;
		}

		/// <summary>
		/// Sanitize a path.
		/// </summary>
		/// <param name="fileName">The path to sanitize.</param>
		/// <returns>Returns sanitized string (No special characters or unauthorized).</returns>
		public static string SanitizeArchiveName(string fileName)
		{
			// Remove spaces in archive entry
			string result = new string(fileName.Where(c => !char.IsControl(c)).ToArray());

			string pattern = @"\s*/\s*";
			string replacement = "/";
			Regex rgx = new Regex(pattern);
			result = rgx.Replace(result, replacement);

			foreach (string character in new List<string> { ":", "?", "<", ">", "|", "\"", "*" })
			{
				result = result.Replace(Convert.ToChar(character), Convert.ToChar("_"));
			}
			return result.Trim();
		}
	}
}
