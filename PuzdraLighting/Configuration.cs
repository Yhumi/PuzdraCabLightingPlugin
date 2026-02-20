using Dalamud.Configuration;
using ECommons.DalamudServices;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool VerboseLogColourSets { get; set; } = false;

    public void Save()
    {
        Svc.Log.Info($"Saving config.");
        Svc.PluginInterface.SavePluginConfig(this);
    }

    public static Configuration Load()
    {
        try
        {
            var contents = File.ReadAllText(Svc.PluginInterface.ConfigFile.FullName);
            var json = JObject.Parse(contents);
            var version = (int?)json["Version"] ?? 0;
            return json.ToObject<Configuration>() ?? new();
        }
        catch (Exception e)
        {
            Svc.Log.Error($"Failed to load config from {Svc.PluginInterface.ConfigFile.FullName}: {e}");
            return new();
        }
    }
}
