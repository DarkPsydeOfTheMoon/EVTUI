using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using AtlusScriptLibrary.Common.Text.Encodings;

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

public class ScriptManager
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private static Dictionary<(byte, byte), string> EnCharLookup = CharLookup("P5R_EFIGS");
    private static Dictionary<(byte, byte), string> JpCharLookup = CharLookup("P5R_Japanese");

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public string?                 ActiveBMD = null;
    public Dictionary<string, BMD> MessageFiles { get; }
    public List<string>            BmdList      { get; }

    // TODO: this definitely belongs... somewhere else. probably in the actual AudioPreview ViewModel
    public AudioCues EventCues
    {
        get
        {
            AudioCues eventCues = new AudioCues();
            foreach (string key in this.MessageFiles.Keys)
            {
                bool isJP = key.Contains("BASE.CPK");
                LocaleCues locale;
                if (isJP)
                    locale = eventCues.JpCues;
                else
                    locale = eventCues.EnCues;
                foreach (Turn turn in this.MessageFiles[key].Turns)
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
                                    speakerName = this.Parse(this.MessageFiles[key].Speakers[turn.SpeakerId], isJP)[0].Text;
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
                            } catch {}
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
            foreach (Turn turn in this.MessageFiles[this.ActiveBMD].Turns)
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
            //foreach (Turn turn in this.MessageFiles[this.ActiveBMD].Turns)
            for (int i=0; i<this.MessageFiles[this.ActiveBMD].TurnCount; i++)
            {
                string name = "";
                //foreach (Node node in this.Parse(turn.Name, this.ActiveBMD.Contains("BASE.CPK")))
                foreach (Node node in this.Parse(this.MessageFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("BASE.CPK")))
                    name += node.Text;
                //if (Regex.IsMatch(name, "^M(SG|ND)_\\d\\d\\d_\\d_\\d$"))
                if (this.MessageFiles[this.ActiveBMD].TurnKinds[i] == 0)
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
            //foreach (Turn turn in this.MessageFiles[this.ActiveBMD].Turns)
            for (int i=0; i<this.MessageFiles[this.ActiveBMD].TurnCount; i++)
            {
                string name = "";
                //foreach (Node node in this.Parse(turn.Name, this.ActiveBMD.Contains("BASE.CPK")))
                foreach (Node node in this.Parse(this.MessageFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("BASE.CPK")))
                    name += node.Text;
                //if (Regex.IsMatch(name, "^SEL_\\d\\d\\d_\\d_\\d$"))
                if (this.MessageFiles[this.ActiveBMD].TurnKinds[i] != 0)
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
            foreach (byte[] speaker in this.MessageFiles[this.ActiveBMD].Speakers)
            {
                string name = "";
                foreach (Node node in this.Parse(speaker, this.ActiveBMD.Contains("BASE.CPK")))
                    name += node.Text;
                names.Add(name);
            }
            names.Add("(UNNAMED)");
            return names;
        }
    }

    public int GetTurnIndex(Int16 majorId, byte minorId, byte subId)
    {
        for (int i=0; i<this.MessageFiles[this.ActiveBMD].Turns.Length; i++)
        {
            string name = "";
            foreach (Node node in this.Parse(this.MessageFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("BASE.CPK")))
                name += node.Text;
            if (Regex.IsMatch(name, $"^[A-Z][A-Z][A-Z]_{majorId:000}_{minorId}_{subId}$"))
                return i;
        }
        return -1;
    }

    public int GetTurnIndex(string targetName)
    {
        for (int i=0; i<this.MessageFiles[this.ActiveBMD].Turns.Length; i++)
        {
            string name = "";
            foreach (Node node in this.Parse(this.MessageFiles[this.ActiveBMD].Turns[i].Name, this.ActiveBMD.Contains("BASE.CPK")))
                name += node.Text;
            if (name == targetName)
                return i;
        }
        return -1;
    }

    public string GetTurnName(int turnIndex)
    {
        string name = "";
        foreach (Node node in this.Parse(this.MessageFiles[this.ActiveBMD].Turns[turnIndex].Name, this.ActiveBMD.Contains("BASE.CPK")))
            name += node.Text;
        return name;
    }

    public string GetTurnSpeakerName(int turnIndex)
    {
        string name = "";
        if (this.MessageFiles[this.ActiveBMD].Turns[turnIndex].SpeakerId != 0xFFFF)
            foreach (Node node in this.Parse(this.MessageFiles[this.ActiveBMD].Speakers[this.MessageFiles[this.ActiveBMD].Turns[turnIndex].SpeakerId], this.ActiveBMD.Contains("BASE.CPK")))
                name += node.Text;
        return name;
    }

    public string GetTurnText(int turnIndex)
    {
        string text = "";
        foreach (byte[] elem in this.MessageFiles[this.ActiveBMD].Turns[turnIndex].Elems)
            foreach (Node node in this.Parse(elem, this.ActiveBMD.Contains("BASE.CPK")))
                text += node.Text;
        return text;
    }

    public List<(string Source, uint CueId)?> GetTurnVoices(int turnIndex)
    {
        List<(string Source, uint CueId)?> ret = new List<(string Source, uint CueId)?>();
        foreach (byte[] elem in this.MessageFiles[this.ActiveBMD].Turns[turnIndex].Elems)
            foreach (Node node in this.Parse(elem, this.ActiveBMD.Contains("BASE.CPK")))
                if (node.FunctionTableIndex == 3 && node.FunctionIndex == 1 && node.FunctionArguments[3] != 0)
                    switch (node.FunctionArguments[1])
                    {
                        case 0:
                            if (this.GetTurnSpeakerName(turnIndex).StartsWith("(SE)"))
                                ret.Add(("SFX", node.FunctionArguments[3]));
                            else
                                ret.Add(("Voice", node.FunctionArguments[3]));
                            break;
                        case 1:
                            ret.Add(("Field", node.FunctionArguments[3]));
                            break;
                        case 2:
                            ret.Add(("Common", node.FunctionArguments[3]));
                            break;
                        default:
                            ret.Add(null);
                            break;
                    }
        return ret;
    }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ScriptManager()
    {
        MessageFiles = new Dictionary<string, BMD>();
        BmdList      = new List<string>();
    }

    public void UpdateMessages(List<string> bmdPaths, string modPath)
    {
        this.MessageFiles.Clear();
        this.BmdList.Clear();
        foreach (var bmdPath in bmdPaths)
        {
            var messageFile = new BMD();
            messageFile.Read(bmdPath);
            string key = bmdPath.Substring((modPath.Length+1), bmdPath.Length-(modPath.Length+1));
            this.MessageFiles[key] = messageFile;
            this.BmdList.Add(key);
        }
        if (bmdPaths.Count > 0)
            this.ActiveBMD = this.BmdList[0];
        else
            this.ActiveBMD = null;
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
                nodes.Add(new Node(textbuff, isJP));
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

        public Node(Queue<byte> unencoded, bool isJP)
        {
            Text = Encode(unencoded, isJP);
        }

        public int FunctionId;
        public int FunctionTableIndex;
        public int FunctionIndex;
        public int FunctionArgumentCount;
        public List<ushort> FunctionArguments;

        public string Text;
    }

    private static string Encode(Queue<byte> unencoded, bool isJP)
    {
        string encoded = "";
        while (unencoded.Count > 0)
        {
            byte high = unencoded.Dequeue();
            if ((high & 0x80) == 0x80)
                try
                {
                    if (isJP)
                        encoded += JpCharLookup[(high, unencoded.Dequeue())];
                    else
                        encoded += EnCharLookup[(high, unencoded.Dequeue())];
                }
                catch (KeyNotFoundException)
                {
                    encoded += "*";
                }
            else if (high != 0)
                encoded += (char)high;
        }
        return encoded;
    }

}
