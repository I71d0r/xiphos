using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Xiphos.Areas.Administration.Validation;

namespace Xiphos.Areas.Administration.Models
{
    /// <summary>
    /// MelodyModel model
    /// </summary>
    public class MelodyModel
    {
        [Key]
        [DisplayName("Identifier")]
        public int Id { get; set; }

        [MaxLength(512)]
        [Required(ErrorMessage = "MelodyModel name is required.")]
        [DisplayName("MelodyModel Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "MelodyModel data are required.")]
        [NotesOnly(ErrorMessage = "MelodyModel data can contain only space delimited notes e.g, C C# Db etc.")]
        [DisplayName("MelodyModel Data")]
        public string Data { get; set; }

    }
}
