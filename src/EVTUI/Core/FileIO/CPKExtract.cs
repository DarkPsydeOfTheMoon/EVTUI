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

    public static List<string> ExtractMatchingFiles(List<string> CpkList, string filePattern, string OutputFolder)
    {
        List<string> matches = new List<string>();
        Parallel.ForEach(CpkList, CpkPath =>
        {
            CpkFile[] files;
            using (var fileStream = new FileStream(CpkPath, FileMode.Open))
            using (var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true, CriFsLib.Instance.GetKnownDecryptionFunction(KnownDecryptionFunction.P5R)))
                { files = reader.GetFiles(); }
            using var extractor = CriFsLib.Instance.CreateBatchExtractor<ItemModel>(CpkPath, CriFsLib.Instance.GetKnownDecryptionFunction(KnownDecryptionFunction.P5R));
            Parallel.For(0, files.Length, x =>
            {
                var inCpkPath = Path.Combine(files[x].Directory ?? "", files[x].FileName);
                var outputPath = Path.GetFullPath(Path.Combine(OutputFolder, Path.GetFileName(CpkPath), inCpkPath));
                if (Regex.IsMatch(inCpkPath, filePattern))
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

    public static CpkEVTContents? ExtractEVTFiles(List<string> CpkList, string eventId, string OutputFolder)
    {
        var retval = new CpkEVTContents();
        string eventPattern = $"[\\\\/]{eventId}([\\\\/\\.]|_SE)";
        bool evtFound = false;

        Parallel.ForEach(CpkList, CpkPath =>
        {
            Console.WriteLine(CpkPath);
            CpkFile[] files;
            using (var fileStream = new FileStream(CpkPath, FileMode.Open))
            using (var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true, CriFsLib.Instance.GetKnownDecryptionFunction(KnownDecryptionFunction.P5R)))
            {
                files = reader.GetFiles();
            }

            using var extractor = CriFsLib.Instance.CreateBatchExtractor<ItemModel>(CpkPath, CriFsLib.Instance.GetKnownDecryptionFunction(KnownDecryptionFunction.P5R));

            Parallel.For(0, files.Length, x =>
            {
                var inCpkPath = Path.Combine(files[x].Directory ?? "", files[x].FileName);
                var outputPath = Path.GetFullPath(Path.Combine(OutputFolder, Path.GetFileName(CpkPath), inCpkPath));
                if (Regex.IsMatch(inCpkPath, eventPattern))
                {
                    Console.WriteLine(inCpkPath);
                    if (Regex.IsMatch(inCpkPath, "\\.EVT$"))
                    {
                        evtFound = true;
                        retval.evtPath = outputPath;
                    }
                    else if (Regex.IsMatch(inCpkPath, "\\.ECS$"))
                        retval.ecsPath = outputPath;
                    else if (Regex.IsMatch(inCpkPath, "\\.ACB$"))
                        retval.acbPaths.Add(outputPath);
                    else if (Regex.IsMatch(inCpkPath, "\\.AWB$"))
                        retval.awbPaths.Add(outputPath);
                    else if (Regex.IsMatch(inCpkPath, "\\.BMD$"))
                        retval.bmdPaths.Add(outputPath);
                    else if (Regex.IsMatch(inCpkPath, "\\.BF$"))
                        retval.bfPaths.Add(outputPath);
                    else
                        return;
                }
                else if (Regex.IsMatch(inCpkPath, "VOICE_SINGLEWORD\\.ACB$"))
                    retval.acbPaths.Add(outputPath);
                else if (Regex.IsMatch(inCpkPath, "VOICE_SINGLEWORD\\.AWB$"))
                    retval.awbPaths.Add(outputPath);
                else if (Regex.IsMatch(inCpkPath, "SYSTEM\\.ACB$"))
                    retval.acbPaths.Add(outputPath);
                else if (Regex.IsMatch(inCpkPath, "SYSTEM\\.AWB$"))
                    retval.awbPaths.Add(outputPath);
                else if (Regex.IsMatch(inCpkPath, "BGM\\.ACB$"))
                    retval.acbPaths.Add(outputPath);
                else if (Regex.IsMatch(inCpkPath, "BGM\\.AWB$"))
                    retval.awbPaths.Add(outputPath);
                else
                    return;
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
