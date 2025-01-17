﻿using ECommons.DalamudServices;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace ECommons.Configuration;

public static class EzConfig
{
    public static IEzConfig Config { get; private set; }

    public static T Init<T>() where T : IEzConfig, new()
    {
        Config = LoadConfiguration<T>("DefaultConfig.json");
        return (T)Config;
    }

    public static void Save()
    {
        if (Config != null)
        {
            SaveConfiguration(Config, "DefaultConfig.json", true);
        }
    }

    public static void SaveConfiguration(this IEzConfig Configuration, string path, bool indented = false, bool appendConfigDirectory = true)
    {
        if (appendConfigDirectory) path = Path.Combine(Svc.PluginInterface.GetPluginConfigDirectory(), path);
        File.WriteAllText(path, JsonConvert.SerializeObject(Configuration, new JsonSerializerSettings()
        {
            Formatting = indented ? Formatting.Indented : Formatting.None,
            DefaultValueHandling = Configuration.GetType().IsDefined(typeof(IgnoreDefaultValueAttribute), false) ?DefaultValueHandling.Ignore:DefaultValueHandling.Include
        }), Encoding.UTF8) ;
    }

    public static T LoadConfiguration<T>(string path, bool appendConfigDirectory = true) where T : IEzConfig, new()
    {
        if (appendConfigDirectory) path = Path.Combine(Svc.PluginInterface.GetPluginConfigDirectory(), path);
        if (!File.Exists(path))
        {
            return new T();
        }
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(path, Encoding.UTF8), new JsonSerializerSettings()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        }) ?? new T();
    }
}
