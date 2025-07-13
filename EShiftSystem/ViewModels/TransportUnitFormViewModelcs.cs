using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EShiftSystem.Models;

namespace EShiftSystem.ViewModels
{
    // view model for creating/editing transport units with associated assistants
    public class TransportUnitFormViewModel : IValidatableObject
    {
        // the transport unit being created or edited
        public TransportUnit TransportUnit { get; set; } = new TransportUnit{Name=""};

        // collection of assistant ids selected for this transport unit
        [Display(Name = "Assistants")]
        public List<int> SelectedAssistantIds { get; set; } = new List<int>();

        // Custom validation method to enforce assistant limits
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SelectedAssistantIds != null && SelectedAssistantIds.Count > 4)
            {
                yield return new ValidationResult(
                    "A transport unit can have a maximum of 4 assistants.",
                    new[] { nameof(SelectedAssistantIds) }
                );
            }
        }
    }
}
