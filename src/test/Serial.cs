using System.Diagnostics;
using System.IO;
using System.Linq;

using EVTUI;

namespace EVTUI.Test;

public class SerialTests
{

    public static void TestEVT()
    {
        string evtPath = Path.Combine("Assets", "E764_001.EVT");
        EVT evt = new EVT();
        evt.Read(evtPath);
        evt.Write(evtPath + ".COPY");
        EVT evtCopy = new EVT();
        evtCopy.Read(evtPath + ".COPY");
        Trace.Assert(File.ReadAllBytes(evtPath).SequenceEqual(File.ReadAllBytes(evtPath + ".COPY")), $"Reflexivity of read/write fails for {evtPath}");
        File.Delete(evtPath + ".COPY");
    }

    public static void TestBMD()
    {
        string bmdPath = Path.Combine("Assets", "E764_001.BMD");
        BMD bmd = new BMD();
        bmd.Read(bmdPath);
        // this won't actually be equal to the original, mostly, because of padding
        // will need to test later whether that actually matters for the game's reader
        bmd.Write(bmdPath + ".COPY");
        BMD bmdCopy = new BMD();
        bmdCopy.Read(bmdPath + ".COPY");
        bmdCopy.Write(bmdPath + ".COPY.COPY");
        BMD bmdCopyCopy = new BMD();
        bmdCopyCopy.Read(bmdPath + ".COPY.COPY");
        Trace.Assert(File.ReadAllBytes(bmdPath + ".COPY").SequenceEqual(File.ReadAllBytes(bmdPath + ".COPY.COPY")), $"Reflexivity of read/write fails for {bmdPath}");
        Trace.Assert(bmd.Speakers.Length == bmdCopyCopy.Speakers.Length, $"Original speakers ({bmd.Speakers.Length}) don't match rewritten speakers ({bmdCopyCopy.Speakers.Length})");
        for (int i=0; i<bmd.Speakers.Length; i++)
        {
            string speakerIn = System.Text.Encoding.Default.GetString(bmd.Speakers[i]);
            string speakerOut = System.Text.Encoding.Default.GetString(bmdCopyCopy.Speakers[i]);
            Trace.Assert(speakerIn == speakerOut, $"Original speaker ({speakerIn}) doesn't match rewritten speaker ({speakerOut})");
        }
        File.Delete(bmdPath + ".COPY");
        File.Delete(bmdPath + ".COPY.COPY");
    }

}
