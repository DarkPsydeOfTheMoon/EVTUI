using CriFsV2Lib;
using CriFsV2Lib.Definitions;
using CriFsV2Lib.Definitions.Interfaces;
using CriFsV2Lib.Definitions.Structs;
using CriFsV2Lib.Definitions.Utilities;
using CriFsV2Lib.Utilities.Parsing;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
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
        if (!Directory.Exists(dirName))
            return;

        DirectoryInfo dirInfo = new DirectoryInfo(dirName);
        foreach (FileInfo file in dirInfo.EnumerateFiles())
        {
            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                Trace.TraceWarning($"Couldn't delete the file {file.ToString()}");
            }
        }
        foreach (DirectoryInfo dir in dirInfo.EnumerateDirectories())
        {
            try
            {
                dir.Delete(true);
            }
            catch (Exception)
            {
                Trace.TraceWarning($"Couldn't delete the directory {dir.ToString()}");
            }
        }
    }

    public static CpkFile[] ListAllFiles(string cpkPath, string decryptionFunctionName)
    {
        KnownDecryptionFunction decryptionFunctionIndex;
        InPlaceDecryptionFunction decryptionFunction = null;
        if (Enum.TryParse(decryptionFunctionName, out decryptionFunctionIndex))
            decryptionFunction = CriFsLib.Instance.GetKnownDecryptionFunction(decryptionFunctionIndex);

        using (var fileStream = new FileStream(cpkPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true, decryptionFunction))
                return reader.GetFiles();
    }

    public static async IAsyncEnumerable<string> FindModFiles(string[] prefix, string suffix, string ExistingFolder)
    {
        string delim = "/";
        if (Path.DirectorySeparatorChar == '\\')
            delim = "\\\\";
        string filePatternString = String.Join(delim, prefix);
        if (!(suffix is null))
            filePatternString += delim + suffix;
        filePatternString += "$";
        Regex filePattern = new Regex(filePatternString, RegexOptions.IgnoreCase);
        if (!String.IsNullOrEmpty(ExistingFolder))
            foreach (var ModPath in Directory.GetFiles(ExistingFolder, "*.*", SearchOption.AllDirectories))
            {
                if (filePattern.IsMatch(ModPath) && new FileInfo(ModPath).Length > 0)
                    yield return await Task.FromResult(ModPath);
            }
    }

    private static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
    public static async IAsyncEnumerable<string> ExtractFiles(List<CpkFile> files, string CpkPath, string OutputFolder, string decryptionFunctionName)
    {
        KnownDecryptionFunction decryptionFunctionIndex;
        InPlaceDecryptionFunction decryptionFunction = null;
        if (Enum.TryParse(decryptionFunctionName, out decryptionFunctionIndex))
            decryptionFunction = CriFsLib.Instance.GetKnownDecryptionFunction(decryptionFunctionIndex);

        using var extractor = CriFsLib.Instance.CreateBatchExtractor<ItemModel>(CpkPath, decryptionFunction);
        using (var fileStream = new FileStream(CpkPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            foreach (CpkFile file in files)
            {
                string dirPath = Path.GetFullPath(Path.Combine(OutputFolder, Path.GetFileName(CpkPath), file.Directory ?? ""));
                Directory.CreateDirectory(dirPath);
                string inCpkPath = Path.Combine(file.Directory ?? "", file.FileName);
                string outputPath = Path.GetFullPath(Path.Combine(OutputFolder, Path.GetFileName(CpkPath), inCpkPath));
                if (rwLock.TryEnterWriteLock(2000))
                {
                    try
                    {
                        if (!File.Exists(outputPath))
                        {
                            using (File.Create(outputPath)) {}
                            extractor.QueueItem(new ItemModel(outputPath, file));
                        }
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }
                else
                {
                    Trace.TraceError("Failed to acquire extraction write lock after 2s.");
                    throw new IOException("Failed to acquire ectraction write lock after 2s.");
                }
                yield return await Task.FromResult(outputPath);
            }
        extractor.WaitForCompletion();
        ArrayRental.Reset();
    }

    public static List<(int MajorId, int MinorId)> ListAllEvents(List<string> CpkList, string decryptionFunctionName)
    {
        Regex pattern = new Regex("^EVENT[\\\\/]E\\d\\d\\d[\\\\/]E\\d\\d\\d[\\\\/]E\\d\\d\\d_\\d\\d\\d\\.EVT$", RegexOptions.IgnoreCase);
        HashSet<(int MajorId, int MinorId)> events = new HashSet<(int MajorId, int MinorId)>();

        KnownDecryptionFunction decryptionFunctionIndex;
        InPlaceDecryptionFunction decryptionFunction = null;
        if (Enum.TryParse(decryptionFunctionName, out decryptionFunctionIndex))
            decryptionFunction = CriFsLib.Instance.GetKnownDecryptionFunction(decryptionFunctionIndex);

        Parallel.ForEach(CpkList, CpkPath =>
        {
            CpkFile[] files;
            using (var fileStream = new FileStream(CpkPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = CriFsLib.Instance.CreateCpkReader(fileStream, true, decryptionFunction))
            {
                files = reader.GetFiles();
            }

            Parallel.For(0, files.Length, x =>
            {
                var inCpkPath = Path.Combine(files[x].Directory ?? "", files[x].FileName);
                if (pattern.IsMatch(inCpkPath))
                {
                    lock (events)
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
}
