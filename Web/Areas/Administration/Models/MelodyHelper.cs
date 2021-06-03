using System;
using System.Collections.Generic;
using System.Linq;

namespace Xiphos.Areas.Administration.Models
{
    /// <summary>
    /// Melody parsing helper
    /// </summary>
    public static class MelodyHelper
    {
        /// <summary>
        /// Parses upper-cased, space delimited elements out of given string.
        /// </summary>
        /// <param name="data">Parsed string</param>
        /// <returns>Enumerator over parsed segments</returns>
        public static IEnumerable<string> ParseNotes(object data)
        {
            if (data is string dataString)
                return dataString
                    .Split(' ', StringSplitOptions.TrimEntries)
                    .Select(CorrectNodeFormat);

            return Enumerable.Empty<string>();
        }

        // DB | dB | Db | db => Db
        private static string CorrectNodeFormat(string note)
            => note.Length > 1 ?
                note[..1].ToUpperInvariant() + note[1..].ToLowerInvariant() : note.ToUpperInvariant();
    }
}
