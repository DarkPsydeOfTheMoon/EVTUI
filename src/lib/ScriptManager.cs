using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ImageMagick;

using AtlusScriptLibrary.Common.Text.Encodings;

using AtlusScriptLibrary.Common.Libraries;
using AtlusScriptLibrary.Common.Logging;
using AtlusScriptLibrary.Common.Text;

using AtlusScriptLibrary.MessageScriptLanguage;
using AtlusScriptLibrary.MessageScriptLanguage.BinaryModel;
using AtlusScriptLibrary.MessageScriptLanguage.Decompiler;
using AtlusScriptLibrary.MessageScriptLanguage.Compiler;

using AtlusScriptLibrary.FlowScriptLanguage;
using AtlusScriptLibrary.FlowScriptLanguage.BinaryModel;
using AtlusScriptLibrary.FlowScriptLanguage.Decompiler;
using AtlusScriptLibrary.FlowScriptLanguage.Compiler;

using MsgFormatVersion = AtlusScriptLibrary.MessageScriptLanguage.FormatVersion;
using FlowFormatVersion = AtlusScriptLibrary.FlowScriptLanguage.FormatVersion;
using ASTLogEventArgs = AtlusScriptLibrary.Common.Logging.LogEventArgs;
using System.Text.Json;

namespace EVTUI;

public struct AudioCues
{
    public AudioCues()
    {
        EnCues = new LocaleCues();
        JpCues = new LocaleCues();
    }

    public LocaleCues EnCues { get; set; }
    public LocaleCues JpCues { get; set; }
}

public struct LocaleCues
{
    public LocaleCues()
    {
        EventVoice = new Dictionary<uint, MessageCue>();
        EventSFX   = new Dictionary<uint, MessageCue>();
        Common     = new Dictionary<uint, MessageCue>();
        Field      = new Dictionary<uint, MessageCue>();
    }

    public Dictionary<uint, MessageCue> EventVoice { get; set; }
    public Dictionary<uint, MessageCue> EventSFX   { get; set; }
    public Dictionary<uint, MessageCue> Common     { get; set; }
    public Dictionary<uint, MessageCue> Field      { get; set; }
}

public class MessageCue
{
    public MessageCue(string speakerName, string turnName, int indWithinTurn)
    {
        SpeakerName   = speakerName;
        TurnName      = turnName;
        IndWithinTurn = indWithinTurn;
    }

    public string                   SpeakerName   { get; set; }
    public string                   TurnName      { get; set; }
    public int                      IndWithinTurn { get; set; }
    public (uint Lower, uint Upper) CueRange      { get; set; }

    public string Stringification { get { return $"{SpeakerName} - {TurnName} ({IndWithinTurn})"; } }
}

public class AppLogListener : LogListener
{
    public string Text;

    // LogLevel.All includes LogLevel.Trace which gets soooooo slow for longer scripts
    public AppLogListener() : base(LogLevel.Debug | LogLevel.Info | LogLevel.Warning | LogLevel.Error | LogLevel.Fatal)
    {
        this.Text = "";
    }

    protected override void OnLogCore( object sender, ASTLogEventArgs e )
    {
        if (this.Text.Length > 0)
            this.Text += "\n";
        this.Text += $"{DateTime.Now} {e.ChannelName} {e.Level}: {e.Message}";
    }
}


public class ScriptManager
{
    protected DataManager config;

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private string GameName;
    private Dictionary<(byte, byte), string> EnCharLookup;
    private Dictionary<(byte, byte), string> JpCharLookup;

    private AtlusEncoding Encoding => GetEncodingFromLang(config.Language);
    private Library Lib;

    private static Dictionary<string, string> CpkLanguages = new()
    {
        { "EN.CPK", "English" },
        { "JP.CPK", "Japanese" },
        { "BASE.CPK", "Japanese" },
        { "FR.CPK", "French" },
        { "IT.CPK", "Italian" },
        { "DE.CPK", "German" },
        { "ES.CPK", "Spanish" },
        { "KR.CPK", "Korean" },
        { "SC.CPK", "Simplified Chinese" },
        { "TC.CPK", "Traditional Chinese" }
    };
    private static Dictionary<string, string> LangCodeToP5Encoding { get; } = new()
    {
        { "en", "P5" },
        { "ja", "P5" },
        { "fr", "P5" },
        { "it", "P5" },
        { "de", "P5" },
        { "es", "P5" },
        { "ko", "P5_Korean" },
        { "zh-Hans", "P5_Chinese" },
        { "zh-Hant", "P5_Chinese" }
    };
    private static Dictionary<string, string> LangCodeToP5REncoding { get; } = new()
    {
        { "en", "P5R_EFIGS" },
        { "ja", "P5R_Japanese" },
        { "fr", "P5R_EFIGS" },
        { "it", "P5R_EFIGS" },
        { "de", "P5R_EFIGS" },
        { "es", "P5R_EFIGS" },
        { "ko", "P5_Korean" },
        { "zh-Hans", "P5R_CHS" },
        { "zh-Hant", "P5R_CHT" }
    };

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public string?                                        ActiveBMD = null;
    public Dictionary<string, BMD>                        BMDFiles { get; }
    public string?                                        ActiveBF = null;

    public static Dictionary<string, string> LangCodeDict { get; } = new()
    {
        { "English", "en" },
        { "Japanese", "ja" },
        { "French", "fr" },
        { "Italian", "it" },
        { "German", "de" },
        { "Spanish", "es" },
        { "Korean", "ko" },
        { "Simplified Chinese", "zh-Hans" },
        { "Traditional Chinese", "zh-Hant" }
    };

    public record ScriptInfo(string path, string scriptKind)
    {
        public string cpk => path.Substring(0, path.IndexOfAny(['\\', '/']));
        public string pathInCpk => path.Substring(path.IndexOfAny(['\\', '/']) + 1);

        public string? flowPath;
        public string? msgPath;
    }
    public record VanillaScriptInfo(string path, string scriptKind) : ScriptInfo(path, scriptKind)
    {
        public string language => CpkToLang(cpk); // TODO: this probably wont work with console...
    }
    public record ModdedScriptInfo(string path, string scriptKind, bool isEmulated) : ScriptInfo(path, scriptKind)
    {
        //public Dictionary<string, string> localizedFlowPaths; // most cases of this being used will probably be non-emu bfs. emulated ones *could* use it but there arent a lot of cases where you would
        //public Dictionary<string, string> localizedMsgPaths;

        // additional script files imported by the main .flow
        public List<string> additionalFlowPaths;
        public List<string> additionalMsgPaths;
    }

    public List<VanillaScriptInfo> VanillaScripts { get; }
    public List<ModdedScriptInfo> ModdedScripts { get; }

    public Dictionary<string, string?> fileTexts { get; } // full path to decomped file as key, text of file as value
    public Dictionary<string, Dictionary<string, string>> LocalizedFileDict { get; } // english path as top level key, langcode as nested key, localized path as nested value
    public Dictionary<string, Dictionary<string, string>> ScriptErrors { get; } // script object's path member as top-level key, langcode as nested key, error text as nested value. (not using script object directly as its fields are mutable)

