using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

using FormatVersion = AtlusScriptLibrary.MessageScriptLanguage.FormatVersion;
using FlowFormatVersion = AtlusScriptLibrary.FlowScriptLanguage.FormatVersion;

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

    protected override void OnLogCore( object sender, LogEventArgs e )
    {
        if (this.Text.Length > 0)
            this.Text += "\n";
        this.Text += $"{DateTime.Now} {e.ChannelName} {e.Level}: {e.Message}";
    }
}


public class ScriptManager
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private string GameName;
    private Dictionary<(byte, byte), string> EnCharLookup;
    private Dictionary<(byte, byte), string> JpCharLookup;
    private AtlusEncoding Encoding;

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public string?                                        ActiveBMD = null;
    public Dictionary<string, BMD>                        BMDFiles { get; }

    public string?                                        ActiveBF = null;
    //public Dictionary<string, BF>                         BFFiles { get; }

    public Dictionary<string, List<string>>                                   ScriptList  { get; }
    public Dictionary<string, Dictionary<string, Dictionary<string, string>>> ScriptTexts { get; }
    public Dictionary<string, Dictionary<string, string>>                     ScriptErrors   { get; }

    // TODO: this definitely belongs... somewhere else. probably in the actual AudioPreview ViewModel
    public AudioCues EventCues
    {
        get
        {
            AudioCues eventCues = new AudioCues();
            foreach (string key in this.BMDFiles.Keys)
            {
                bool isJP = key.Contains("BASE.CPK");
                LocaleCues locale;
                if (isJP)
                    locale = eventCues.JpCues;
                else
                    locale = eventCues.EnCues;
                foreach (Turn turn in this.BMDFiles[key].Turns)
                    for (int indWithinTurn=0; indWithinTurn<turn.Elems.Length; indWithinTurn++)
                        foreach (Node node in this.Parse(turn.Elems[indWithinTurn], isJP))
                        {
                            try
                            {
                                string turnName = this.Parse(turn.Name, isJP)[0].Text;
                                string speakerName = "";
                                if (turn.SpeakerId == 0xFFFF && turnName.StartsWith("MND_"))
                                    speakerName = "Joker";
                                else
                                    speakerName = this.Parse(this.BMDFiles[key].Speakers[turn.SpeakerId], isJP)[0].Text;
                                if (node.FunctionTableIndex == 3 && node.FunctionIndex == 1 && node.FunctionArguments[3] != 0)
                                {
                                    Dictionary<uint, MessageCue> messageCues = null;
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
                                    messageCues.Add(node.FunctionArguments[3], new MessageCue(speakerName, turnName, indWithinTurn+1));
                                }
                            } catch (Exception ex) { Trace.TraceError(ex.ToString()); }
                        }
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
                    foreach (Node node in this.Parse(turn.Name, this.ActiveBMD.Contains("BASE.CPK")))
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
                    foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("BASE.CPK")))
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
                    foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("BASE.CPK")))
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
                    foreach (Node node in this.Parse(speaker, this.ActiveBMD.Contains("BASE.CPK")))
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
    public ScriptManager()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        this.ScriptList   = new Dictionary<string, List<string>>();
        this.ScriptTexts  = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        this.ScriptErrors = new Dictionary<string, Dictionary<string, string>>();

        foreach (string scriptType in new[] { "BMD", "BF" })
        {
            if (scriptType == "BMD")
                this.BMDFiles = new Dictionary<string, BMD>();
            // TODO: actually make BF Serializable :')
            //else
            //    this.BFFiles = new Dictionary<string, BF>();
            this.ScriptList[scriptType]   = new List<string>();
            this.ScriptTexts[scriptType]  = new Dictionary<string, Dictionary<string, string>>();
            this.ScriptErrors[scriptType] = new Dictionary<string, string>();
        }
    }

    public string RemovePrefix(string prefix, string s)
    {
        return s.Substring((prefix.Length+1), s.Length-(prefix.Length+1));
    }

    public string BasePath(string parentDir, string fileBase)
    {
        return Path.Combine(parentDir, Path.GetDirectoryName(fileBase), Path.GetFileNameWithoutExtension(fileBase));
    }

    public void SaveScript(string scriptType, string workingDir, string modDir, string emuDir)
    {
        foreach (string script in this.ScriptList[scriptType])
        {
            // for now, only saves the active script...
            // there's probably a better way to do this, but i think the UI needs to be a bit clearer
            // for now, this at least is better than the hardcoding there was before, but TODO
            if ((scriptType == "BMD" && script == this.ActiveBMD) || (scriptType == "BF" && script == this.ActiveBF))
            {
                if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(modDir, script))))
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(modDir, script)));
                if (emuDir is null)
                    File.Copy(Path.Combine(workingDir, script), Path.Combine(modDir, script), true);
                else
                {
                    File.Create(Path.Combine(modDir, script)).Dispose();
                    foreach (string ext in new[] { ".flow", ".msg" })
                        if (this.ScriptTexts[scriptType][script].ContainsKey(ext))
                        {
                            // if they already have full path femu, fine, just use that
                            if (File.Exists(this.BasePath(Path.Combine(emuDir, scriptType), script)+ext))
                                File.Copy(this.BasePath(workingDir, script)+ext, this.BasePath(Path.Combine(emuDir, scriptType), script)+ext, true);
                            // otherwise, do it the dummy + top-level (recommended) way
                            else
                            {
                                File.WriteAllText(this.BasePath(workingDir, script)+ext, this.ScriptTexts[scriptType][script][ext]);
                                File.Copy(this.BasePath(workingDir, script)+ext, Path.Combine(emuDir, scriptType, Path.GetFileNameWithoutExtension(script)+ext), true);
                            }
                        }
                }
            }
        }
    }

    public void PopulateWorkingDir(string workingDir, string baseDir, string modDir, string emuDir, List<string> bmdPaths, List<string> bfPaths, string gameType)
    {
        // TODO: this is dumb, fix this with the file management PR
        foreach (List<string> pathList in new[] { bmdPaths, bfPaths })
            if (!(pathList is null))
                //for (int i = 0; i < pathList.Count; i++)
                // oh god this is bad and stupid. bandaid solution to go with the sort + reverse thing for the files
                // otherwise, if the vanilla and mod cpk names are the same and emulator is off, it gets ugly.
                // TODO TODO TODO oh god TODO
                for (int i = pathList.Count-1; i >= 0; i--)
                {
                    string workingPath = null;
                    if (pathList[i].StartsWith(baseDir))
                        workingPath = Path.Combine(workingDir, this.RemovePrefix(baseDir, pathList[i]));
                    else if (pathList[i].StartsWith(modDir))
                        workingPath = Path.Combine(workingDir, this.RemovePrefix(modDir, pathList[i]));
                    // copy BFs/BMDs to working dir
                    if (!Directory.Exists(Path.GetDirectoryName(workingPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(workingPath));
                    File.Copy(pathList[i], workingPath, true);
                    // make sure those are the ref paths
                    pathList[i] = workingPath;
                }

        this.UpdateScripts(bmdPaths, bfPaths, workingDir, gameType);

        // THEN decompile them
        this.DecompileAll(workingDir);

        // and THEN copy modded decomp files over the fresh decomp'd ones (if emu is active)
        this.MaybeOverwriteScripts(workingDir, modDir, emuDir);
        this.RefreshScriptTexts(workingDir);

        // and then recompile with those to make sure we have the right binaries
        foreach (string fileBase in this.ScriptTexts["BMD"].Keys)
            this.CompileMessage(workingDir, fileBase);
    }

    public void UpdateScripts(List<string> bmdPaths, List<string> bfPaths, string modPath, string gameType)
    {
        // TODO: I gooootta handle game/locale/encoding more elegantly, lmao
        if (gameType.StartsWith("P5R"))
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
        this.Encoding = AtlusEncoding.GetByName(this.GameName);

        this.BMDFiles.Clear();
        //this.BFFiles.Clear();

        foreach (string scriptType in this.ScriptList.Keys)
        {
            this.ScriptList[scriptType].Clear();
            this.ScriptTexts[scriptType].Clear();
            this.ScriptErrors[scriptType].Clear();
            List<string> paths = (scriptType == "BMD") ? bmdPaths : bfPaths;
            if (!(paths is null))
                foreach (string scriptPath in paths)
                {
                    string key = this.RemovePrefix(modPath, scriptPath);
                    this.ScriptList[scriptType].Add(key);
                    if (scriptType == "BMD")
                    {
                        var messageFile = new BMD();
                        messageFile.Read(scriptPath);
                        this.BMDFiles[key] = messageFile;
                    }
                    // TODO: do same for BF once i make the class for it....
                }
        }

        if (this.ScriptList["BMD"].Count > 0)
            this.ActiveBMD = this.ScriptList["BMD"][0];
        else
            this.ActiveBMD = null;

        if (this.ScriptList["BF"].Count > 0)
            this.ActiveBF = this.ScriptList["BF"][0];
        else
            this.ActiveBF = null;
    }

    public void DecompileAll(string targetDir)
    {
        foreach (string script in this.ScriptList["BMD"])
            this.DecompileMessage(targetDir, script);
        foreach (string script in this.ScriptList["BF"])
            this.DecompileScript(targetDir, script);
    }

    public void MaybeOverwriteScripts(string workingDir, string modDir, string emuDir)
    {
        if (modDir is null || emuDir is null)
            return;
        foreach (string scriptType in this.ScriptTexts.Keys)
            foreach (string fileBase in this.ScriptTexts[scriptType].Keys)
                foreach (string fileExt in this.ScriptTexts[scriptType][fileBase].Keys)
                    // recommended format is dummy file in essentials + top-level femu
                    if (File.Exists(Path.Combine(modDir, fileBase)) && File.Exists(Path.Combine(emuDir, scriptType, Path.GetFileNameWithoutExtension(fileBase))+fileExt))
                        File.Copy(Path.Combine(emuDir, scriptType, Path.GetFileNameWithoutExtension(fileBase))+fileExt, this.BasePath(workingDir, fileBase)+fileExt, true);
                    // ...but full-path femu is also fine
                    else if (File.Exists(this.BasePath(Path.Combine(emuDir, scriptType), fileBase)+fileExt))
                        File.Copy(this.BasePath(Path.Combine(emuDir, scriptType), fileBase)+fileExt, this.BasePath(workingDir, fileBase)+fileExt, true);
    }

    public void RefreshScriptTexts(string targetDir)
    {
        foreach (string scriptType in this.ScriptTexts.Keys)
            foreach (string fileBase in this.ScriptTexts[scriptType].Keys)
                foreach (string fileExt in this.ScriptTexts[scriptType][fileBase].Keys)
                    this.ScriptTexts[scriptType][fileBase][fileExt] = File.ReadAllText(this.BasePath(targetDir, fileBase)+fileExt);
    }

    public void DecompileMessage(string targetDir, string fileBase)
    {
        AppLogListener listener = new AppLogListener();
        this.ScriptTexts["BMD"][fileBase] = new Dictionary<string, string>();
        this.ScriptErrors["BMD"][fileBase] = "";
        try
        {
            string outPath = this.BasePath(targetDir, fileBase);
            string baseExt = Path.GetExtension(fileBase);
            MessageScriptBinary binary = MessageScriptBinary.FromStream(this.BMDFiles[fileBase].ToStream());
            MessageScript msgScript = MessageScript.FromBinary(binary, FormatVersion.Detect, this.Encoding);
            using (var decompiler = new MessageScriptDecompiler(new FileTextWriter(outPath+".msg")))
            {
                decompiler.Library = LibraryLookup.GetLibrary(this.GameName);
                decompiler.Decompile(msgScript);
            }
            string[] fileNames = Directory.GetFiles(Path.GetDirectoryName(outPath));
            Array.Sort(fileNames);
            foreach (string fileName in fileNames)
                if (fileName != outPath+baseExt && fileName.StartsWith(outPath))
                    this.ScriptTexts["BMD"][fileBase][fileName.Substring(outPath.Length, fileName.Length-outPath.Length)] = File.ReadAllText(fileName);

            if (this.ScriptTexts["BMD"][fileBase].Count == 0)
                this.ScriptErrors["BMD"][fileBase] = "Decompilation seemed okay, but no files found....";
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            this.ScriptErrors["BMD"][fileBase] = ex.ToString();
        }
    }

    public string CompileMessage(string targetDir, string fileBase)
    {
        string outPath = this.BasePath(targetDir, fileBase);
        foreach (string ext in this.ScriptTexts["BMD"][fileBase].Keys)
            File.WriteAllText(outPath+ext, this.ScriptTexts["BMD"][fileBase][ext]);
                
        MessageScriptCompiler compiler = new MessageScriptCompiler(FormatVersion.Version1BigEndian, this.Encoding);
        compiler.Library = LibraryLookup.GetLibrary(this.GameName);
        AppLogListener listener = new AppLogListener();
        compiler.AddListener(listener);
        try
        {
            MessageScript msgScript = new MessageScript(FormatVersion.Version1BigEndian, this.Encoding);
            bool success = compiler.TryCompile(this.ScriptTexts["BMD"][fileBase][".msg"], out msgScript);
            this.BMDFiles[fileBase] = new BMD();
            byte[] newBytes = ((MemoryStream)msgScript.ToBinary().ToStream()).ToArray();
            this.BMDFiles[fileBase].FromBytes(newBytes);
            File.WriteAllBytes(outPath+".BMD", newBytes);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            listener.Text += ex.ToString();
        }
        return listener.Text;
    }

    public void DecompileScript(string targetDir, string fileBase)
    {
        AppLogListener listener = new AppLogListener();
        this.ScriptTexts["BF"][fileBase] = new Dictionary<string, string>();
        this.ScriptErrors["BF"][fileBase] = "";
        try
        {
            string outPath = this.BasePath(targetDir, fileBase);
            string baseExt = Path.GetExtension(fileBase);
            FlowScriptBinary binary = FlowScriptBinary.FromStream(new FileStream(outPath+baseExt, FileMode.Open));
            FlowScript flowScript = FlowScript.FromBinary(binary, this.Encoding);
            var decompiler = new FlowScriptDecompiler();
            decompiler.Library = LibraryLookup.GetLibrary(this.GameName);
            decompiler.AddListener(listener);
            bool success = decompiler.TryDecompile(flowScript, outPath+".flow");
            if (success)
            {
                string[] fileNames = Directory.GetFiles(Path.GetDirectoryName(outPath));
                Array.Sort(fileNames);
                foreach (string fileName in fileNames)
                    if (fileName != outPath+baseExt && fileName.StartsWith(outPath))
                        this.ScriptTexts["BF"][fileBase][fileName.Substring(outPath.Length, fileName.Length-outPath.Length)] = File.ReadAllText(fileName);
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            listener.Text += ex.ToString();
        }
        this.ScriptErrors["BF"][fileBase] = listener.Text;
    }

    public string CompileScript(string targetDir, string fileBase)
    {
        AppLogListener listener = new AppLogListener();
        string outPath = this.BasePath(targetDir, fileBase);
        foreach (string ext in this.ScriptTexts["BF"][fileBase].Keys)
            File.WriteAllText(outPath+ext, this.ScriptTexts["BF"][fileBase][ext]);

        string oldWorkingDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(Path.GetDirectoryName(outPath));

        try
        {
            // TODO: should detect the version from the vanilla extracted file...?
            // orrrr output version just depends on game type?
            FlowScriptCompiler compiler = new FlowScriptCompiler(FlowFormatVersion.Version1BigEndian);
            compiler.Encoding = this.Encoding;
            compiler.Library = LibraryLookup.GetLibrary(this.GameName);
            compiler.AddListener(listener);
            FlowScript flowScript = new FlowScript(FlowFormatVersion.Version1BigEndian);
            bool success = compiler.TryCompile(this.ScriptTexts["BF"][fileBase][".flow"], out flowScript);
            byte[] newBytes = ((MemoryStream)flowScript.ToBinary().ToStream()).ToArray();
            //this.BFFiles[fileBase] = new BF();
            //this.BFFiles[fileBase].FromBytes(newBytes);
            File.WriteAllBytes(outPath+".BF", newBytes);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            listener.Text += ex.ToString();
        }
        Directory.SetCurrentDirectory(oldWorkingDir);
        return listener.Text;
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
                    foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("BASE.CPK")))
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
                foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("BASE.CPK")))
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
                foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[turnIndex].Name, this.ActiveBMD.Contains("BASE.CPK")))
                    name += node.Text;
        return name;
    }

    public string GetTurnSpeakerName(int turnIndex)
    {
        string name = "";
        if (!(this.ActiveBMD is null))
            if (turnIndex >= 0 && turnIndex < this.BMDFiles[this.ActiveBMD].Turns.Length && this.BMDFiles[this.ActiveBMD].Turns[turnIndex].SpeakerId != 0xFFFF)
                foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Speakers[this.BMDFiles[this.ActiveBMD].Turns[turnIndex].SpeakerId], this.ActiveBMD.Contains("BASE.CPK")))
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
                foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[turnIndex].Elems[elemIndex], this.ActiveBMD.Contains("BASE.CPK")))
                    text += node.Text;
        return text;
    }

    public (string Source, uint CueId)? GetTurnVoice(int turnIndex, int elemIndex)
    {
        if (!(this.ActiveBMD is null))
            foreach (Node node in this.Parse(this.BMDFiles[this.ActiveBMD].Turns[turnIndex].Elems[elemIndex], this.ActiveBMD.Contains("BASE.CPK")))
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
