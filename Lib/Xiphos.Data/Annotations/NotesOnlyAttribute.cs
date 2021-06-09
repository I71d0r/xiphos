using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xiphos.Data.Models;

namespace Xiphos.Data.Annotations
{
    /// <summary>
    /// Validation attribute for melody data
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NotesOnlyAttribute : ValidationAttribute
    {
        private static readonly HashSet<string> Notes =
            new HashSet<string>(new[]
            {
                "C", "C#", "Db","D", "D#", "Eb",
                "E", "F", "F#", "Gb", "G", "G#",
                "Ab", "A", "A#", "Bb", "B"
            });

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var fails = MelodyHelper.ParseNotes(value)
                .Where(note => !Notes.Contains(note))
                .ToList();

            return fails.Count == 0
                ? ValidationResult.Success
                : new ValidationResult($"Invalid notes: {string.Join(',', fails)}", new[] { nameof(MelodyModel.Data) });
        }
    }
}
