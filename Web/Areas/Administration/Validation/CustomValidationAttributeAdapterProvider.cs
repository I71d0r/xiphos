using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using Xiphos.Data.Annotations;

namespace Xiphos.Areas.Administration.Validation
{
    /// <summary>
    /// Custom validation attribute adapter provider provides adapters for both
    /// system and custom validation attributes.
    /// </summary>
    public class CustomValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private static readonly IValidationAttributeAdapterProvider BaseProvider
            = new ValidationAttributeAdapterProvider();

        public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer stringLocalizer)
        // --Notable--
        // A custom provider allows to register own adapters based on attribute type.
        // This provider has to registered as override in service container (startup).
            => attribute switch
            {
                NotesOnlyAttribute notesOnly => new NotesOnlyAttributeAdapter(notesOnly, stringLocalizer),
                _ => BaseProvider.GetAttributeAdapter(attribute, stringLocalizer)
            };
    }
}
