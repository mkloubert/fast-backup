// This file is part of sharp-hole distribution.
// Copyright (c) Marcel Joachim Kloubert (https://marcel.coffee/)
//
// sharp-hole is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, version 3.
//
// sharp-hole is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Threading.Tasks;

namespace MarcelJKloubert.FastBackup;

public static class Program
{
    private static readonly Logger l = new();

    public async static Task<int> Main(string[] args)
    {
        Console.WriteLine("FastCopy by Marcel J. Kloubert (https://marcel.coffee)");
        Console.WriteLine();

        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("[ERROR] Please define at least a target directory!");

                return 2;
            }

            DirectoryInfo sourceDir;
            DirectoryInfo targetDir;

            if (args.Length == 1)
            {
                // 1 argument means that we want to copy
                // from current working directory to
                // a specific destination

                sourceDir = new DirectoryInfo(Environment.CurrentDirectory);
                targetDir = new DirectoryInfo(args[0]);
            }
            else
            {
                // first argument: source directory
                sourceDir = new DirectoryInfo(args[0]);
                // second argument: target directory
                targetDir = new DirectoryInfo(args[1]);
            }

            if (!sourceDir.Exists)
            {
                Console.WriteLine("[ERROR] Directory '{0}' not found!", sourceDir.FullName);

                return 3;
            }

            var job = new CopyJob(l)
            {
                SourceDirectory = sourceDir,
                TargetDirectory = targetDir,
            };

            l.Info("Starting copy process from '{0}' to '{1}' ...", sourceDir.FullName, targetDir.FullName);

            var startTime = DateTime.UtcNow;
            await job.RunAsync();
            var endTime = DateTime.UtcNow;

            var duration = endTime - startTime;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("SUMMARY:");
            Console.WriteLine("=============");
            Console.WriteLine("Duration: {0}", duration);
            Console.WriteLine();
            Console.WriteLine("Handled directories: {0}", job.CopiedDirectories);
            Console.WriteLine("Copied files: {0} ({1})", job.CopiedFiles, job.CopiedBytes);
            Console.WriteLine();
            Console.WriteLine("Skipped files: {0} ({1})", job.SkippedFiles, job.SkippedBytes);
            Console.WriteLine();
            Console.WriteLine("Extra directories: {0}", job.ExtraDirectories);
            Console.WriteLine("Extra files: {0} ({1})", job.ExtraFiles, job.ExtraBytes);
            Console.WriteLine();
            Console.WriteLine("Failed directories: {0}", job.FailedDirectories);
            Console.WriteLine("Failed files: {0} ({1})", job.FailedFiles, job.FailedBytes);

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[UNCAUGHT EXCEPTION]: {0}", ex);

            return 1;
        }
    }
}
