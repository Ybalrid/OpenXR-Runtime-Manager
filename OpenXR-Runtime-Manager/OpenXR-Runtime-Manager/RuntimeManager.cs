using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace OpenXR_Runtime_Manager
{
	class RuntimeManager
	{
		private readonly List<Runtime> _availableRuntimes = new List<Runtime>();
		private Runtime _activeRuntime = null;
		
		public bool HasActiveRuntime { get; } = false;
		public int RuntimeCount => _availableRuntimes.Count;

		public Runtime ActiveRuntime => _activeRuntime;

		private struct RuntimeInfo
		{
			[JsonProperty("library_path")]
			public string LibraryPath;
			[JsonProperty("name")]
			public string Name;
			[JsonProperty("api_version")]
			public string ApiVersion;
			[JsonProperty("VALVE_runtime_is_steamvr")]
			public bool VALVE_runtime_is_steamvr;
		}

		private struct RuntimeManifest
		{
			[JsonProperty("file_format_version")] public string versionTag;
			[JsonProperty("runtime")] public RuntimeInfo runtime;
		}

		private bool GetActiveRuntimeFromRegistry()
		{
			const string KhronosOpenXRPath = @"SOFTWARE\Khronos\OpenXR\1";
			RegistryKey OpenXRV1Key = Registry.LocalMachine.OpenSubKey(KhronosOpenXRPath);
			var activeRuntimeManifestPath = (string) OpenXRV1Key?.GetValue("ActiveRuntime");

			if (string.IsNullOrEmpty(activeRuntimeManifestPath)) return false;
			using (StreamReader r = new StreamReader(activeRuntimeManifestPath))
			{
				var json = r.ReadToEnd();
				var manifest = JsonConvert.DeserializeObject<RuntimeManifest>(json);

				_activeRuntime = new Runtime(manifest.runtime.Name, activeRuntimeManifestPath,
					manifest.runtime.LibraryPath, new Version(1));

				_availableRuntimes.Add(_activeRuntime);
				return true;
			}
		}

		public RuntimeManager()
		{
			HasActiveRuntime = GetActiveRuntimeFromRegistry();
		}
	}
}
