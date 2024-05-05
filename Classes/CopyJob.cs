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
using System.Linq;
using System.Threading.Tasks;

namespace MarcelJKloubert.FastBackup;

/// <summary>
/// A copy job.
/// </summary>
public class CopyJob
{
    #region Fields

    /// <summary>
    /// The number of successfully copied bytes.
    /// </summary>
    public ulong CopiedBytes = 0;

    /// <summary>
    /// The number of successfully finished directories.
    /// </summary>
    public ulong CopiedDirectories = 0;

    /// <summary>
    /// The number of successfully copied files.
    /// </summary>
    public ulong CopiedFiles = 0;

    /// <summary>
    /// The number of successfully removed extra bytes.
    /// </summary>
    public ulong ExtraBytes = 0;

    /// <summary>
    /// The number of successfully removed extra directories.
    /// </summary>
    public ulong ExtraDirectories = 0;

    /// <summary>
    /// The number of successfully removed extra files.
    /// </summary>
    public ulong ExtraFiles = 0;

    /// <summary>
    /// The number of failed bytes, that could not be copied.
    /// </summary>
    public ulong FailedBytes = 0;

    /// <summary>
    /// The number of failed directories, that could not be copied.
    /// </summary>
    public ulong FailedDirectories = 0;

    /// <summary>
    /// The number of failed extra bytes, that could not be deleted.
    /// </summary>
    public ulong FailedExtraBytes = 0;

    /// <summary>
    /// The number of failed extra directories, that could not be deleted.
    /// </summary>
    public ulong FailedExtraDirectories = 0;

    /// <summary>
    /// The number of failed extra files, that could not be deleted.
    /// </summary>
    public ulong FailedExtraFiles = 0;

    /// <summary>
    /// The number of failed files, that could not be copied.
    /// </summary>
    public ulong FailedFiles = 0;

    private readonly Logger l;

    /// <summary>
    /// The number of skipped bytes.
    /// </summary>
    public ulong SkippedBytes = 0;

    /// <summary>
    /// The number of skipped files.
    /// </summary>
    public ulong SkippedFiles = 0;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of that class.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    public CopyJob(Logger logger)
    {
        if (logger == null)
        {
            ArgumentNullException.ThrowIfNull(logger);
        }

        l = logger;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the initial source directory.
    /// </summary>
    public DirectoryInfo SourceDirectory { get; set; }

    /// <summary>
    /// Gets or sets the initial target directory.
    /// </summary>
    public DirectoryInfo TargetDirectory { get; set; }

    #endregion

    #region Methods

    private async Task CopyDirAsync(DirectoryInfo src, DirectoryInfo dest)
    {
        l.Info("Entering '{0}' ...", src.FullName);

        src = new DirectoryInfo(src.FullName);
        if (!src.Exists)
        {
            l.Warn("\tDirectory '{0}' does not exist (anymore)!", src.FullName);

            return;
        }

        dest = new DirectoryInfo(dest.FullName);
        if (!dest.Exists)
        {
            dest.Create();
            dest.Refresh();

            l.Info("\tDirectory '{0}' created", dest.FullName);
        }

        try
        {
            // handle EXTRA dirs
            var maybeExtraDirs = dest.EnumerateDirectories()
                .OrderBy((dir) =>
                {
                    return dir.Name.ToLower();
                });
            foreach (var destinationDir in maybeExtraDirs)
            {
                try
                {
                    var sourceDirPath = Path.Combine(src.FullName, destinationDir.Name);
                    if (Directory.Exists(sourceDirPath))
                    {
                        continue;
                    }

                    destinationDir.Delete(true);
                    destinationDir.Refresh();

                    ++ExtraDirectories;
                }
                catch (Exception ex)
                {
                    l.Warn("\tCould not delete extra directory '{0}': {1}", destinationDir.FullName, ex);

                    ++FailedExtraDirectories;
                }
            }
            maybeExtraDirs = null;

            // handle EXTRA files
            var maybeExtraFiles = dest.EnumerateFiles()
                .OrderByDescending((file) =>
                {
                    return file.Length;
                })
                .ThenBy((file) =>
                {
                    return file.Name.ToLower();
                });
            foreach (var destinationFile in maybeExtraFiles)
            {
                var destinationFileLength = (ulong)destinationFile.Length;

                try
                {
                    var sourceFilePath = Path.Combine(src.FullName, destinationFile.Name);
                    if (File.Exists(sourceFilePath))
                    {
                        continue;
                    }

                    destinationFile.Delete();
                    destinationFile.Refresh();

                    l.Info("\tRemoved extra file '{0}' ({1})", destinationFile.FullName, destinationFileLength);

                    ExtraBytes += destinationFileLength;
                    ++ExtraFiles;
                }
                catch (Exception ex)
                {
                    FailedExtraBytes += destinationFileLength;
                    ++FailedExtraFiles;

                    l.Warn("\tCould not delete extra file '{0}': {1}", destinationFile.FullName, ex);
                }
            }
            maybeExtraFiles = null;

            // copy files
            var filesToCopy = src.EnumerateFiles()
                .OrderBy((file) =>
                {
                    return file.Length;
                })
                .ThenBy((file) =>
                {
                    return file.Name.ToLower();
                });
            foreach (var sourceFile in filesToCopy)
            {
                try
                {
                    var destinationFile = new FileInfo(Path.Combine(dest.FullName, sourceFile.Name));

                    var shouldCopyFile = true;
                    if (
                        destinationFile.Exists &&
                        destinationFile.Length == sourceFile.Length &&
                        destinationFile.LastWriteTimeUtc.NormalizeToSeconds() == sourceFile.LastWriteTimeUtc.NormalizeToSeconds()
                    )
                    {
                        // for performance reasons it is enough
                        // to check if
                        // - file exists
                        // - has the same length
                        // - has the same LastWriteTimeUtc (normalized to seconds)
                        shouldCopyFile = false;
                    }

                    if (shouldCopyFile)
                    {
                        l.Info("Copying file to '{0}' ...", destinationFile.FullName);

                        var newFile = sourceFile.CopyTo(destinationFile.FullName, true);

                        ++CopiedFiles;
                        CopiedBytes += (ulong)newFile.Length;
                    }
                    else
                    {
                        ++SkippedFiles;
                        SkippedBytes += (ulong)sourceFile.Length;
                    }
                }
                catch (Exception ex)
                {
                    l.Err("Could not copy file '{0}': {1}", sourceFile.FullName, ex);

                    ++FailedFiles;
                    FailedBytes += (ulong)sourceFile.Length;
                }
            }
            filesToCopy = null;

            // copy sub directories
            var subDirs = src.EnumerateDirectories()
                .OrderBy((dir) =>
                {
                    return dir.Name.ToLower();
                });
            foreach (var subDir in subDirs)
            {
                try
                {
                    var subDestDir = new DirectoryInfo
                    (
                        Path.Combine(dest.FullName, subDir.Name)
                    );

                    await CopyDirAsync(subDir, subDestDir);
                }
                catch (Exception ex)
                {
                    l.Err("Could not copy directory '{0}': {1}", subDir.FullName, ex);
                }
            }
            subDirs = null;

            ++CopiedDirectories;
        }
        catch
        {
            ++FailedDirectories;

            throw;
        }
    }

    /// <summary>
    /// Runs the jobs asynchronously.
    /// </summary>
    public async Task RunAsync()
    {
        await CopyDirAsync(SourceDirectory, TargetDirectory);
    }

    #endregion
}
