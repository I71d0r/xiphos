using Xiphos.Data.Models;

namespace Xiphos.Models
{
    /// <summary>
    /// Model for Tone Zone view
    /// </summary>
    public class ToneZoneModel
    {
        /// <summary>
        /// Default number of melody rows on the page selection
        /// </summary>
        public const int DefaultRowCount = 5;

        /// <summary>
        /// Whether or not display debug information on Tone Zone UI
        /// </summary>
        public bool DisplayDebugMessages { get; set; }

        /// <summary>
        /// Max row count in melody list
        /// </summary>
        public int MaxDisplayRowsCount { get; set; } = DefaultRowCount;


        /// <summary>
        /// Melodies to display
        /// </summary>
        public MelodyModel[] Melodies { get; set; }
    }
}
