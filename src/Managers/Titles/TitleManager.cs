﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lotus.API.Odyssey;
using Lotus.Logging;
using VentLib.Utilities.Extensions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lotus.Managers.Titles;

public class TitleManager
{
    private DirectoryInfo directory;
    private Dictionary<string, List<CustomTitle>> titles = null!;

    private static IDeserializer _titleDeserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    public TitleManager(DirectoryInfo directory)
    {
        if (!directory.Exists) directory.Create();
        this.directory = directory;
        LoadAll();
    }

    public string ApplyTitle(string friendCode, string playerName)
    {
        if (!AmongUsClient.Instance.AmHost) return playerName;
        if (Game.State is not GameState.InLobby) return playerName;
        if (friendCode == "") return playerName;
        return titles.GetOptional(friendCode)
            .Transform(p => p.LastOrDefault()?.ApplyTo(playerName) ?? playerName, () => playerName);
    }

    public CustomTitle? GetTitle(string friendCode)
    {
        if (!AmongUsClient.Instance.AmHost) return null;
        if (Game.State is not GameState.InLobby) return null;
        return friendCode == "" ? null : titles.GetOptional(friendCode).Map(c => c.LastOrDefault()).OrElse(null);
    }

    public void HandlePlayerJoin()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        Game.GetAllPlayers().ForEach(p => p.RpcSetName(p.name));
    }

    public void Reload()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        LoadAll();
        Game.GetAllPlayers().ForEach(p => p.RpcSetName(p.name));
    }

    public void LoadAll()
    {
        // Loading from manifest
        titles = Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .Where(n => n.Contains("Lotus.assets.Titles"))
            .Select(s =>
            {
                string friendcode = s.Replace("Lotus.assets.Titles.", "").Replace(".yaml", "");
                return (friendcode, LoadTitleFromManifest(s));
            })
            .Where(t => t.Item2 != null)
            .ToDict(t => t.friendcode, t => new List<CustomTitle> { t.Item2! });
        
        // Load from titles directory
        directory.GetFiles("*.yaml")
            .Select(f =>
            {
                string friendCode = f.Name.Replace(".yaml", "");
                return (friendCode, LoadFromFileInfo(f));
            })
            .ForEach(pair =>
            {
                titles.GetOrCompute(pair.friendCode, () => new List<CustomTitle>()).Add(pair.Item2);
            });
    }

    private static CustomTitle? LoadTitleFromManifest(string manifestResource)
    {
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResource);
        return stream == null ? null : LoadFromStream(stream);
    }

    private static CustomTitle LoadFromFileInfo(FileInfo file) => LoadFromStream(file.Open(FileMode.Open));

    private static CustomTitle LoadFromStream(Stream stream)
    {
        string result;
        using (StreamReader reader = new(stream))
        {
            result = reader.ReadToEnd();
        }

        return _titleDeserializer.Deserialize<CustomTitle>(result);
    }
}