using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Xiphos.Areas.Administration.Validation;

namespace Xiphos.Areas.Administration.Models
{
    /// <summary>
    /// Melody model
    /// </summary>
    public class MelodyModel
    {
        [Key]
        [DisplayName("Identifier")]
        public int Id { get; set; }

        [MaxLength(512)]
        [Required(ErrorMessage = "Melody name is required.")]
        [DisplayName("Melody Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Melody data are required.")]
        [NotesOnly(ErrorMessage = "Melody data can contain only space delimited notes e.g, C C# Db etc.")]
        [DisplayName("Melody Data")]
        public string Data { get; set; }

    }
}
