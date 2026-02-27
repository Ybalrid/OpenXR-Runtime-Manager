using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Text.Json;
using System.Text.Json.Serialization;
using Valve.VR;

namespace OpenXR_Runtime_Manager
{
    class RuntimeManager
    {
        //TODO make a database of paths to manifest for known OpenXR runtimes that are compatible with MS Windows
        List<string> WellKnwonOpenXRRuntimeManifestPaths = new List<string>()
        {
            "%windir%\\system32\\MixedRealityRuntime.json",
            "%ProgramFiles%\\Varjo\\varjo-openxr\\VarjoOpenXR.json",
            "%ProgramFiles(x86)%\\VIVE\\Updater\\App\\ViveVRRuntime\\ViveVR_openxr\\ViveOpenXR.json"
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
            [JsonPropertyName("library_path")]
            public string LibraryPath { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("api_version")]
            public string ApiVersion { get; set; }
            [JsonPropertyName("VALVE_runtime_is_steamvr")]
            public bool ValveRuntimeIsSteamvr { get; set; }
        }

        private struct RuntimeManifest
        {
            [JsonPropertyName("file_format_version")] public string VersionTag { get; set; }
            [JsonPropertyName("runtime")] public RuntimeInfo Runtime { get; set; }
        }

        private Runtime ReadManifest(string runtimeManifestPath)
        {
            try
            {
                using (StreamReader r = new StreamReader(Environment.ExpandEnvironmentVariables(runtimeManifestPath)))
                {
                    var json = r.ReadToEnd();
                    var manifest = JsonSerializer.Deserialize<RuntimeManifest>(json);
                    if (manifest.Runtime.LibraryPath == null)
                        throw new InvalidDataException("Runtime manifest did not include runtime.library_path");

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

        private bool GetAvaialbleRuntimesFromRegistry()
        {
            Debug.Print("Looking for AvailableRuntimes in the registry");
            bool hasAppended = false;
            RegistryKey openXRv1Key = Registry.LocalMachine.OpenSubKey(GetKhronosOpenXRVersionRegistryKeyPath());
            RegistryKey availableRuntimesKey = openXRv1Key?.OpenSubKey("AvailableRuntimes");

            if (availableRuntimesKey != null)
            {
                var availableRuntimes = availableRuntimesKey.GetValueNames();
                foreach (string runtimeManifestPath in availableRuntimes)
                {
                    Debug.Print($"Manifest path in registry : {runtimeManifestPath}");
                    if (availableRuntimesKey.GetValue(runtimeManifestPath).Equals(0))
                    {
                        var availableRuntimeManifest = ReadManifest(runtimeManifestPath);
                        Debug.Print($"Read manifest for {availableRuntimeManifest.Name}");
                        if (availableRuntimes != null)
                        {
                            _availableRuntimes[availableRuntimeManifest.Name] = availableRuntimeManifest;
                            hasAppended = true;
                        }
                    }
                }
            }

            return hasAppended;
        }

        private bool GetActiveRuntimeFromRegistry()
        {
            RegistryKey OpenXRV1Key = Registry.LocalMachine.OpenSubKey(GetKhronosOpenXRVersionRegistryKeyPath());
            var activeRuntimeManifestPath = (string)OpenXRV1Key?.GetValue("ActiveRuntime");

            if (string.IsNullOrEmpty(activeRuntimeManifestPath)) return false;

            _activeRuntime = ReadManifest(activeRuntimeManifestPath);
            if (_activeRuntime != null)
            {
                _availableRuntimes[_activeRuntime.Name] = _activeRuntime;
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
                if (_activeRuntime != null)
                    activeRuntimeManifestPath = _activeRuntime.ManifestFilePath;

                if (probedRuntime != null && Environment.ExpandEnvironmentVariables(probedRuntime.ManifestFilePath) !=
                    Environment.ExpandEnvironmentVariables(activeRuntimeManifestPath))
                {
                    string name = probedRuntime.Name;
                    _availableRuntimes[name] = probedRuntime;
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
            uint bufferSize = 256;
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
            catch (DllNotFoundException e)
            {
                Debug.Print("Missing openvr_api.dll?!");
                Debug.Print(e.Message);
                return false;
            }
        }

        public RuntimeManager()
        {
            if (!GetActiveRuntimeFromRegistry())
                Debug.WriteLine("Failed to get current runtime");
            if (!GetAvaialbleRuntimesFromRegistry())
                Debug.WriteLine("Failed to get available runtimes from registry");

            if (!ProbeForSteamVRInstallationPath())
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

        public Runtime GetRuntimeInfo(string name)
        {
            if (_availableRuntimes.TryGetValue(name, out Runtime value))
                return value;

            throw new InvalidOperationException("That runtime does not exist");
        }
    }
}
