
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

public static class FastBackupExtensions
{
    /// <summary>
    /// Normalizes a <see cref="DateTime" /> value up to its second part.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>The normalized value.</returns>
    public static DateTime NormalizeToSeconds(this DateTime value)
    {
        return new DateTime
        (
            value.Year, value.Month, value.Day,
            value.Hour, value.Minute, value.Second,
            0, 0,
            value.Kind
        );
    }
}
