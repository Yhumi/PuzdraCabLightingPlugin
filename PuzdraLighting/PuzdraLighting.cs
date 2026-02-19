using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin;
using PuzdraLighting.LightingControllers;
using ECommons;
using ECommons.DalamudServices;
using PuzdraLighting.Helpers;
using ECommons.Throttlers;
using PuzdraLighting.Data;

namespace PuzdraLighting;

public sealed class PuzdraLighting : IDalamudPlugin
{
    public string Name => "PuzdraLighting";

    internal static PuzdraLighting P = null;

    internal WindowSystem ws;
    internal Configuration Config;

    // Lighting Controllers
    internal FastIOLightingHelper LightingHelper;

    // Lighting Controllers
    internal InstanceLighting InstanceLighting;

    public PuzdraLighting(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this, Module.All);

        ConstantData.Init();

        P = this;
        P.Config = Configuration.Load();
        P.Config.Save();

        ws = new();
        Config = P.Config;

        LightingHelper = new FastIOLightingHelper();
        InstanceLighting = new InstanceLighting();
        EzThrottler.Throttle($"Puzdra.DriverSetup", 2000);

        Svc.Log.Debug($"Initialising Cab Lights.");
        LightingHelper.OpenDriver();
        
        Svc.Framework.Update += Tick;
    }

    public void Dispose()
    {
        InstanceLighting.Dispose();

        LightingHelper.WriteRGBColourValues(new FastIOColour(0, 0, 0), new FastIOColour(0, 0, 0), new FastIOColour(0, 0, 0));
        LightingHelper.Dispose();

        GenericHelpers.Safe(() => Svc.Framework.Update -= Tick);

        ws?.RemoveAllWindows();
        ws = null!;

        ECommonsMain.Dispose();
        P = null!;
    }

    public void Tick(object _)
    {
        if (LightingHelper.FastIOState == State.Closed && LightingHelper.OpenAttempts < 5)
        {
            if (!EzThrottler.Throttle($"Puzdra.DriverSetup", 2000)) return;
            LightingHelper.OpenDriver();
        }

        if (LightingHelper.FastIOState == State.Closed)
        {
            if (LightingHelper.OpenAttempts >= 5)
                Svc.Framework.Update -= Tick;

            return;
        }     

        if (InstanceLighting.Enabled)
            InstanceLighting.Tick();
    }
}