    // TODO: this definitely belongs... somewhere else. probably in the actual AudioPreview ViewModel
    public AudioCues EventCues
    {
        get
        {
            AudioCues eventCues = new AudioCues();
            foreach (string key in this.BMDFiles.Keys)
            {
                bool isJP = key.Contains("JP.CPK") || key.Contains("BASE.CPK");
                LocaleCues locale;
                if (isJP)
                    locale = eventCues.JpCues;
                else
                    locale = eventCues.EnCues;
                try
                {
                foreach (Turn turn in this.BMDFiles[key].Turns)
                    for (int indWithinTurn=0; indWithinTurn<turn.Elems.Length; indWithinTurn++)
                        foreach (Node node in this.Parse(turn.Elems[indWithinTurn], isJP))
                        {
                            try
                            {
                                string turnName = this.Parse(turn.Name, isJP)[0].Text;
                                string speakerName = "";
                                if (turn.SpeakerId == 0xFFFF)
                                {
                                    if (turnName.StartsWith("MND_"))
                                        speakerName = "(JOKER)";
                                    else
                                        speakerName = "(SYSTEM)";
                                }
                                else
                                    speakerName = this.Parse(this.BMDFiles[key].Speakers[turn.SpeakerId], isJP)[0].Text;
                                if (node.FunctionTableIndex == 3 && node.FunctionIndex == 1 && node.FunctionArguments[3] != 0)
                                {
                                    // apparently this is doing something but i don't understand how...
                                    // this should really all be in the ViewModel and not the Model, anyway
                                    // TODO
                                    Dictionary<uint, MessageCue> messageCues = new Dictionary<uint, MessageCue>();
                                    switch (node.FunctionArguments[1])
                                    {
                                        case 0:
                                            if (speakerName.StartsWith("(SE)"))
                                                messageCues = locale.EventSFX;
                                            else
                                                messageCues = locale.EventVoice;
                                            break;
                                        case 1:
                                            messageCues = locale.Field;
                                            break;
                                        case 2:
                                            messageCues = locale.Common;
                                            break;
                                        default:
                                            break;
                                    }
                                    messageCues[node.FunctionArguments[3]] = new MessageCue(speakerName, turnName, indWithinTurn+1);
                                }
                            } catch (Exception ex) { Trace.TraceError(ex.ToString()); }
                        }
                // seems like sometimes the BMDFile can be null... or the Turns can be null? unclear
                } catch (Exception ex) { Trace.TraceError(ex.ToString()); }
            }
            return eventCues;
        }
    }

