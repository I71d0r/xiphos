using Xiphos.Areas.Administration.Models;
using Xiphos.Data.Models;

namespace Xiphos.Models
{
    /// <summary>
    /// Model for Tone Zone view
    /// </summary>
    public class ToneZoneModel
    {
        /// <summary>
        /// Whether or not display debug information on Tone Zone UI
        /// </summary>
        public bool DisplayDebugMessages { get; set; }

        /// <summary>
        /// Max row count in melody list
        /// </summary>
        public int MaxDisplayRowsCount { get; set; } = 5;


        /// <summary>
        /// Melodies to display
        /// </summary>
        public MelodyModel[] Melodies { get; set; }
    }
}
