using CriFsV2Lib;
using CriFsV2Lib.Definitions;
using CriFsV2Lib.Definitions.Interfaces;
using CriFsV2Lib.Definitions.Structs;
using CriFsV2Lib.Definitions.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EVTUI;

public static class CPKExtract
{
    public class ItemModel : IBatchFileExtractorItem
    {
        public string FullPath { get; }
        public CpkFile File { get; }
        
        public ItemModel(string fullPath, CpkFile file)
        {
            FullPath = fullPath;
            File = file;
        }
    }

    public static void ClearDirectory(string dirName)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(dirName);
        foreach (FileInfo file in dirInfo.EnumerateFiles())
            file.Delete(); 
        foreach (DirectoryInfo dir in dirInfo.EnumerateDirectories())
            dir.Delete(true); 
    }

    public static List<string> ExtractMatchingFiles(List<string> CpkList, string filePatternString, string OutputFolder, string decryptionFunctionName)
    {
        Regex filePattern = new Regex(filePatternString, RegexOptions.IgnoreCase);
        List<string> matches = new List<string>();

        KnownDecryptionFunction decryptionFunctionIndex;
        InPlaceDecryptionFunction decryptionFunction = null;
        if (Enum.TryParse(decryptionFunctionName, out decryptionFunctionIndex))
            decryptionFunction = CriFsLib.Instance.GetKnownDecryptionFunction(decryptionFunctionIndex);

        Parallel.ForEach(CpkList, CpkPath =>
        {
            CpkFile[] files;
            using (var fileStream = new FileStream(CpkPath, FileMode.Open))
            using (var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true, decryptionFunction))
                { files = reader.GetFiles(); }
            using var extractor = CriFsLib.Instance.CreateBatchExtractor<ItemModel>(CpkPath, decryptionFunction);
            Parallel.For(0, files.Length, x =>
            {
                var inCpkPath = Path.Combine(files[x].Directory ?? "", files[x].FileName);
                var outputPath = Path.GetFullPath(Path.Combine(OutputFolder, Path.GetFileName(CpkPath), inCpkPath));
                if (filePattern.IsMatch(inCpkPath))
                {
                    matches.Add(outputPath);
                    extractor.QueueItem(new ItemModel(outputPath, files[x]));
                }
            });
            extractor.WaitForCompletion();
            ArrayRental.Reset();
        });

        return matches;
    }

    private static Dictionary<string, Regex> Patterns = new Dictionary<string, Regex>()
    {
        ["EVT:EVT"] = new Regex("\\.EVT$",                 RegexOptions.IgnoreCase),
        ["EVT:ECS"] = new Regex("\\.ECS$",                 RegexOptions.IgnoreCase),
        ["EVT:ACB"] = new Regex("\\.ACB$",                 RegexOptions.IgnoreCase),
        ["EVT:AWB"] = new Regex("\\.AWB$",                 RegexOptions.IgnoreCase),
        ["EVT:BMD"] = new Regex("\\.BMD$",                 RegexOptions.IgnoreCase),
        ["EVT:BF"]  = new Regex("\\.BF$",                  RegexOptions.IgnoreCase),
        ["VSW:ACB"] = new Regex("VOICE_SINGLEWORD\\.ACB$", RegexOptions.IgnoreCase),
        ["VSW:AWB"] = new Regex("VOICE_SINGLEWORD\\.AWB$", RegexOptions.IgnoreCase),
        ["SYS:ACB"] = new Regex("SYSTEM\\.ACB$",           RegexOptions.IgnoreCase),
        ["SYS:AWB"] = new Regex("SYSTEM\\.AWB$",           RegexOptions.IgnoreCase),
        ["BGM:ACB"] = new Regex("BGM\\.ACB$",              RegexOptions.IgnoreCase),
        ["BGM:AWB"] = new Regex("BGM\\.AWB$",              RegexOptions.IgnoreCase),
    };

    public static List<(int MajorId, int MinorId)> ListAllEvents(List<string> CpkList, string decryptionFunctionName)
    {
        Regex pattern = new Regex("^EVENT[\\\\/]E\\d\\d\\d[\\\\/]E\\d\\d\\d[\\\\/]E\\d\\d\\d_\\d\\d\\d\\.EVT$", RegexOptions.IgnoreCase);
        HashSet<(int MajorId, int MinorId)> events = new HashSet<(int MajorId, int MinorId)>();

        KnownDecryptionFunction decryptionFunctionIndex;
        InPlaceDecryptionFunction decryptionFunction = null;
        if (Enum.TryParse(decryptionFunctionName, out decryptionFunctionIndex))
            decryptionFunction = CriFsLib.Instance.GetKnownDecryptionFunction(decryptionFunctionIndex);

        object _lock = new();
        Parallel.ForEach(CpkList, CpkPath =>
        {
            CpkFile[] files;
            using (var fileStream = new FileStream(CpkPath, FileMode.Open))
            using (var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true, decryptionFunction))
            {
                files = reader.GetFiles();
            }

            Parallel.For(0, files.Length, x =>
            {
                var inCpkPath = Path.Combine(files[x].Directory ?? "", files[x].FileName);
                if (pattern.IsMatch(inCpkPath))
                {
                    lock (_lock)
                    {
                        events.Add((int.Parse(files[x].FileName.Substring(1, 3)), int.Parse(files[x].FileName.Substring(5, 3))));
                    }
                }
            });
        });

        List<(int MajorId, int MinorId)> ret = new List<(int MajorId, int MinorId)>(events);
        ret.Sort();
        return ret;
    }

    public static CpkEVTContents? ExtractEVTFiles(List<string> CpkList, string eventId, string OutputFolder, string existingFolder, string decryptionFunctionName)
    {
        // clear it first!
        CPKExtract.ClearDirectory(OutputFolder);

        var retval = new CpkEVTContents();
        Regex eventPattern = new Regex($"[\\\\/]{eventId}([\\\\/\\.]|_SE)", RegexOptions.IgnoreCase);
        bool evtFound = false;

        KnownDecryptionFunction decryptionFunctionIndex;
        InPlaceDecryptionFunction decryptionFunction = null;
        if (Enum.TryParse(decryptionFunctionName, out decryptionFunctionIndex))
            decryptionFunction = CriFsLib.Instance.GetKnownDecryptionFunction(decryptionFunctionIndex);

        Parallel.ForEach(CpkList, CpkPath =>
        {
            Console.WriteLine(CpkPath);
            CpkFile[] files;
            using (var fileStream = new FileStream(CpkPath, FileMode.Open))
            using (var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true, decryptionFunction))
            {
                files = reader.GetFiles();
            }

            using var extractor = CriFsLib.Instance.CreateBatchExtractor<ItemModel>(CpkPath, decryptionFunction);

            Parallel.For(0, files.Length, x =>
            {
                string inCpkPath = Path.Combine(files[x].Directory ?? "", files[x].FileName);
                string outputPath = Path.GetFullPath(Path.Combine(OutputFolder, Path.GetFileName(CpkPath), inCpkPath));

                string maybeExistingPath = null;
                if (!(existingFolder is null))
                {
                    maybeExistingPath = Path.Combine(existingFolder, Path.GetFileName(CpkPath), inCpkPath);
                    if (File.Exists(maybeExistingPath))
                        outputPath = maybeExistingPath;
                }

                if (eventPattern.IsMatch(inCpkPath))
                {
                    if (CPKExtract.Patterns["EVT:EVT"].IsMatch(inCpkPath))
                    {
                        evtFound = true;
                        retval.evtPath = outputPath;
                    }
                    else if (CPKExtract.Patterns["EVT:ECS"].IsMatch(inCpkPath))
                        retval.ecsPath = outputPath;
                    else if (CPKExtract.Patterns["EVT:ACB"].IsMatch(inCpkPath))
                        retval.acbPaths.Add(outputPath);
                    else if (CPKExtract.Patterns["EVT:AWB"].IsMatch(inCpkPath))
                        retval.awbPaths.Add(outputPath);
                    else if (CPKExtract.Patterns["EVT:BMD"].IsMatch(inCpkPath))
                        retval.bmdPaths.Add(outputPath);
                    else if (CPKExtract.Patterns["EVT:BF"].IsMatch(inCpkPath))
                        retval.bfPaths.Add(outputPath);
                    else
                        return;
                }
                else if (CPKExtract.Patterns["VSW:ACB"].IsMatch(inCpkPath))
                    retval.acbPaths.Add(outputPath);
                else if (CPKExtract.Patterns["VSW:AWB"].IsMatch(inCpkPath))
                    retval.awbPaths.Add(outputPath);
                else if (CPKExtract.Patterns["SYS:ACB"].IsMatch(inCpkPath))
                    retval.acbPaths.Add(outputPath);
                else if (CPKExtract.Patterns["SYS:AWB"].IsMatch(inCpkPath))
                    retval.awbPaths.Add(outputPath);
                else if (CPKExtract.Patterns["BGM:ACB"].IsMatch(inCpkPath))
                    retval.acbPaths.Add(outputPath);
                else if (CPKExtract.Patterns["BGM:AWB"].IsMatch(inCpkPath))
                    retval.awbPaths.Add(outputPath);
                else
                    return;
                Console.WriteLine(outputPath);
                if (outputPath != maybeExistingPath)
                    extractor.QueueItem(new ItemModel(outputPath, files[x]));
            });
            extractor.WaitForCompletion();
            ArrayRental.Reset();
        });
        
        if (!evtFound)
            return null;

        return retval;
    }
}
