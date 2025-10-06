using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

using System.Diagnostics;

namespace EVTUI;

public static class UserCache
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private static string DefaultYaml = @"
Projects: []
Games: []
Preferences: {}";
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

    public static string                  UserLogPath   = Path.Combine(LocalDir, "UserLog.txt");
    public static TextWriter              UserLogWriter = File.CreateText(UserLogPath);
    public static TextWriterTraceListener Logger        = new TextWriterTraceListener(UserLogWriter);

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public static User InitializeOrLoadUser()
    {
        Console.SetOut(UserLogWriter);
        Console.SetError(UserLogWriter);

        Trace.Listeners.Add(Logger);
        Trace.AutoFlush = true;

        TextReader yamlStream;
        if (File.Exists(UserCacheFile))
            // TODO: handle a failed read without just overwriting, as the below code inelegantly does
            // e.g., just return null and let App.axaml.cs handle it and show a popup....
            try
            {
                yamlStream = new StreamReader(UserCacheFile);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                yamlStream = new StringReader(DefaultYaml);
            }
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
    public List<Project>              Projects    { get; set; }
    public List<GameSettings>         Games       { get; set; }
    public Dictionary<string, object> Preferences { get; set; }
}

public class Project
{
    public string                   Name       { get; set; }
    public string                   Notes      { get; set; }
    public string                   Game       { get; set; }
    public ModSettings              Mod        { get; set; }
    public Dictionary<string, bool> Frameworks { get; set; }
    public List<string>             LoadOrder  { get; set; }
    public EventCollections         Events     { get; set; }
}

public class GameSettings
{
    public string           Path   { get; set; }
    public string           Type   { get; set; }
    public string           Notes  { get; set; }
    public EventCollections Events { get; set; }
}

public class ModSettings
{
    public string Path { get; set; }
    public string Type { get; set; }
}

public class EventCollections
{
    public List<SimpleEvent>   Pinned { get; set; }
    public List<SimpleEvent>   Recent { get; set; }
    public List<ExpandedEvent> Notes  { get; set; }

    public EventCollections()
    {
        this.Pinned = new List<SimpleEvent>();
        this.Recent = new List<SimpleEvent>();
        this.Notes  = new List<ExpandedEvent>();
    }
}

public class SimpleEvent
{
    public int    MajorId { get; set; }
    public int    MinorId { get; set; }
}

public class ExpandedEvent
{
    public int    MajorId { get; set; }
    public int    MinorId { get; set; }
    public string Text    { get; set; }
}
