using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace OpenXR_Runtime_Manager
{
    internal class InfoToolReport
    {
        public struct Extension
        {
            public string name;
            public int version;
        }

        public struct Layer
        {
            public string description;
            public string name;
            public int version;
        }

        public struct OpenXR
        {
            public string version;
            public Extension[] extension_list;
            public Layer[] layer_list;
        }

        public struct Report
        {
            public OpenXR openxr;
        }

        public static OpenXR GetInfo(string report)
        {
            Report output = JsonConvert.DeserializeObject<Report>(report);
            return output.openxr;
        }
    }
}
