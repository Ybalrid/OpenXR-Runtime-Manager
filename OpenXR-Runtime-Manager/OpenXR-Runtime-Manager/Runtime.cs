using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXR_Runtime_Manager
{
	class Runtime
	{
		private string name;
		private string manifestPath;
		private string libraryPath;
		private Version version;

		public string Name => name;

		public Runtime(string name, string manifestPath, string libraryPath, Version version)
		{
			this.name = name;
			this.manifestPath = manifestPath;
			this.libraryPath = libraryPath;
			this.version = version;
		}
	}
}
