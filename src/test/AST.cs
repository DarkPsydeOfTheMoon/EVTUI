using System.Diagnostics;
using System.IO;

using AtlusScriptLibrary.Common.Text;
using AtlusScriptLibrary.Common.Text.Encodings;
using AtlusScriptLibrary.Common.Libraries;

using AtlusScriptLibrary.FlowScriptLanguage;
using AtlusScriptLibrary.FlowScriptLanguage.BinaryModel;
using AtlusScriptLibrary.FlowScriptLanguage.Compiler;
using AtlusScriptLibrary.FlowScriptLanguage.Decompiler;
using FlowFormatVersion = AtlusScriptLibrary.FlowScriptLanguage.FormatVersion;

using AtlusScriptLibrary.MessageScriptLanguage;
using AtlusScriptLibrary.MessageScriptLanguage.BinaryModel;
using AtlusScriptLibrary.MessageScriptLanguage.Compiler;
using AtlusScriptLibrary.MessageScriptLanguage.Decompiler;
using MsgFormatVersion = AtlusScriptLibrary.MessageScriptLanguage.FormatVersion;

namespace EVTUI.Test;

public class ASTTests
{

    public static void TestDecompileBF()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        string oldWorkingDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory("Assets");
        string bfPath = "E767_001.BF";
        FlowScriptBinary binary = FlowScriptBinary.FromStream(new FileStream(bfPath, FileMode.Open));
        FlowScript flowScript = FlowScript.FromBinary(binary, AtlusEncoding.GetByName("p5r"));
        var decompiler = new FlowScriptDecompiler();
        decompiler.Library = LibraryLookup.GetLibrary("p5r");
        Trace.Assert(decompiler.TryDecompile(flowScript, bfPath+".flow"), $"Failed to decompile {bfPath}");
        Directory.SetCurrentDirectory(oldWorkingDir);
    }

    public static void TestCompileFlow()
    {
        string oldWorkingDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory("Assets");
        string flowPath = "E767_001.BF.flow";
        string flowText = File.ReadAllText(flowPath);
        FlowScriptCompiler compiler = new FlowScriptCompiler(FlowFormatVersion.Unknown);
        compiler.Encoding = AtlusEncoding.GetByName("p5r");
        compiler.Library = LibraryLookup.GetLibrary("p5r");
        FlowScript flowScript = new FlowScript(FlowFormatVersion.Unknown);
        Trace.Assert(compiler.TryCompile(flowText, out flowScript), $"Failed to compile {flowPath}");
        Directory.SetCurrentDirectory(oldWorkingDir);
    }

    public static void TestDecompileBMD()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        string oldWorkingDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory("Assets");
        string bmdPath = "E764_001.BMD";
        MessageScriptBinary binary = MessageScriptBinary.FromStream(new FileStream(bmdPath, FileMode.Open));
        MessageScript msgScript = MessageScript.FromBinary(binary, MsgFormatVersion.Detect, AtlusEncoding.GetByName("p5r"));
        var decompiler = new MessageScriptDecompiler(new FileTextWriter(bmdPath+".msg"));
        decompiler.Library = LibraryLookup.GetLibrary("p5r");
        // do or do not... there is no TryDecompile
        decompiler.Decompile(msgScript);
        Directory.SetCurrentDirectory(oldWorkingDir);
    }

    public static void TestCompileMsg()
    {
        string oldWorkingDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory("Assets");
        string msgPath = "E764_001.BMD.msg";
        string msgText = File.ReadAllText(msgPath);
        MessageScriptCompiler compiler = new MessageScriptCompiler(MsgFormatVersion.Version1BigEndian, AtlusEncoding.GetByName("p5r"));
        compiler.Library = LibraryLookup.GetLibrary("p5r");
        MessageScript msgScript = new MessageScript(MsgFormatVersion.Version1BigEndian, AtlusEncoding.GetByName("p5r"));
        Trace.Assert(compiler.TryCompile(msgText, out msgScript), $"Failed to compile {msgPath}");
        Directory.SetCurrentDirectory(oldWorkingDir);
    }

}
