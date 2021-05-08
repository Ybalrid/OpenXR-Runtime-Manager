using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXR_Runtime_Manager
{
	class Version
	{
		public int major;
		public int minor;
		public int patch;

		public string ShortName => $"{major}.{minor}";

		public override string ToString()
		{
			return $"{major}.{minor}.{patch}";
		}

		public Version(int major = 0, int minor = 0, int patch = 0)
		{
			this.major = major;
			this.minor = minor;
			this.patch = patch;
		}
	}
}
