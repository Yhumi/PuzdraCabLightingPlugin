using ECommons.DalamudServices;
using Newtonsoft.Json;
using PuzdraLighting.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PuzdraLighting.Data
{
    internal class ConstantData
    {
        public static Dictionary<ushort, Instance> InstanceData { get; set; } = new Dictionary<ushort, Instance>();

        public static readonly FastIOColour Lights_Off = new FastIOColour(0x00, 0x00, 0x00);

        public static readonly FastIOColour Lights_Healer = new FastIOColour(0x41, 0x71, 0x35);
        public static readonly FastIOColour Lights_Tank = new FastIOColour(0x41, 0x55, 0xAB);
        public static readonly FastIOColour Lights_DPS = new FastIOColour(0x93, 0x43, 0x46);

        public static readonly FastIOColour Lights_Magenta = new FastIOColour(0xFF, 0x0, 0xFF);

        public static void Init()
        {
            try
            {
                var filePath = Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, "Data/Json/InstanceData.json");
                var jsonData = File.ReadAllText(filePath);

                InstanceData = JsonConvert.DeserializeObject<Dictionary<int, Instance>>(jsonData);
            }
            catch (Exception e)
            {
                Svc.Log.Error($"Failed to load config from {Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, "Data/Json/InstanceData.json")}: {e}");
            }
        }

        public static (FastIOColour baseColour, FastIOColour brightColour, FastIOColour dullColour) GetPhaseColours(ushort territoryId, byte weatherId)
        {
            if (!InstanceData.TryGetValue(territoryId, out Instance currentInstance))
                return (Lights_Magenta, Lights_Magenta, Lights_Magenta);

            var phaseData = currentInstance.Phases.FirstOrDefault(x => x.Weather == weatherId);
            if (phaseData == null)
                return (Lights_Magenta, Lights_Magenta, Lights_Magenta);

            return (phaseData.BaseColour, phaseData.BrightColour, phaseData.DullColour);
        }
    }

    internal class Instance
    {
        [JsonProperty("phases")]
        public List<Phase> Phases {  get; set; } = new List<Phase>();
    }

    internal class Phase
    {
        [JsonProperty("weather")]
        public byte Weather { get; set; }

        [JsonProperty("base")]
        public List<string> Base { get; set; } = new List<string>();

        [JsonProperty("bright")]
        public List<string> Bright { get; set; } = new List<string>();

        [JsonProperty("dull")]
        public List<string> Dull { get; set; } = new List<string>();

        public FastIOColour BaseColour { get { return new FastIOColour(Base); } }
        public FastIOColour BrightColour { get { return new FastIOColour(Bright); } }
        public FastIOColour DullColour { get { return new FastIOColour(Dull); } }
    }
}
