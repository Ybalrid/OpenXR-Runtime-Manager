﻿using System;
using System.Diagnostics;

namespace OpenXR_Runtime_Manager
{
	class Runtime
	{
		private string name;
		private string manifestPath;
		private string libraryPath;
		private Version version;

		public string Name => name;
		public string ManifestFilePath => manifestPath;
		public string LibraryDLLPath => libraryPath;
		public Version Version => version;

		public Runtime(string name, string manifestPath, string libraryPath, Version version)
		{
			this.name = name;
			this.manifestPath = manifestPath;
			this.libraryPath = libraryPath;
			this.version = version;

			if (Name == null)
				HandleUnnamedRuntime();
		}

		public void DecorateName(string appended)
		{
			name += appended;
		}

		private static string[] skippedWordList = new[]
		{
			"OpenXR",
			"SteamVR"
		};

		/// <summary>Advance the index so we skip words in `skippedWordList`</summary>
		/// <returns>Numbers of characters to skip</returns>
		/// <param name="restOfTheString">A substrings starting at the current index</param>
		private int SkipIgnoredWord(string restOfTheString)
		{
			foreach (string word in skippedWordList)
			{
				if (restOfTheString.StartsWith(word))
					return word.Length;
			}

			return 0;
		}

		private string UncammelCase(string str)
		{
			for (int i = SkipIgnoredWord(str);
				i < str.Length - 1;
				++i)
			{
				if (Char.IsWhiteSpace(str[i]))
				{
					i += SkipIgnoredWord(str.Substring(i + 1));
					continue;
				}

				if (!Char.IsUpper(str[i]) && Char.IsUpper(str[i + 1]))
				{
					str = str.Insert(i + 1, " ");
				}
			}

			return str;
		}

		private void HandleUnnamedRuntime()
		{
      
			Debug.Print($"The runtime manifest did not contain the runtime name. We're extracting a string from the library_path ({libraryPath}) field instead so we can name this one.");
			name = libraryPath;
			name = name.Remove(name.Length - 4, 4);
			if (name.StartsWith("bin"))
				name = name.Remove(0, 4);

			char[] toTrim =
			{
				'.',
				'\\',
				'/'
			};

			name = name.Trim(toTrim);
			name = UncammelCase(name);

            if (libraryPath.Contains("MixedRealityRuntime") && manifestPath.Contains("WINDOWS\\system32"))
            {
                name = "Windows " + name;
            }

		}
	}
}