    public List<string> TurnNames
    {
        get
        {
            List<string> names = new List<string>();
            if (!(this.ActiveBMD is null))
                foreach (Turn turn in this.BMDFiles[this.ActiveBMD].Turns)
                {
                    string name = "";
                    foreach (Node node in this.Parse(turn.Name, this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                        name += node.Text;
                }
            return names;
        }
    }

    public List<string> MsgNames
    {
        get
        {
            List<string> names = new List<string>();
            if (!(this.ActiveBMD is null))
                for (int i=0; i<this.BMDFiles[this.ActiveBMD].TurnCount; i++)
                {
                    string name = "";
                    foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                        name += node.Text;
                    if (this.BMDFiles[this.ActiveBMD].TurnKinds[i] == 0)
                        names.Add(name);
                }
            return names;
        }
    }

    public List<string> SelNames
    {
        get
        {
            List<string> names = new List<string>();
            if (!(this.ActiveBMD is null))
                for (int i=0; i<this.BMDFiles[this.ActiveBMD].TurnCount; i++)
                {
                    string name = "";
                    foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                        name += node.Text;
                    if (this.BMDFiles[this.ActiveBMD].TurnKinds[i] != 0)
                        names.Add(name);
                }
            return names;
        }
    }

    public List<string> SpeakerNames
    {
        get
        {
            List<string> names = new List<string>();
            if (!(this.ActiveBMD is null))
            {
                foreach (byte[] speaker in this.BMDFiles[this.ActiveBMD].Speakers)
                {
                    string name = "";
                    foreach (Node node in this.Parse(speaker, this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                        name += node.Text;
                    names.Add(name);
                }
                names.Add("(UNNAMED)");
            }
            return names;
        }
    }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ScriptManager(DataManager config)
    {
        this.config = config;

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        VanillaScripts = new List<VanillaScriptInfo>();
        ModdedScripts = new List<ModdedScriptInfo>();

        LocalizedFileDict = new();
        fileTexts = new Dictionary<string, string>();
        ScriptErrors = new();

        this.BMDFiles = new();
        // TODO: actually make BF Serializable :')
        // this.BFFiles = new();
    }

    public void Dispose()
    {
        VanillaScripts.Clear();
        ModdedScripts.Clear();

        fileTexts.Clear();
        LocalizedFileDict.Clear();
        ScriptErrors.Clear();

        this.config = null;
    }

    public string RemovePrefix(string prefix, string s)
    {
        return s.Substring((prefix.Length+1), s.Length-(prefix.Length+1));
    }

    public string BasePath(string parentDir, string fileBase)
    {
        return Path.Combine(parentDir, Path.GetDirectoryName(fileBase), Path.GetFileNameWithoutExtension(fileBase));
    }

    public static string GetLangCodeFromLang(string language) => LangCodeDict[language];
    public static string CpkToLang(string cpk) => CpkLanguages[cpk];

    public AtlusEncoding GetEncodingFromLang(string language) => GetEncodingFromLangCode(GetLangCodeFromLang(language));
    private AtlusEncoding GetEncodingFromLangCode(string langCode)
    {
        bool p5r = config.ProjectManager.ActiveGame.Type.StartsWith("P5R");
        try
        {
            var charsetName = p5r ? LangCodeToP5REncoding[langCode] : LangCodeToP5Encoding[langCode];
            return AtlusEncoding.Create(charsetName);
        }
        catch (Exception e)
        {
            return p5r ? AtlusEncoding.Persona5RoyalEFIGS : AtlusEncoding.Persona5;
        }
    }

    public void ExportScripts(Func<ModdedScriptInfo, bool> predicate, out List<string> failed)
    {
        failed = new List<string>();
        foreach (var script in ModdedScripts.Where(predicate))
            if (!TryExportScript(script, out var failedLangs))
                failed.Add(script.path + $" ({string.Join(", ", failedLangs)})");
    }
    //public void ExportScript(string fileRelativePath) => ExportScript(ModdedScripts.First(s => s.path == fileRelativePath));
    public bool TryExportScript(ModdedScriptInfo script, out List<string> failed)
    {
        failed = new();

        if (script.msgPath is not null && fileTexts[script.msgPath] is not null && fileTexts[script.msgPath] != string.Empty)
        {
            File.WriteAllText(script.msgPath, fileTexts[script.msgPath]);
            if (LocalizedFileDict.ContainsKey(script.msgPath))
            {
                foreach (var msg in LocalizedFileDict[script.msgPath].Values)
                    File.WriteAllText(msg, fileTexts[msg]);
            }
        }

        if (script.flowPath is not null && fileTexts[script.flowPath] is not null && fileTexts[script.flowPath] != string.Empty)
        {
            File.WriteAllText(script.flowPath, fileTexts[script.flowPath]);
            if (LocalizedFileDict.ContainsKey(script.flowPath))
            {
                foreach (var flow in LocalizedFileDict[script.flowPath].Values)
                    File.WriteAllText(flow, fileTexts[flow]);
            }
        }

        if (script.additionalFlowPaths is not null)
        {
            foreach (var path in script.additionalFlowPaths)
            {
                if (fileTexts[path] is not null)
                    File.WriteAllText(path, fileTexts[path]);

                if (LocalizedFileDict.ContainsKey(path))
                {
                    foreach (var localized in LocalizedFileDict[path].Values)
                        File.WriteAllText(localized, fileTexts[localized]);
                }
            }
        }

        if (script.additionalMsgPaths is not null)
        {
            foreach (var path in script.additionalMsgPaths)
            {
                if (fileTexts[path] is not null)
                    File.WriteAllText(path, fileTexts[path]);

                if (LocalizedFileDict.ContainsKey(path))
                {
                    foreach (var localized in LocalizedFileDict[path].Values)
                        File.WriteAllText(localized, fileTexts[localized]);
                }
            }
        }

        if (script.isEmulated) // TODO: update message preview? the non-emu case does it...
        {
            string dummyFilePath = Path.Combine(config.ProjectManager.ModdedFileDir, script.path);
            if (!File.Exists(dummyFilePath))
                File.Create(dummyFilePath).Dispose();

            return true;
        }
        else
        {
            var log = string.Empty;
            if (script.scriptKind == "BF")
            {
                var success = TryCompileBF(script.flowPath, out var flowScript, out log);
                if (success)
                    try
                    {
                        flowScript.ToFile(Path.Combine(config.ProjectManager.ModdedFileDir, script.path));
                    }
                    catch (Exception ex)
                    {
                        log += ('\n' + ex.Message);
                        failed.Add(GetLangCodeFromLang(config.Language));
                        success = false;
                    }
                else
                    failed.Add(GetLangCodeFromLang(config.Language));
                ScriptErrors[script.path][GetLangCodeFromLang(config.Language)] = log;

                if (LocalizedFileDict.ContainsKey(script.flowPath))
                    foreach (var (langcode, flow) in LocalizedFileDict[script.flowPath])
                    {
                        success = TryCompileBF(flow, GetEncodingFromLangCode(langcode), out flowScript, out log);
                        if (success)
                            try
                            {
                                flowScript.ToFile(Path.ChangeExtension(flow, ".bf"));
                            }
                            catch (Exception ex)
                            {
                                log += ('\n' + ex.Message);
                                failed.Add(langcode);
                                success = false;
                            }
                        else
                            failed.Add(langcode);
                        ScriptErrors[script.path][langcode] = log;
                    }
                return success;
            }
            else
            {
                var success = TryCompileBMD(script, out var msgScript, out log);
                if (success)
                    try
                    {
                        msgScript.ToFile(Path.Combine(config.ProjectManager.ModdedFileDir, script.path));
                    }
                    catch (Exception ex)
                    {
                        log += ('\n' + ex.Message);
                        failed.Add(GetLangCodeFromLang(config.Language));
                        success = false;
                    }
                else
                    failed.Add(GetLangCodeFromLang(config.Language));
                ScriptErrors[script.path][GetLangCodeFromLang(config.Language)] = log;

                if (LocalizedFileDict.ContainsKey(script.msgPath))
                    foreach (var (langcode, msg) in LocalizedFileDict[script.msgPath])
                    {
                        success = TryCompileBMD(msg, GetEncodingFromLangCode(langcode), out msgScript, out log);
                        if (success)
                        {
                            try
                            {
                                string exportPath = Path.ChangeExtension(msg, ".bmd");
                                msgScript.ToFile(exportPath);

                                var key = Path.Combine(langcode, script.pathInCpk);
                                var workingPath = Path.Combine(config.WorkingPath, key);

                                File.Copy(exportPath, workingPath, true);
                                BMDFiles[key].Read(workingPath);
                            }
                            catch (Exception ex)
                            {
                                log += ('\n' + ex.Message);
                                failed.Add(langcode);
                                success = false;
                            }
                        }
                        else
                            failed.Add(langcode);
                        ScriptErrors[script.path][langcode] = log;
                    }
                return success;
            }
        }
    }

    public bool TryCompileBF(ModdedScriptInfo bf, out FlowScript script, out string log) => TryCompileBF(bf, config.Language, out script, out log);
    public bool TryCompileBF(ModdedScriptInfo bf, string language, out FlowScript script, out string log)
    {
        bool success;
        if (bf.isEmulated)
        {
            var vanillaBf = FindMatchingVanillaScript(bf, language);
            var vanillaBfPath = vanillaBf is null ? null : Path.Combine(config.VanillaExtractionPath, vanillaBf.path);

            string workingFlowPath = null;
            string workingMsgPath = null;

            var files = bf.additionalFlowPaths.Concat(bf.additionalMsgPaths).ToList();
            files.Add(bf.flowPath);
            files.Add(bf.msgPath);

            foreach (var file in files)
            {
                var localizedFile = GetLocalizedFilePath(file, language);
                if (fileTexts.ContainsKey(localizedFile) && fileTexts[localizedFile] is not null)
                {
                    var workingPath = Path.Combine(config.WorkingPath, GetLangCodeFromLang(language), RemovePrefix(config.ProjectManager.EmulatedBfDir, file));
                    if (file == bf.flowPath)
                        workingFlowPath = workingPath;
                    if (file == bf.msgPath)
                        workingMsgPath = workingPath;

                    if (!Directory.Exists(Path.GetDirectoryName(workingPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(workingPath));
                    File.WriteAllText(workingPath, fileTexts[localizedFile]);
                }
            }

            success = TryCompileEmulatedBF(vanillaBfPath, workingFlowPath, workingMsgPath is not null && workingFlowPath is null ? new() { workingMsgPath } : new(), GetEncodingFromLang(language), out script, out log);
        }
        else
            success = TryCompileBF(GetLocalizedFilePath(bf.flowPath, language), GetEncodingFromLang(language), out script, out log);

        ScriptErrors[bf.path][GetLangCodeFromLang(language)] = log;
        return success;
    }

    public bool TryCompileBF(string flowPath, out FlowScript script, out string log) => TryCompileBF(flowPath, this.Encoding, out script, out log);
    public bool TryCompileBF(string flowPath, AtlusEncoding encoding, out FlowScript script, out string log)
    {
        var listener = new AppLogListener();
        bool success;
        script = null;

        try
        {
            var compiler = new FlowScriptCompiler(FlowFormatVersion.Version3BigEndian);
            compiler.Encoding = encoding;
            compiler.Library = this.Lib;
            compiler.EnableProcedureTracing = false;
            compiler.AddListener(listener);

            success = compiler.TryCompile(fileTexts[flowPath], out script);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            listener.Text += ex.ToString();
            success = false;
        }
        log = listener.Text;
        return success;
    }

    public bool TryCompileBMD(ModdedScriptInfo bmd, out MessageScript script, out string log) => TryCompileBMD(bmd, config.Language, out script, out log);
    public bool TryCompileBMD(ModdedScriptInfo bmd, string language, out MessageScript script, out string log)
    {
        bool success;
        if (bmd.isEmulated)
        {
            var vanillaBmd = FindMatchingVanillaScript(bmd, language);
            var vanillaBmdPath = vanillaBmd is null ? null : Path.Combine(config.VanillaExtractionPath, vanillaBmd.path);

            var msgPath = GetLocalizedFilePath(bmd.msgPath, language);
            var workingPath = Path.Combine(config.WorkingPath, GetLangCodeFromLang(language), RemovePrefix(config.ProjectManager.EmulatedBmdDir, msgPath));
            if (fileTexts.ContainsKey(msgPath) && fileTexts[msgPath] is not null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(workingPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(workingPath));
                File.WriteAllText(workingPath, fileTexts[msgPath]);
            }
            success = TryCompileEmulatedBMD(vanillaBmdPath, new() { workingPath }, GetEncodingFromLang(language), out script, out log);
        }
        else
            success = TryCompileBMD(GetLocalizedFilePath(bmd.msgPath, language), GetEncodingFromLang(language), out script, out log);

        ScriptErrors[bmd.path][GetLangCodeFromLang(language)] = log;
        if (success)
        {
            try // somehow i'm managing to get files that compile with bad encoding only to fail here so fuck it, extra try-catch
            {
                var key = Path.Combine(GetLangCodeFromLang(language), bmd.pathInCpk);
                string workingPath = Path.Combine(config.WorkingPath, key);
                script.ToFile(workingPath);

                if (!BMDFiles.ContainsKey(key))
                    BMDFiles[key] = new();
                BMDFiles[key].Read(workingPath);
            }
            catch (Exception ex)
            {
                ScriptErrors[bmd.path][GetLangCodeFromLang(language)] += ('\n' + ex.Message);
                success = false;
            }
        }

        return success;
    }

    public bool TryCompileBMD(string msgPath, out MessageScript script, out string log) => TryCompileBMD(msgPath, this.Encoding, out script, out log);
    public bool TryCompileBMD(string msgPath, AtlusEncoding encoding, out MessageScript script, out string log)
    {
        var listener = new AppLogListener();
        bool success;
        script = null;

        try
        {
            var compiler = new MessageScriptCompiler(MsgFormatVersion.Version1BigEndian, encoding);
            compiler.Library = this.Lib;
            compiler.AddListener(listener);

            success = compiler.TryCompile(fileTexts[msgPath], out script);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            listener.Text += ex.ToString();
            success = false;
        }

        log = listener.Text;
        return success;
    }

    //public bool TryCompileEmulatedBF(string baseBf, string flowImport, List<string> importPaths, out FlowScript script, out string log) => TryCompileEmulatedBF(baseBf, flowImport, importPaths, this.Encoding, out script, out log);
    public bool TryCompileEmulatedBF(string baseBf, string flowImport, List<string> importPaths, AtlusEncoding encoding, out FlowScript script, out string log)
    {
        // importPaths should contain .msg import *if* .flow import does not exist, otherwise importPaths should be empty/other mod imports only and flowImport should just import the .msg file directly
        // it's not a big deal if we double import for some reason (the dupe will just be ignored), but we should try to mimic actual femu behavior when possible so users can trust that what works here works in actual mods
        var listener = new AppLogListener();
        bool success;
        script = null;

        try
        {
            var compiler = new FlowScriptCompiler(FlowFormatVersion.Version3BigEndian);
            compiler.Encoding = encoding;
            compiler.Library = this.Lib;
            compiler.EnableProcedureTracing = false;
            compiler.OverwriteExistingMsgs = true;
            compiler.OverwriteExistingProcedures = true;
            compiler.ProcedureHookMode = ProcedureHookMode.ImportedOnly;
            compiler.AddListener(listener);

            FileStream baseBfStream = File.Exists(baseBf) ? new FileStream(baseBf, FileMode.Open) : null;
            success = compiler.TryCompileWithImports(baseBfStream, importPaths, flowImport, out script);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            listener.Text += ex.ToString();
            success = false;
        }
        log = listener.Text;
        return success;
    }

    //public bool TryCompileEmulatedBMD(string baseBmd, List<string> importPaths, out MessageScript script, out string log) => TryCompileEmulatedBMD(baseBmd, importPaths, this.Encoding, out script, out log);
    public bool TryCompileEmulatedBMD(string baseBmd, List<string> importPaths, AtlusEncoding encoding, out MessageScript script, out string log)
    {
        var listener = new AppLogListener();
        bool success;
        script = null;

        try
        {
            var compiler = new MessageScriptCompiler(MsgFormatVersion.Version1BigEndian, encoding);
            compiler.Library = this.Lib;
            compiler.AddListener(listener);
            compiler.OverwriteExistingMsgs = true;

            FileStream baseBmdStream = File.Exists(baseBmd) ? new FileStream(baseBmd, FileMode.Open) : null;
            success = compiler.TryCompileWithImports(baseBmdStream, importPaths, out script);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            listener.Text += ex.ToString();
            success = false;
        }
        log = listener.Text;
        return success;
    }

    public Dictionary<string, string> FindLocalizedFiles(string englishFileRoute)
    {
        var localizedFiles = new Dictionary<string, string>();
        foreach (var langCode in LangCodeDict.Values)
        {
            var curLangFolder = Path.Combine(config.ProjectManager.LocalizedFileDir, langCode);
            if (!Directory.Exists(curLangFolder)) continue;
            var files = Directory.EnumerateFiles(curLangFolder, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var file in files)
            {
                if (englishFileRoute.EndsWith(Path.GetRelativePath(curLangFolder, file), StringComparison.InvariantCultureIgnoreCase))
                {
                    localizedFiles[langCode] = file;
                    fileTexts[file] = File.ReadAllText(file);
                    break;
                }
            }
        }
        return localizedFiles;
    }

    public string GetLocalizedFilePath(string file, string language)
    {
        var langCode = GetLangCodeFromLang(language);

        if (language != "English" && LocalizedFileDict.ContainsKey(file) && LocalizedFileDict[file].ContainsKey(langCode))
            return LocalizedFileDict[file][langCode];
        else
            return file;
    }

    public void FetchScripts()
    {
        // TODO: I gooootta handle game/locale/encoding more elegantly, lmao
        if (config.ProjectManager.ActiveGame.Type.StartsWith("P5R"))
        {
            this.GameName = "p5r";
            this.EnCharLookup = CharLookup("P5R_EFIGS");
            this.JpCharLookup = CharLookup("P5R_Japanese");
        }
        else
        {
            this.GameName = "p5";
            this.EnCharLookup = CharLookup("P5");
            this.JpCharLookup = this.EnCharLookup;
        }
        Lib = LibraryLookup.GetLibrary(GameName);

        foreach (var vanillaBf in config.EventManager.VanillaBfPaths)
        {
            VanillaScriptInfo scriptInfo = new(Path.GetRelativePath(config.VanillaExtractionPath, vanillaBf), "BF");
            scriptInfo.flowPath = Path.ChangeExtension(vanillaBf, ".flow");
            scriptInfo.msgPath = Path.ChangeExtension(vanillaBf, ".msg");

            string error = string.Empty;
            string flowText = string.Empty;
            string? msgText = null;

            if (File.Exists(scriptInfo.flowPath) || TryDecompileBF(vanillaBf, GetEncodingFromLang(scriptInfo.language), scriptInfo.flowPath, out error))
            {
                flowText = File.ReadAllText(scriptInfo.flowPath);
                if (File.Exists(scriptInfo.msgPath)) { msgText = File.ReadAllText(scriptInfo.msgPath); }
                fileTexts[scriptInfo.flowPath] = flowText;
                fileTexts[scriptInfo.msgPath] = msgText;

                VanillaScripts.Add(scriptInfo);
            }
            // TODO: throw an exception if decomp fails...? since it should only happen with either bad dump or bad script tools
        }

        foreach (var vanillaBmd in config.EventManager.VanillaBmdPaths)
        {
            VanillaScriptInfo scriptInfo = new(Path.GetRelativePath(config.VanillaExtractionPath, vanillaBmd), "BMD");
            scriptInfo.msgPath = Path.ChangeExtension(vanillaBmd, ".msg");
            string error = string.Empty;
            string msgText = string.Empty;

            if (File.Exists(scriptInfo.msgPath) || TryDecompileBMD(vanillaBmd, GetEncodingFromLang(scriptInfo.language), scriptInfo.msgPath, out error))
            {
                msgText = File.ReadAllText(scriptInfo.msgPath);
                fileTexts[scriptInfo.msgPath] = msgText;
                scriptInfo.flowPath = null;

                VanillaScripts.Add(scriptInfo);
            }
            // TODO: throw an exception if decomp fails...? since it should only happen with either bad dump or bad script tools
        }

        if (!config.ReadOnly)
        {
            string moddedFileDir = config.ProjectManager.ModdedFileDir;
            bool locFrameworkEnabled = config.ProjectManager._hasFramework("LocalisationFramework");
            foreach (var moddedBf in config.EventManager.BfPaths)
            {
                ModdedScriptInfo scriptInfo = new(Path.GetRelativePath(moddedFileDir, moddedBf), "BF", false);
                scriptInfo.flowPath = Path.ChangeExtension(moddedBf, ".flow");
                scriptInfo.msgPath = Path.ChangeExtension(moddedBf, ".msg");

                string error = string.Empty;
                string flowText = string.Empty;
                string? msgText = null;

                if (File.Exists(scriptInfo.flowPath) || TryDecompileBF(moddedBf, this.Encoding, scriptInfo.flowPath, out error))
                {
                    flowText = File.ReadAllText(scriptInfo.flowPath);
                    if (File.Exists(scriptInfo.msgPath)) { msgText = File.ReadAllText(scriptInfo.msgPath); }
                    fileTexts[scriptInfo.flowPath] = flowText;
                    fileTexts[scriptInfo.msgPath] = msgText;

                    if (locFrameworkEnabled)
                    {
                        LocalizedFileDict[scriptInfo.flowPath] = FindLocalizedFiles(RemovePrefix(moddedFileDir, scriptInfo.flowPath));
                        LocalizedFileDict[scriptInfo.msgPath] = FindLocalizedFiles(RemovePrefix(moddedFileDir, scriptInfo.msgPath));
                    }

                    scriptInfo.additionalFlowPaths = new();
                    scriptInfo.additionalMsgPaths = new();

                    ModdedScripts.Add(scriptInfo);
                }

                ScriptErrors[scriptInfo.path] = new();
                ScriptErrors[scriptInfo.path][GetLangCodeFromLang(config.Language)] = error;
            }

            foreach (var moddedBmd in config.EventManager.BmdPaths)
            {
                ModdedScriptInfo scriptInfo = new(Path.GetRelativePath(moddedFileDir, moddedBmd), "BMD", false);
                scriptInfo.msgPath = Path.ChangeExtension(moddedBmd, ".msg");
                string error = string.Empty;
                string msgText = string.Empty;

                if (File.Exists(scriptInfo.msgPath) || TryDecompileBMD(moddedBmd, this.Encoding, scriptInfo.msgPath, out error))
                {
                    msgText = File.ReadAllText(scriptInfo.msgPath);
                    fileTexts[scriptInfo.msgPath] = msgText;
                    scriptInfo.flowPath = null;

                    if (locFrameworkEnabled)
                        LocalizedFileDict[scriptInfo.msgPath] = FindLocalizedFiles(RemovePrefix(moddedFileDir, scriptInfo.msgPath));

                    scriptInfo.additionalFlowPaths = null;
                    scriptInfo.additionalMsgPaths = null;

                    ModdedScripts.Add(scriptInfo);
                }

                ScriptErrors[scriptInfo.path] = new();
                ScriptErrors[scriptInfo.path][GetLangCodeFromLang(config.Language)] = error;
            }

            // find all .flow and .msg files associated with an emulated script
            // FEmulator routing allows for partial matching, so the relative path from FEmulator/BF (or BMD) to the file can be any substring of the cpk path where the filename matches (ignoring extensions)
            // in practice most people just put script files at the top level for that emulator (eg. FEmulator/BMD/filename.msg), we'll use this as default export path if no matching file exists
            // but mods with a lot of script files may use subfolders matching the route for organization which we want EVTUI to detect
            bool bfEmuEnabled = config.ProjectManager._hasFramework("BFEmulator");
            string emulatedBfDir = config.ProjectManager.EmulatedBfDir;
            if (bfEmuEnabled)
            {
                foreach (var emulatedBf in config.EventManager.EmulatedBfPaths)
                {
                    var dummyRelativePath = Path.GetRelativePath(moddedFileDir, emulatedBf);
                    var scriptInfo = new ModdedScriptInfo(dummyRelativePath, "BF", true);
                    var routeWithoutCpk = Path.ChangeExtension(scriptInfo.pathInCpk, null);
                    var fileName = Path.GetFileName(routeWithoutCpk);

                    var filesForThisRoute = Directory.EnumerateFiles(emulatedBfDir, "*.*", SearchOption.AllDirectories).Where(x => routeWithoutCpk.EndsWith(Path.ChangeExtension(RemovePrefix(emulatedBfDir, x), null), StringComparison.InvariantCultureIgnoreCase));
                    scriptInfo.flowPath = filesForThisRoute.FirstOrDefault(x => Path.GetExtension(x) == ".flow", Path.Combine(emulatedBfDir, fileName) + ".flow");
                    scriptInfo.msgPath = filesForThisRoute.FirstOrDefault(x => Path.GetExtension(x) == ".msg", Path.Combine(emulatedBfDir, fileName) + ".msg");

                    fileTexts[scriptInfo.flowPath] = File.Exists(scriptInfo.flowPath) ? File.ReadAllText(scriptInfo.flowPath) : null;
                    fileTexts[scriptInfo.msgPath] = File.Exists(scriptInfo.msgPath) ? File.ReadAllText(scriptInfo.msgPath) : null;

                    if (locFrameworkEnabled)
                    {
                        LocalizedFileDict[scriptInfo.flowPath] = FindLocalizedFiles(RemovePrefix(moddedFileDir, scriptInfo.flowPath));
                        LocalizedFileDict[scriptInfo.msgPath] = FindLocalizedFiles(RemovePrefix(moddedFileDir, scriptInfo.msgPath));
                    }

                    scriptInfo.additionalFlowPaths = new();
                    scriptInfo.additionalMsgPaths = new();

                    ModdedScripts.Add(scriptInfo);
                }
            }

            bool bmdEmuEnabled = config.ProjectManager._hasFramework("BMDEmulator");
            string emulatedBmdDir = config.ProjectManager.EmulatedBmdDir;
            if (bmdEmuEnabled)
            {
                foreach (var emulatedBmd in config.EventManager.EmulatedBmdPaths)
                {
                    var dummyRelativePath = Path.GetRelativePath(moddedFileDir, emulatedBmd);
                    var scriptInfo = new ModdedScriptInfo(dummyRelativePath, "BMD", true);
                    var routeWithoutCpk = Path.ChangeExtension(scriptInfo.pathInCpk, ".msg");
                    var fileName = Path.GetFileName(routeWithoutCpk);

                    scriptInfo.flowPath = null;
                    scriptInfo.msgPath = Directory.EnumerateFiles(emulatedBmdDir, "*.*", SearchOption.AllDirectories).FirstOrDefault(x => routeWithoutCpk.EndsWith(RemovePrefix(emulatedBmdDir, x), StringComparison.InvariantCultureIgnoreCase), Path.Combine(emulatedBmdDir, fileName));
                    fileTexts[scriptInfo.msgPath] = File.Exists(scriptInfo.msgPath) ? File.ReadAllText(scriptInfo.msgPath) : null;

                    if (locFrameworkEnabled)
                        LocalizedFileDict[scriptInfo.msgPath] = FindLocalizedFiles(RemovePrefix(moddedFileDir, scriptInfo.msgPath));

                    scriptInfo.additionalFlowPaths = null;
                    scriptInfo.additionalMsgPaths = null;

                    ModdedScripts.Add(scriptInfo);
                }
            }

            // create info for files that don't already exist
            // TODO: this should probably require a user prompt...
            /*var evt = config.EventManager.SerialEvent;

            if (!ModdedScripts.Exists(s => s.scriptKind == "BF"))
            {
                var bf = new ModdedScriptInfo(Path.Combine("EN.CPK", (evt.EventBfPath is null) ? $"event_data/script/e{(100 * (evt.MajorId / 100)):000}/e{evt.MajorId:000}_{evt.MinorId:000}.bf" : evt.EventBfPath.Replace("\0", "")), "BF", bfEmuEnabled);
                bf.flowPath = bfEmuEnabled ? Path.Combine(emulatedBfDir, Path.GetFileNameWithoutExtension(bf.pathInCpk)) + ".flow" : Path.ChangeExtension(Path.Combine(moddedFileDir, bf.path), ".flow");
                bf.msgPath = Path.ChangeExtension(bf.flowPath, ".msg");
                fileTexts[bf.flowPath] = File.Exists(bf.flowPath) ? File.ReadAllText(bf.flowPath) : string.Empty;
                fileTexts[bf.msgPath] = File.Exists(bf.msgPath) ? File.ReadAllText(bf.msgPath) : string.Empty;
                bf.localizedFlowPaths = locFrameworkEnabled ? new() : null;
                bf.localizedMsgPaths = locFrameworkEnabled ? new() : null;
                ModdedScripts.Add(bf);
            }

            if (!ModdedScripts.Exists(s => s.scriptKind == "BMD"))
            {
                var bmd = new ModdedScriptInfo(Path.Combine("EN.CPK", (evt.EventBmdPath is null) ? $"event_data/message/e{(100 * (evt.MajorId / 100)):000}/e{evt.MajorId:000}_{evt.MinorId:000}.bmd" : evt.EventBmdPath.Replace("\0", "")), "BMD", bmdEmuEnabled);
                bmd.flowPath = null;
                bmd.msgPath = bmdEmuEnabled ? Path.Combine(emulatedBmdDir, Path.GetFileNameWithoutExtension(bmd.pathInCpk)) + ".msg" : Path.ChangeExtension(Path.Combine(moddedFileDir, bmd.path), ".msg");
                fileTexts[bmd.msgPath] = File.Exists(bmd.msgPath) ? File.ReadAllText(bmd.msgPath) : string.Empty;
                bmd.localizedFlowPaths = null;
                bmd.localizedMsgPaths = locFrameworkEnabled ? new() : null;
                ModdedScripts.Add(bmd);
            }*/
        }

        foreach (var script in ModdedScripts)
        {
            if (!ScriptErrors.ContainsKey(script.path))
                ScriptErrors[script.path] = new();

            foreach (var langCode in LangCodeDict.Values)
                if (!ScriptErrors[script.path].ContainsKey(langCode))
                    ScriptErrors[script.path][langCode] = string.Empty;

            GetLibraryOverrides(script);
            GetAdditionalScriptImports(script);
        }

        InitializeWorkingDir();
        ChangeActiveScripts();
    }

    private void GetAdditionalScriptImports(ModdedScriptInfo script)
    {
        if (script.scriptKind != "BF" || fileTexts[script.flowPath] is null || fileTexts[script.flowPath] == string.Empty)
            return;

        GetAdditionalScriptImports(script.flowPath, ref script);
    }

    // might be worth asyncifying this? since potentially reading multiple files at a time
    // hell, might be worth asyncifying FetchScripts itself
    private void GetAdditionalScriptImports(string flowFile, ref ModdedScriptInfo script)
    {
        if (fileTexts[flowFile] is null || fileTexts[flowFile] == string.Empty)
            return;

        using (var reader = new StringReader(fileTexts[flowFile]))
        {
            var line = reader.ReadLine();
            if (line is null) // this shouldnt happen, we shouldnt get this far if fileTexts[flowFile] is empty, but just to be sure
                return;

            // this assumes all import statements are on consecutive lines which could be an issue in the case of non-emu files that have to be decompiled. i dont think non-emu bfs usually import anything other than the msg file though?
            while (line.StartsWith("import(\"") && line.EndsWith("\");"))
            {
                var relativePathToNextFile = line.Substring(line.IndexOf('\"') + 1, line.LastIndexOf('\"') - 1 - line.IndexOf('\"'));
                var nextFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(flowFile), relativePathToNextFile)); // we use Path.GetFullPath here to avoid eg. "C://path/to/../file.flow"

                // adding these to script info even though compilation will fail if these dont exist bc tbh you probably *want* compilation to fail in that case
                if (nextFile != script.msgPath && Path.GetExtension(nextFile).Equals(".msg", StringComparison.InvariantCultureIgnoreCase)) // weeding out the redundant import isnt too important tbh, we just want to save a little time during compilation
                    script.additionalMsgPaths.Add(nextFile);
                if (Path.GetExtension(nextFile).Equals(".flow", StringComparison.InvariantCultureIgnoreCase))
                    script.additionalFlowPaths.Add(nextFile);

                fileTexts[nextFile] = File.Exists(nextFile) ? File.ReadAllText(nextFile) : null;
                if (config.ProjectManager._hasFramework("LocalisationFramework"))
                {
                    string baseFolder = script.isEmulated ? config.ProjectManager.EmulatedBfDir : config.ProjectManager.ModdedFileDir;
                    LocalizedFileDict[nextFile] = FindLocalizedFiles(RemovePrefix(baseFolder, nextFile));
                }

                if (File.Exists(nextFile) && Path.GetExtension(nextFile).Equals(".flow", StringComparison.InvariantCultureIgnoreCase))
                    GetAdditionalScriptImports(nextFile, ref script);

                line = reader.ReadLine();
                if (line is null) // end of file reached
                    return;
            }
        }
    }

    private void GetLibraryOverrides(ModdedScriptInfo script)
    {
        if (script.scriptKind != "BF" || !script.isEmulated)
            return;

        var functionsPath = Path.Combine(Path.GetDirectoryName(script.flowPath), "Functions.json");
        if (File.Exists(functionsPath))
        {
            try
            {
                var functions = JsonSerializer.Deserialize<List<FlowScriptModuleFunction>>(File.ReadAllText(functionsPath));
                foreach (var f in functions)
                {
                    int module = -1;
                    int index = -1;

                    for (int i = 0; i < Lib.FlowScriptModules.Count; i++)
                    {
                        index = Lib.FlowScriptModules[i].Functions.FindIndex(x => x.Index == f.Index);
                        if (index != -1)
                        {
                            module = i;
                            break;
                        }
                    }

                    if (module != -1 && index != -1)
                    {
                        var existingFunc = Lib.FlowScriptModules[module].Functions[index];
                        if (f.ReturnType == existingFunc.ReturnType && f.Parameters.Select(p => p.Type).SequenceEqual(existingFunc.Parameters.Select(p => p.Type)))
                        {
                            existingFunc.Aliases ??= new List<string>();
                            existingFunc.Aliases.Add(f.Name);
                        }
                        else
                            Lib.FlowScriptModules[module].Functions[index] = f;
                    }
                    else
                        Lib.FlowScriptModules.Last().Functions.Add(f);
                }

            }
            catch (JsonException ex)
            {
                ScriptErrors[script.path][GetLangCodeFromLang(config.Language)] += ex.Message;
            }
        }

        var enumsPath = Path.Combine(Path.GetDirectoryName(script.flowPath), "Enums.json");
        if (File.Exists(enumsPath))
        {
            try
            {
                var enums = JsonSerializer.Deserialize<List<FlowScriptModuleEnum>>(File.ReadAllText(enumsPath));
                Lib.FlowScriptModules[0].Enums.AddRange(enums);
            }
            catch (JsonException ex)
            {
                ScriptErrors[script.path][GetLangCodeFromLang(config.Language)] += ex.Message;
            }
        }
    }

    private VanillaScriptInfo FindMatchingVanillaScript(ModdedScriptInfo moddedScript, string language)
    {
        VanillaScriptInfo vanillaScript = VanillaScripts.FirstOrDefault(s => s.pathInCpk.Equals(moddedScript.pathInCpk, StringComparison.InvariantCultureIgnoreCase) && s.language == language, null);
        vanillaScript ??= VanillaScripts.FirstOrDefault(s => s.pathInCpk.Equals(moddedScript.pathInCpk, StringComparison.InvariantCultureIgnoreCase) && s.language == "Japanese", null);

        return vanillaScript;
    }

    public void ChangeActiveScripts()
    {
        if (ModdedScripts.Count == 0)
        {
            if (VanillaScripts.Exists(s => s.scriptKind == "BF"))
            {
                var vanillaScript = VanillaScripts.FirstOrDefault(s => s.language == config.Language && s.scriptKind == "BF", null);
                if (config.Language != "Japanese")
                    vanillaScript ??= VanillaScripts.First(s => s.scriptKind == "BF" && s.language == "Japanese");

                ActiveBF = Path.Combine(GetLangCodeFromLang(vanillaScript.language), vanillaScript.pathInCpk);
            }

            if (VanillaScripts.Exists(s => s.scriptKind == "BMD"))
            {
                var vanillaScript = VanillaScripts.FirstOrDefault(s => s.language == config.Language && s.scriptKind == "BMD", null);
                if (config.Language != "Japanese")
                    vanillaScript ??= VanillaScripts.First(s => s.scriptKind == "BMD" && s.language == "Japanese");

                ActiveBMD = Path.Combine(GetLangCodeFromLang(vanillaScript.language), vanillaScript.pathInCpk);
            }
        }
        else
        {
            var bf = ModdedScripts.FirstOrDefault(s => s.scriptKind == "BF", null);
            if (bf is not null)
                ActiveBF = Path.Combine(GetLangCodeFromLang(config.Language), bf.pathInCpk);

            var bmd = ModdedScripts.FirstOrDefault(s => s.scriptKind == "BMD", null);
            if (bmd is not null)
                ActiveBMD = Path.Combine(GetLangCodeFromLang(config.Language), bmd.pathInCpk);
        }
    }

    private void InitializeWorkingDir() // i feel like theres a better name for this but i cant think of it lol
    {
        if (VanillaScripts.Exists(s => s.scriptKind == "BMD") && !ModdedScripts.Exists(s => s.scriptKind == "BMD"))
            foreach (var lang in LangCodeDict.Keys)
            {
                var vanillaScript = VanillaScripts.FirstOrDefault(s => s.scriptKind == "BMD" && s.language == lang, null);
                if (lang != "Japanese")
                    vanillaScript ??= VanillaScripts.First(s => s.scriptKind == "BMD" && s.language == "Japanese");

                var key = Path.Combine(GetLangCodeFromLang(vanillaScript.language), vanillaScript.pathInCpk);
                BMDFiles[key] = new();
                BMDFiles[key].Read(Path.Combine(config.VanillaExtractionPath, vanillaScript.path));
            }
        // TODO same as above but for bfs lol
        if (ModdedScripts.Count == 0)
            return;

        string workingDir = config.WorkingPath;
        if (!Directory.Exists(workingDir))
            Directory.CreateDirectory(workingDir);
        foreach (var lang in LangCodeDict.Values)
            Directory.CreateDirectory(Path.Combine(workingDir, lang));

        void UseVanillaScriptForPreview(ModdedScriptInfo script, string language)
        {
            string key = Path.Combine(GetLangCodeFromLang(language), script.pathInCpk);
            VanillaScriptInfo vanillaScript = FindMatchingVanillaScript(script, language);
            if (vanillaScript == null)
            {
                BMDFiles[key] = null;
                return;
            }

            if (vanillaScript.scriptKind == "BMD")
            {
                var scriptPath = Path.Combine(workingDir, key);
                if (!Directory.Exists(Path.GetDirectoryName(scriptPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(scriptPath));
                File.Copy(Path.Combine(config.VanillaExtractionPath, vanillaScript.path), scriptPath, true);

                if (!BMDFiles.ContainsKey(key))
                    BMDFiles[key] = new();
                BMDFiles[key].Read(scriptPath);
            }
            // TODO bfs lol
        }

        foreach (var script in ModdedScripts)
        {
            if (script.scriptKind == "BMD")
            {
                if (script.isEmulated)
                {
                    foreach (var lang in LangCodeDict.Keys)
                    {
                        if (!TryCompileBMD(script, lang, out var msgScript, out _))
                            UseVanillaScriptForPreview(script, lang);
                    }
                }
                else
                {
                    var enKey = Path.Combine("en", script.pathInCpk);
                    var enScriptPath = Path.Combine(workingDir, enKey);
                    string pathInMod = Path.Combine(config.ProjectManager.ModdedFileDir, script.path);
                    if (File.Exists(pathInMod))
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(enScriptPath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(enScriptPath));
                        File.Copy(pathInMod, enScriptPath, true);

                        BMDFiles[enKey] = new();
                        BMDFiles[enKey].Read(enScriptPath);
                    }
                    else
                    {
                        if (!File.Exists(script.msgPath) || !TryCompileBMD(script, out var msgScript, out _)) // loc framework won't work without a non-localized file so treat all languages as vanilla
                        {
                            foreach (var lang in LangCodeDict.Keys)
                                UseVanillaScriptForPreview(script, lang);
                            continue;
                        }
                    }

                    foreach (var (lang, langCode) in LangCodeDict)
                    {
                        if (lang == "English")
                            continue;

                        var key = Path.Combine(langCode, script.pathInCpk);
                        var localizedScriptPath = Path.Combine(workingDir, key);
                        if (!Directory.Exists(Path.GetDirectoryName(localizedScriptPath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(localizedScriptPath));

                        if (!LocalizedFileDict.ContainsKey(script.msgPath) || !LocalizedFileDict[script.msgPath].ContainsKey(langCode))
                            BMDFiles[key] = BMDFiles[enKey];
                        else if (File.Exists(Path.ChangeExtension(LocalizedFileDict[script.msgPath][langCode], ".bmd")))
                        {
                            File.Copy(Path.ChangeExtension(LocalizedFileDict[script.msgPath][langCode], ".bmd"), localizedScriptPath, true);
                            BMDFiles[key] = new();
                            BMDFiles[key].Read(localizedScriptPath);
                        }
                        else if (!TryCompileBMD(script, lang, out var msgScript, out _))    // this file should be localized, but no working script exists...
                            BMDFiles[key] = BMDFiles[enKey];                                // do this separately from the LocalizedFileDict check because I/O
                    }
                }

            }
            // TODO: serialize bfs lol
        }
    }

    public bool TryDecompileBMD(string bmd, string outputPath, out string log) => TryDecompileBMD(bmd, this.Encoding, outputPath, out log);
    public bool TryDecompileBMD(string bmd, AtlusEncoding encoding, string outputPath, out string log)
    {
        log = string.Empty;
        try
        {
            var msgScript = MessageScript.FromFile(bmd, MsgFormatVersion.Detect, this.Encoding);
            using (var decompiler = new MessageScriptDecompiler(new FileTextWriter(outputPath)))
            {
                decompiler.Library = LibraryLookup.GetLibrary(this.GameName);
                decompiler.Decompile(msgScript);
            }
            return true;
        }
        catch (Exception ex)
        {
            log = ex.ToString();
            Trace.TraceError(log);
            return false;
        }
    }

    public bool TryDecompileBF(string bf, string outputPath, out string log) => TryDecompileBF(bf, this.Encoding, outputPath, out log);
    public bool TryDecompileBF(string bf, AtlusEncoding encoding, string outputPath, out string log)
    {
        bool success;
        log = string.Empty;
        var listener = new AppLogListener();
        try
        {
            var flowScript = FlowScript.FromFile(bf, encoding);
            var decompiler = new FlowScriptDecompiler();
            decompiler.Library = LibraryLookup.GetLibrary(this.GameName);
            decompiler.AddListener(listener);

            success = decompiler.TryDecompile(flowScript, outputPath);
        }
        catch (Exception ex)
        {
            log = ex.ToString();
            Trace.TraceError(log);
            listener.Text += log;

            success = false;
        }
        log = listener.Text;
        return success;
    }

    public int GetTurnIndex(Int16 majorId, byte minorId, byte _subId)
    {
        if (!(this.ActiveBMD is null))
            // it works even without an exact subId match, so... fallback logic
            foreach (byte subId in new[]{_subId, 0})
            {
                for (int i=0; i<this.BMDFiles[this.ActiveBMD].Turns.Length; i++)
                {
                    string name = "";
                    foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                        name += node.Text;
                    if (Regex.IsMatch(name, $"^[A-Z][A-Z][A-Z]_{majorId:000}_{minorId}_{subId}$"))
                        return i;
                }
            }
        return -1;
    }

    public int GetTurnIndex(string targetName)
    {
        if (!(this.ActiveBMD is null))
            for (int i=0; i<this.BMDFiles[this.ActiveBMD].Turns.Length; i++)
            {
                string name = "";
                foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                    name += node.Text;
                if (name == targetName)
                    return i;
            }
        return -1;
    }

    public string GetTurnName(int turnIndex)
    {
        string name = "";
        if (!(this.ActiveBMD is null))
            if (turnIndex >= 0 && turnIndex < this.BMDFiles[this.ActiveBMD].Turns.Length)
                foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[turnIndex].Name, this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                    name += node.Text;
        return name;
    }

    public string GetTurnSpeakerName(int turnIndex)
    {
        string name = "";
        if (!(this.ActiveBMD is null))
            if (turnIndex >= 0 && turnIndex < this.BMDFiles[this.ActiveBMD].Turns.Length && this.BMDFiles[this.ActiveBMD].Turns[turnIndex].SpeakerId != 0xFFFF)
                foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Speakers[this.BMDFiles[this.ActiveBMD].Turns[turnIndex].SpeakerId], this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                    name += node.Text;
        return name;
    }

    public int GetTurnElemCount(int turnIndex)
    {
        if (!(this.ActiveBMD is null))
            if (turnIndex >= 0 && turnIndex < this.BMDFiles[this.ActiveBMD].Turns.Length)
                return this.BMDFiles[this.ActiveBMD].Turns[turnIndex].ElemCount;
        return 0;
    }

    public string GetTurnText(int turnIndex, int elemIndex)
    {
        string text = "";
        if (!(this.ActiveBMD is null))
            if (turnIndex >= 0 && turnIndex < this.BMDFiles[this.ActiveBMD].Turns.Length)
                foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[turnIndex].Elems[elemIndex], this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                    text += node.Text;
        return text;
    }

    public (string Source, uint CueId)? GetTurnVoice(int turnIndex, int elemIndex)
    {
        if (!(this.ActiveBMD is null))
            foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[turnIndex].Elems[elemIndex], this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                if (node.FunctionTableIndex == 3 && node.FunctionIndex == 1 && node.FunctionArguments[3] != 0)
                    switch (node.FunctionArguments[1])
                    {
                        case 0:
                            if (this.GetTurnSpeakerName(turnIndex).StartsWith("(SE)"))
                                return ("SFX", node.FunctionArguments[3]);
                            else
                                return ("Voice", node.FunctionArguments[3]);
                        case 1:
                            return ("Field", node.FunctionArguments[3]);
                        case 2:
                            return ("Common", node.FunctionArguments[3]);
                        default:
                            break;
                    }
        return null;
    }

    public (string[] Prefix, string Suffix) GetTurnBustupPath(int turnIndex, int elemIndex)
    {
        if (!(this.ActiveBMD is null))
            foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[turnIndex].Elems[elemIndex], this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                if (node.FunctionTableIndex == 4 && node.FunctionIndex == 6)
                {
                    int characterId = (int)node.FunctionArguments[1];
                    int emoteId = (int)node.FunctionArguments[2];
                    int outfitId = -1;
                    if (node.FunctionArguments[3] != 0xFFFF)
                        outfitId = (int)node.FunctionArguments[3];
                    bool isOnCall = (node.FunctionArguments[4] != 0);

                    if (outfitId == -1)
                        return (new string[] { "bustup" }, $"b{characterId:000}_{emoteId:000}_[0-9][0-9]\\.bin");
                    else
                        return (new string[] { "bustup", $"b{characterId:000}_{emoteId:000}_{outfitId:00}.bin" }, null);
                }
        return (null, null);
    }

    public MagickImage GetMainBustupImage(string[] prefix, string suffix)
    {
        List<string> paths = config.ExtractExactFiles(prefix, suffix);
        if (paths.Count > 0)
        {
            AtlusArchive bin = new AtlusArchive();
            bin.Read(paths[0]);
            foreach (FileEntry entry in bin.Entries)
            {
                MagickReadSettings settings = new MagickReadSettings() { Format = MagickFormat.Dds };
                if (entry.Name.Replace("\0", "").Trim().EndsWith(".dds2"))
                {
                    AtlusArchive dds2 = new AtlusArchive();
                    dds2.FromBytes(entry.Data);
                    using MagickImageCollection images = new MagickImageCollection();
                    foreach (FileEntry dds in dds2.Entries)
                        images.Add(new MagickImage(new MemoryStream(dds.Data), settings));
                    return (MagickImage)images.Flatten(MagickColors.None);
                }
                else
                    return new MagickImage(new MemoryStream(entry.Data), settings);
            }
        }
        return null;
    }

    public (string[] Prefix, string Suffix) GetTurnCutinPath(int turnIndex, int elemIndex)
    {
        if (!(this.ActiveBMD is null))
            foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[turnIndex].Elems[elemIndex], this.ActiveBMD.Contains("JP.CPK") || this.ActiveBMD.Contains("BASE.CPK")))
                // it's a cutin command AND it's visible
                if (node.FunctionTableIndex == 4 && node.FunctionIndex == 17 && node.FunctionArguments[11] == 1)
                {
                    int category = 0;
                    int majorId = 0;
                    if (node.FunctionArguments[8] >= 200)
                    {
                        category = 2;
                        majorId = (int)node.FunctionArguments[8] - 200;
                    }
                    else if (node.FunctionArguments[8] >= 100)
                    {
                        category = 1;
                        majorId = (int)node.FunctionArguments[8] - 100;
                    }
                    else
                        majorId = (int)node.FunctionArguments[8];

                    int minorId = (int)node.FunctionArguments[9];

                    int subId = 0;
                    if ((node.FunctionArguments[10] & 2) == 1)
                        subId = 2;
                    else if ((node.FunctionArguments[10] & 1) == 1)
                        subId = 1;

                    if (category == 2)
                        return (new string[] { "cutin", "utarc", "2_sub_0", $"cutinsub_0_{majorId:00}_{minorId:00}.{subId:000}" }, null);
                    else if (category == 1)
                        return (new string[] { "cutin", "utarc", "1_com_0", $"cutincom_0_{majorId:00}_{minorId:00}.{subId:000}" }, null);
                    else
                        return (new string[] { "cutin", "utarc", "0_main_0", $"cutinmain_0_{majorId:00}_{minorId:00}.{subId:000}" }, null);
                }
        return (null, null);
    }

    public MagickImage GetMainCutinImage(string[] prefix, string suffix)
    {
        List<string> paths = config.ExtractExactFiles(prefix, suffix);
        if (paths.Count > 0)
        {
            AtlusArchive bin = new AtlusArchive(isCutin: true);
            bin.Read(paths[0]);
            foreach (FileEntry entry in bin.Entries)
            {
                GLH thing = new GLH();
                thing.FromBytes(entry.Data);
                var settings = new MagickReadSettings() { Format = MagickFormat.Dds };
                return new MagickImage(new MemoryStream(thing.DecompressedData.Data), settings);
            }
        }
        return null;
    }

    /////////////////////////////
    // *** PRIVATE METHODS *** //
    /////////////////////////////
    private static Dictionary<(byte, byte), string> CharLookup(string encoding)
    {
        Dictionary<(byte, byte), string> charLookup = new Dictionary<(byte, byte), string>();
        List<string> charSet = ReadCharTable(encoding);
        for (int charIndex=0; charIndex<charSet.Count; charIndex++)
        {
            int glyphIndex = (charIndex + 0x60);
            int tableIndex = ((glyphIndex / 0x80) - 1);
            int tableRelativeIndex = (glyphIndex - (tableIndex * 0x80));
            charLookup.Add(((byte)(0x80 | tableIndex), (byte)tableRelativeIndex), charSet[charIndex]);
        }
        return charLookup;
    }

    private static List<string> ReadCharTable(string encoding)
    {
        string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Charsets", $"{encoding}.tsv");
        List<string> charSet = new List<string>();
        foreach (string line in File.ReadLines(filename))
            foreach (string elem in line.Split("\t"))
                charSet.Add(System.Uri.UnescapeDataString(elem));
        return charSet;
    }

    private List<Node> Parse(byte[] unencoded, bool isJP)
    {
        List<Node> nodes = new List<Node>();
        Queue<byte> buffer = new Queue<byte>(unencoded);
        while (buffer.Count > 0)
        {
            byte b1 = buffer.Dequeue();
            if ((b1 & 0xF0) == 0xF0)
                nodes.Add(new Node(b1, buffer));
            else
            {
                Queue<byte> textbuff = new Queue<byte>();
                while (true)
                {
                    textbuff.Enqueue(b1);
                    if ((b1 & 0x80) == 0x80)
                        textbuff.Enqueue(buffer.Dequeue());
                    if (buffer.Count <= 0)
                        break;
                    b1 = buffer.Peek();
                    if ((b1 == 0) || (b1 == 0x0A) || ((b1 & 0xF0) == 0xF0))
                        break;
                    else
                       buffer.Dequeue();
                }
                nodes.Add(new Node(this.Encode(textbuff, isJP)));
            }
        }
        return nodes;
    }

    private struct Node
    {
        public Node(byte b1, Queue<byte> buffer)
        {
            FunctionId            = ((b1 << 8) | buffer.Dequeue());
            FunctionTableIndex    = ((FunctionId & 0xE0) >> 5);
            FunctionIndex         = (FunctionId & 0x1F);
            FunctionArgumentCount = ((((FunctionId >> 8) & 0xF) - 1) * 2);
            FunctionArguments     = new List<ushort>();
            for (int i=0; i<FunctionArgumentCount/2; i++)
                FunctionArguments.Add((ushort)((((buffer.Dequeue() - 1) & ~0xFF00) | (((buffer.Dequeue() - 1) << 8) & 0xFF00)) & 0xFFFF));
        }

        public Node(string text)
        {
            Text = text;
        }

        public int FunctionId;
        public int FunctionTableIndex;
        public int FunctionIndex;
        public int FunctionArgumentCount;
        public List<ushort> FunctionArguments;

        public string Text;
    }

    private string Encode(Queue<byte> unencoded, bool isJP)
    {
        string encoded = "";
        while (unencoded.Count > 0)
        {
            byte high = unencoded.Dequeue();
            if ((high & 0x80) == 0x80)
                try
                {
                    if (isJP)
                        encoded += this.JpCharLookup[(high, unencoded.Dequeue())];
                    else
                        encoded += this.EnCharLookup[(high, unencoded.Dequeue())];
                }
                catch (KeyNotFoundException ex)
                {
                    Trace.TraceError(ex.ToString());
                    encoded += "*";
                }
            else if (high != 0)
                encoded += (char)high;
        }
        return encoded;
    }

}
