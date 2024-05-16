using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace EVTUI;

public static class UserCache
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private static string DefaultYaml = @"
Projects: []
ReadOnly:
  History:
    CPKs: []
    Events: []";
    private static ISerializer Serializer = new SerializerBuilder()
                               .WithIndentedSequences()
                               .EnsureRoundtrip()
                               .Build();
    private static IDeserializer Deserializer = new DeserializerBuilder().Build();

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public static string LocalDir      = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EVTUI");
    public static string UserCacheFile = Path.Combine(LocalDir, "UserCache.yaml");

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public static User InitializeOrLoadUser()
    {
        if (!Directory.Exists(LocalDir))
            Directory.CreateDirectory(LocalDir);

        TextReader yamlStream;
        if (File.Exists(UserCacheFile))
            yamlStream = new StreamReader(UserCacheFile);
        else
            yamlStream = new StringReader(DefaultYaml);
        return Deserialize(yamlStream.ReadToEnd());
    }

    public static User Deserialize(string yaml)
    {
        return Deserializer.Deserialize<User>(yaml);
    }

    public static void SaveToYaml(User user)
    {
        using (TextWriter writer = File.CreateText(UserCacheFile))
            writer.Write(Serialize(user));
    }

    public static string Serialize(User user)
    {
        return Serializer.Serialize(user);
    }

}

public class User
{
    public List<Project>    Projects   { get; set; }
    public ReadOnlySettings ReadOnly { get; set; }
}

public class Project
{
    public ImmutableSettings Immutable { get; set; }
    public MutableSettings   Mutable   { get; set; }
    public ProjectHistory    History   { get; set; }
}

public class ImmutableSettings
{
    public GameSettings Game { get; set; }
    public ModSettings Mod   { get; set; }
}

public class GameSettings
{
    public string Path { get; set; }
    public string Type { get; set; }
}

public class ModSettings
{
    public string Path { get; set; }
    public string Type { get; set; }
}

public class MutableSettings
{
    public string                   Name       { get; set; }
    public Dictionary<string, bool> Frameworks { get; set; }
    public List<string>             LoadOrder  { get; set; }
}

public class ProjectHistory
{
    public List<Event> Events { get; set; }
}

public class ReadOnlySettings
{
    public ReadOnlyHistory History { get; set; }
}

public class ReadOnlyHistory
{
    public List<string> CPKs   { get; set; }
    public List<Event>  Events { get; set; }
}

public class Event
{
    public int MajorId { get; set; }
    public int MinorId { get; set; }
}
