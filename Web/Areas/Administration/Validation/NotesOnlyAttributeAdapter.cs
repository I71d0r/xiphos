using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace Xiphos.Areas.Administration.Validation
{
    /// <summary>
    /// Note only validation attribute adapter
    /// </summary>
    public class NotesOnlyAttributeAdapter : AttributeAdapterBase<NotesOnlyAttribute>
    {
        public NotesOnlyAttributeAdapter(NotesOnlyAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer)
        {
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            // --Notable--
            // The framework will ensure that an html editor (input) will be decorated with following attributes.
            // Those are detected by unobtrusive jQuery validation.
            // The jQuery framework will require a handler to be registered to validate data-val-notes-only elements.
            // See melody-validators.js
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-notes-only", GetErrorMessage(context));
        }
        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
        }
    }
}
