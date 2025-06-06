using System.Collections.Generic;
using EShiftSystem.Models;

namespace EShiftSystem.ViewModels
{
    public class TransportUnitFormViewModel
    {
        public TransportUnit TransportUnit { get; set; } = new TransportUnit{Name=""};

        // IDs of assistants selected in the form
        public List<int> SelectedAssistantIds { get; set; } = new List<int>();
    }
}
