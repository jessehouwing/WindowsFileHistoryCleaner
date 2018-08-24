using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ManyConsole;
using Microsoft.VisualBasic.FileIO;

namespace FileHistoryCleaner
{
    public class RemoveHistoryCommand : ConsoleCommand
    {
        public RemoveHistoryCommand()
        {
            IsCommand("remove-history");
            HasRequiredOption<string>("root=", "Root directory to process", root => Root = root);
            HasOption("whatif", "Doesn't actually delete any files.", _ => WhatIf = true);
            HasOption("f|force", "Deletes files instead of moving them to the recycle bin.", _ => Force = true);
            HasOption("r|recursive", "Recursive.", _ => Recursive = true);
            HasOption("verbose", "Verbose logging.", _ => Verbose = true);
            HasOption<string>("rd|remove-directories=", "", _ => StripDirectories = new Regex(_, RegexOptions.IgnoreCase));
            HasOption<string>("rf|remove-files=", "", _ => StripFiles = new Regex(_, RegexOptions.IgnoreCase));
        }

        public Regex StripDirectories 
        {
            get;
            set;
        }

        public Regex StripFiles
        {
            get;
            set;
        }

        public bool Recursive { get; set; }

        public bool Force { get; set; }

        public bool Verbose { get; set; }

        public bool WhatIf { get; set; }

        public string Root { get; set; }

        public override int Run(string[] remainingArguments)
        {
            CleanDirectory(Root);

            return 0;
        }

        private void CleanDirectory(string path)
        {
            CleanFilesInDirectory(path);
            if (Recursive)
            {
                CleanSubDirectories(path);
            }
        }

        private void CleanFilesInDirectory(string path)
        {
            if (Verbose)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("======================================================================");
                Console.WriteLine("Dir: " + path);
            }

            var files = Directory.EnumerateFiles(path);

            var groupedFiles = from file in files
                let fileSnapshot = new FileSnapshot(file)
                orderby fileSnapshot.TargetFilename, fileSnapshot.Timestamp descending 
                group fileSnapshot by (fileSnapshot.TargetFilename);

            var result = from g in groupedFiles
                select StripFiles.IsMatch(g.Key)
                    ? new { keep = (FileSnapshot)null, delete = g.AsEnumerable() }
                    : new { keep = g.First(), delete = g.Skip(1) };

            foreach (var r in result)
            {
                if (Verbose)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("File: " + (r.keep ?? r.delete.First())?.TargetFilename);
                }

                if (r.keep != null && r.keep.Path != r.keep.TargetFilename)
                {
                    if (Verbose) { Console.WriteLine("Keep: " + r.keep.Path); }
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Rename: " + r.keep.Path + " => " + r.keep.TargetFilename);
                    if (!WhatIf)
                    {
                        File.Move(r.keep.Path, r.keep.TargetFilename);
                    }
                }
                
                foreach (var drop in r.delete)
                {
                    if (Force)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Delete: " + drop.Path);
                        if (!WhatIf)
                        {
                            File.Delete(drop.Path);
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("Recycle: " + drop.Path);
                        if (!WhatIf)
                        {
                            FileSystem.DeleteFile(drop.Path, UIOption.OnlyErrorDialogs,
                                RecycleOption.SendToRecycleBin);
                        }
                    }
                }
            }
        }

        internal class FileSnapshot
        {
            internal FileSnapshot(string filename)
            {
                Path = filename;
                string datePart = Regex.Match(filename, @"(?<=(^| )\()\d{4}_\d{2}_\d{2} \d{2}_\d{2}_\d{2} \w+(?=\)(\.|$))", RegexOptions.Multiline).Value;

                if (!string.IsNullOrWhiteSpace(datePart))
                {
                    TargetFilename = filename.Replace(" (" + datePart + ")", "");
                    Timestamp = DateTimeOffset.ParseExact(datePart.Replace(" UTC", "Z"), "yyyy_MM_dd HH_mm_ssZ", CultureInfo.InvariantCulture);
                }
                else
                {
                    TargetFilename = filename;
                    Timestamp = DateTimeOffset.UtcNow;
                }
            }

            internal string Path;
            internal string TargetFilename;
            internal DateTimeOffset Timestamp;
        }

        private void CleanSubDirectories(string path)
        {
            foreach (var sub in Directory.EnumerateDirectories(path))
            {
                if (StripDirectories.IsMatch(sub))
                {
                    DeleteDirectory(sub);
                }
                else
                {
                    CleanDirectory(sub);
                }
            }
        }

        private void DeleteDirectory(string sub)
        {
            if (Force)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Delete: " + sub);
                if (!WhatIf)
                {
                    Directory.Delete(sub, true);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Recycle: " + sub);
                if (!WhatIf)
                {
                    FileSystem.DeleteDirectory(sub, UIOption.OnlyErrorDialogs,
                        RecycleOption.SendToRecycleBin);
                }
            }
        }
    }
}
