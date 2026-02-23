using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using PuzdraLighting.Animations;
using PuzdraLighting.LightingControllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PuzdraLighting.UI
{
    internal class AnimationTestUI : Window
    {
        private bool visible = false;

        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        public AnimationTestUI() : base($"{P.Name} {P.GetType().Assembly.GetName().Version}###PuzdraLighting")
        {
            this.RespectCloseHotkey = false;
            this.SizeConstraints = new()
            {
                MinimumSize = new(250, 100),
                MaximumSize = new(9999, 9999)
            };
            P.ws.AddWindow(this);
        }

        public void Dispose()
        {
        }

        private double PulsePeak;
        private double Duration;

        private byte BaseR;
        private byte BaseG;
        private byte BaseB;

        private byte ToR;
        private byte ToG;
        private byte ToB;

        public override void Draw()
        {
            var refSet = P.Config.VerboseLogColourSets;
            var logUnnamed = P.Config.LogUnnamedActions;

            if (ImGui.Checkbox($"Verbose Log Colour Values", ref refSet)) 
            {
                P.Config.VerboseLogColourSets = refSet;
                P.Config.Save();
            }

            ImGui.SameLine();
            if (ImGui.Checkbox($"Log Unnammed Action", ref logUnnamed))
            {
                P.Config.LogUnnamedActions = logUnnamed;
                P.Config.Save();
            }

            ImGui.TextWrapped($"Test Animations");

            ImGui.Separator();

            ImGui.TextWrapped($"Pulse Peak");
            ImGui.InputDouble("###PulsePeak", ref PulsePeak);

            ImGui.TextWrapped($"Duration");
            ImGui.InputDouble("###Duration", ref Duration);

            ImGui.TextWrapped($"Base R");
            ImGui.InputByte("###BaseR", ref BaseR);
            ImGui.TextWrapped($"Base G");
            ImGui.InputByte("###BaseG", ref BaseG);
            ImGui.TextWrapped($"Base B");
            ImGui.InputByte("###BaseB", ref BaseB);

            ImGui.TextWrapped($"To R");
            ImGui.InputByte("###ToR", ref ToR);
            ImGui.TextWrapped($"To G");
            ImGui.InputByte("###ToG", ref ToG);
            ImGui.TextWrapped($"To B");
            ImGui.InputByte("###ToB", ref ToB);

            if (ImGui.Button($"Override Base###OverrideBase"))
            {
                P.InstanceLighting.Base = new Helpers.FastIOColour(BaseR, BaseG, BaseB);
            }

            if (ImGui.Button("Pulse###PulseAnim"))
            {
                var newPulseAnim = new PulseAnimation()
                {
                    Duration = Duration,
                    StartTime = DateTime.Now,

                    PeakPointMs = PulsePeak,
                    StartEndColour = new Helpers.FastIOColour(BaseR, BaseG, BaseB),
                    PeakColour = new Helpers.FastIOColour(ToR, ToG, ToB),

                    FrontLighting = true,
                    LeftLighting = true,
                    RightLighting = true
                };

                Svc.Log.Debug($"Adding Pulse Animation. Duration: {Duration}, PeakPointMS: {PulsePeak}, StartEndColour: #{BaseR:X2}{BaseG:X2}{BaseB:X2}, PeakColour: {ToR:X2}{ToG:X2}{ToB:X2}");

                P.InstanceLighting.animationStack.Add($"{nameof(InstanceLightingEventType.Test)}-{DateTime.Now.ToString("HH:mm:ss.fffffff")}", newPulseAnim);
            }

            if (ImGui.Button($"Reset Override###Reset Override"))
            {
                P.InstanceLighting.CalculateWeather();
                P.InstanceLighting.OnWeatherChange();
            }

            ImGui.TextWrapped($"Animation Queue Items: {P.InstanceLighting.animationStack.Count}");
        }
    }
}
