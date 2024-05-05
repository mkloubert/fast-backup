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

namespace MarcelJKloubert.FastBackup;

/// <summary>
/// A logger.
/// </summary>
public class Logger
{
    /// <summary>
    /// Writes an error message.
    /// </summary>
    /// <param name="message">The message (template) to write.</param>
    /// <param name="args">One or more argument for the template.</param>
    public void Err(string message, params object?[]? args)
    {
        Log("ERROR", message, args);
    }

    /// <summary>
    /// Writes an info message.
    /// </summary>
    /// <param name="message">The message (template) to write.</param>
    /// <param name="args">One or more argument for the template.</param>
    public void Info(string message, params object?[]? args)
    {
        Log("INFO", message, args);
    }

    /// <summary>
    /// Writes a log message.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="message">The message (template) to write.</param>
    /// <param name="args">One or more argument for the template.</param>
    public void Log(string type, string message, params object?[]? args)
    {
        var time = DateTime.UtcNow;

        Console.Write("[{0:yyyy-MM-dd HH:mm:ss.fff}] [{1}]: ", time, type);
        Console.WriteLine(message, args);
    }

    /// <summary>
    /// Writes a warning message.
    /// </summary>
    /// <param name="message">The message (template) to write.</param>
    /// <param name="args">One or more argument for the template.</param>
    public void Warn(string message, params object?[]? args)
    {
        Log("WARN", message, args);
    }
}
