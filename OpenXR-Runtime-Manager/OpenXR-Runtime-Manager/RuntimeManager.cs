﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json;
using Valve.VR;

namespace OpenXR_Runtime_Manager
{
	class RuntimeManager
	{
		//TODO make a database of paths to manifest for known OpenXR runtimes that are compatible with MS Windows
		List<string> WellKnwonOpenXRRuntimeManifestPaths = new List<string>()
		{
			"%ProgramFiles%\\Oculus\\Support\\oculus-runtime\\oculus_openxr_32.json",
			"%ProgramFiles%\\Oculus\\Support\\oculus-runtime\\oculus_openxr_64.json",
			"%windir%\\system32\\MixedRealityRuntime.json",
			"%ProgramFiles%\\Varjo\\varjo-openxr\\VarjoOpenXR.json"
		};

		private readonly Dictionary<string, Runtime> _availableRuntimes = new Dictionary<string, Runtime>();
		private Runtime _activeRuntime = null;

		public bool HasActiveRuntime => _activeRuntime != null;
		public Runtime ActiveRuntime => _activeRuntime;

		public List<string> AvailableRuntimeNames
		{
			get
			{
				List<string> output = new List<string>();
				foreach (var knownRuntime in _availableRuntimes)
				{
					output.Add(knownRuntime.Key);
				}
				return output;
			}
		}

		private struct RuntimeInfo
		{
			[JsonProperty("library_path")]
			public string LibraryPath;
			[JsonProperty("name")]
			public string Name;
			[JsonProperty("api_version")]
			public string ApiVersion;
			[JsonProperty("VALVE_runtime_is_steamvr")]
			public bool ValveRuntimeIsSteamvr;
		}

		private struct RuntimeManifest
		{
			[JsonProperty("file_format_version")] public string VersionTag;
			[JsonProperty("runtime")] public RuntimeInfo Runtime;
		}

		private Runtime ReadManifest(string runtimeManifestPath)
		{
			try
			{
				using (StreamReader r = new StreamReader(Environment.ExpandEnvironmentVariables(runtimeManifestPath)))
				{
					var json = r.ReadToEnd();
					var manifest = JsonConvert.DeserializeObject<RuntimeManifest>(json);

					return new Runtime(manifest.Runtime.Name, runtimeManifestPath,
						manifest.Runtime.LibraryPath, new Version(1));
				}
			}
			catch (Exception e)
			{
				Debug.Print(e.Message);
				return null;
			}
		}

		private void Handle32bit(Runtime runtime)
		{
			var manifestFilePath = runtime.ManifestFilePath;
			if (manifestFilePath.Contains("oculus") && manifestFilePath.Contains("32"))
				runtime.DecorateName(" (32 bits)");
		}

		private bool GetActiveRuntimeFromRegistry()
		{
			RegistryKey OpenXRV1Key = Registry.LocalMachine.OpenSubKey(GetKhronosOpenXRVersionRegistryKeyPath());
			var activeRuntimeManifestPath = (string)OpenXRV1Key?.GetValue("ActiveRuntime");

			if (string.IsNullOrEmpty(activeRuntimeManifestPath)) return false;

			_activeRuntime = ReadManifest(activeRuntimeManifestPath);
			if (_activeRuntime != null)
			{
				Handle32bit(_activeRuntime);
				_availableRuntimes.Add(_activeRuntime.Name, _activeRuntime);
				return true;
			}

			return false;
		}

		private void ProbeForAdditionalRuntimes()
		{
			foreach (string manifestFilePath in WellKnwonOpenXRRuntimeManifestPaths)
			{
				var probedRuntime = ReadManifest(manifestFilePath);
				var activeRuntimeManifestPath = "";
				if(_activeRuntime != null)
					activeRuntimeManifestPath = _activeRuntime.ManifestFilePath;

				if(probedRuntime != null && Environment.ExpandEnvironmentVariables(probedRuntime.ManifestFilePath) !=
					Environment.ExpandEnvironmentVariables(activeRuntimeManifestPath))
				{
					string name = probedRuntime.Name;
					if (manifestFilePath.Contains("oculus") && manifestFilePath.Contains("32"))
						name += " (32 bits)";

					_availableRuntimes.Add(name, probedRuntime);
				}
			}
		}

		public bool SetRuntimeAsSystem(string name)
		{
			if (_availableRuntimes.TryGetValue(name, out var runtime))
			{
				RegistryKey OpenXRV1Key = Registry.LocalMachine.CreateSubKey(GetKhronosOpenXRVersionRegistryKeyPath(), true);

				try
				{
					OpenXRV1Key.SetValue("ActiveRuntime", Environment.ExpandEnvironmentVariables(runtime.ManifestFilePath));
					_activeRuntime = runtime;
					return true;
				}
				catch (Exception e)
				{
					Debug.Print(e.Message);
					return false;
				}
			}
			return false;
		}

		private static string GetKhronosOpenXRVersionRegistryKeyPath(int OpenXRVersion = 1)
		{
			return $@"SOFTWARE\Khronos\OpenXR\{OpenXRVersion}";
		}

		/// <summary>
		/// Uses OpenVR to query the installation path of SteamVR, to then build the path to the OpenXR manifest file
		/// </summary>
		/// <returns>True upon success. </returns>
		private bool ProbeForSteamVRInstallationPath()
		{
			StringBuilder pathBuilder = new StringBuilder(256);
			uint bufferSize = (uint)pathBuilder.Length;
			try
			{
				if (OpenVRInterop.GetRuntimePath(pathBuilder, bufferSize, ref bufferSize))
				{
					string pathToSteamVR = pathBuilder.ToString();
					Debug.Print($"Found a SteamVR installation at {pathToSteamVR}");
					string pathToSteamVRManifest = Path.Combine(pathBuilder.ToString(), "steamxr_win64.json");
					WellKnwonOpenXRRuntimeManifestPaths.Add(pathToSteamVRManifest);
					return true;
				}
				return false;
			}
			catch(DllNotFoundException e)
			{
				Debug.Print("Missing openvr_api.dll?!");
				Debug.Print(e.Message);
				return false;
			}
		}

		public RuntimeManager()
		{
			GetActiveRuntimeFromRegistry();

			if(!ProbeForSteamVRInstallationPath())
			{
				Debug.Print("This system seems to not have SteamVR installed, or we cannot call OpenVR for some other reasons. Will probe in default installation folder instead");
				WellKnwonOpenXRRuntimeManifestPaths.Add("%ProgramFiles(x86)%\\Steam\\steamapps\\common\\SteamVR\\steamxr_win64.json");
			}

			ProbeForAdditionalRuntimes();

			foreach (KeyValuePair<string, Runtime> availableRuntime in _availableRuntimes)
			{
				Debug.Print($"Found runtime {availableRuntime.Key}");
			}
		}
	}
}
