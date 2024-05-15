using CriFsV2Lib;
using CriFsV2Lib.Definitions;
using CriFsV2Lib.Definitions.Interfaces;
using CriFsV2Lib.Definitions.Structs;
using CriFsV2Lib.Definitions.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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

    public static CpkEVTContents? ExtractEVTFiles(List<string> CpkList, string eventId, string OutputFolder)
    {
        var retval = new CpkEVTContents();
        string eventPattern = $"/{eventId}([/\\.]|_SE)";
        bool evtFound = false;

        foreach (var CpkPath in CpkList) 
        {
            Console.WriteLine(CpkPath);
            CpkFile[] files;
            using (var fileStream = new FileStream(CpkPath, FileMode.Open))
            using (var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true, CriFsLib.Instance.GetKnownDecryptionFunction(KnownDecryptionFunction.P5R)))
            {
                files = reader.GetFiles();
            }

            using var extractor = CriFsLib.Instance.CreateBatchExtractor<ItemModel>(CpkPath, CriFsLib.Instance.GetKnownDecryptionFunction(KnownDecryptionFunction.P5R));

            for (int x = 0; x < files.Length; x++)
            {
                var inCpkPath = Path.Combine(files[x].Directory ?? "", files[x].FileName);
                var outputPath = Path.Combine(OutputFolder, Path.GetFileName(CpkPath), inCpkPath);
                if (Regex.IsMatch(inCpkPath, eventPattern))
                {
                    Console.WriteLine(inCpkPath);
                    if (Regex.IsMatch(inCpkPath, "\\.EVT$"))
                    {
                        evtFound = true;
                        retval.evtPath = outputPath;
                        extractor.QueueItem(new ItemModel(outputPath, files[x]));
                    }
                    else if (Regex.IsMatch(inCpkPath, "\\.ACB$"))
                    {
                        retval.sounds.Add(outputPath);
                        extractor.QueueItem(new ItemModel(outputPath, files[x]));
                    }
                    else if (Regex.IsMatch(inCpkPath, "\\.AWB$"))
                    {
                        extractor.QueueItem(new ItemModel(outputPath, files[x]));
                    }
                    else if (Regex.IsMatch(inCpkPath, "\\.BMD$"))
                    {
                        retval.messages.Add(outputPath);
                        extractor.QueueItem(new ItemModel(outputPath, files[x]));
                    }
                    else if (Regex.IsMatch(inCpkPath, "\\.BF$"))
                    {
                        retval.scripts.Add(outputPath);
                        extractor.QueueItem(new ItemModel(outputPath, files[x]));
                    }
                }
            }
            extractor.WaitForCompletion();
            ArrayRental.Reset();
        }
        
        if (!evtFound)
            return null;

        return retval;
    }
}
