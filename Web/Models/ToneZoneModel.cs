using Xiphos.Areas.Administration.Models;

namespace Xiphos.Models
{
    public class ToneZoneModel
    {
        public bool DisplayDebugMessages { get; set; }
        public int MaxDisplayRowsCount { get; set; } = 10;
        public MelodyModel[] Melodies { get; set; }
    }
}
