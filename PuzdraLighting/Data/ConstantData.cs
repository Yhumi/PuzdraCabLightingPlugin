using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using Newtonsoft.Json;
using PuzdraLighting.Animations;
using PuzdraLighting.Helpers;
using PuzdraLighting.LightingControllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.GroupPoseModule;

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

                InstanceData = JsonConvert.DeserializeObject<Dictionary<ushort, Instance>>(jsonData) ?? [];
            }
            catch (Exception e)
            {
                Svc.Log.Error($"Failed to load config from {Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, "Data/Json/InstanceData.json")}: {e}");
            }
        }

        public static (FastIOColour baseColour, FastIOColour brightColour, FastIOColour dullColour) GetPhaseColours(ushort territoryId, byte weatherId)
        {
            if (!InstanceData.TryGetValue(territoryId, out var currentInstance))
                return (Lights_Magenta, Lights_Magenta, Lights_Magenta);

            var phaseData = currentInstance.Phases.FirstOrDefault(x => x.Weather == weatherId);
            if (phaseData == null)
                return (Lights_Magenta, Lights_Magenta, Lights_Magenta);

            return (phaseData.BaseColour, phaseData.BrightColour, phaseData.DullColour);
        }

        public static AnimationBase? GetAnimationBaseForAction(ushort territoryId, uint actionId)
        {
            if (!InstanceData.TryGetValue(territoryId, out var currentInstance))
                return null;

            if (!currentInstance.Actions.TryGetValue(actionId, out var currentAction))
                return null;

            return currentAction.GetAnimation();
        }
    }

    internal class Instance
    {
        [JsonProperty("phases")]
        public List<Phase> Phases {  get; set; } = new List<Phase>();

        [JsonProperty("actions")]
        public Dictionary<uint, AnimationSetup> Actions { get; set; } = new Dictionary<uint, AnimationSetup>();
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

    internal class AnimationSetup
    {
        [JsonProperty("delay")]
        public double DelayMs { get; set; }

        [JsonProperty("type")]
        public AnimationType AnimationType { get; set; }

        [JsonProperty("duration")]
        public double DurationMs { get; set; }

        [JsonProperty("pulsePeak")]
        public double? PulsePeakMs { get; set; }

        [JsonProperty("colour1List")]
        public List<string> Colour1List { get; set; } = new List<string>();

        [JsonProperty("colour2List")]
        public List<string> Colour2List { get; set; } = new List<string>();

        [JsonProperty("front")]
        public bool Front { get; set; }

        [JsonProperty("left")]
        public bool Left { get; set; }

        [JsonProperty("right")]
        public bool Right { get; set; }

        [JsonProperty("runDead")]
        public bool RunWhenDead { get; set; }

        public FastIOColour Colour1 { get { return new FastIOColour(Colour1List); } }
        public FastIOColour Colour2 { get { return new FastIOColour(Colour2List); } }

        public AnimationBase? GetAnimation()
        {
            switch(AnimationType)
            {
                case AnimationType.Pulse:
                    return GetPulseAnimation();
                default:
                    return null;
            }
        }

        private PulseAnimation GetPulseAnimation()
        {
            return new PulseAnimation()
            {
                StartTime = DateTime.Now.AddMilliseconds(DelayMs),
                Duration = DurationMs,
                PeakPointMs = PulsePeakMs ?? 0,

                StartEndColour = Colour1,
                PeakColour = Colour2,

                FrontLighting = Front,
                LeftLighting = Left,
                RightLighting = Right,

                RunWhenDead = RunWhenDead
            };
        }
    }
}
