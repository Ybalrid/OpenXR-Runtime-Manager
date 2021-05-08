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
		}
	}
}
