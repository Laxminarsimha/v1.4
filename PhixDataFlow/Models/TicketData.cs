using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhixDataFlow.Models
{
    public class TicketData
    {
        public string EscalationId { get; set; }
        public string Summary { get; set; }
        public string Priority { get; set; }
        public string EscalationStatus { get; set; }
    }

    public class TicketFixesData
    {
        public string EscalationId { get; set; }
        public string Summary { get; set; }
        public string Priority { get; set; }
        public string EscalationStatus { get; set; }

        public bool  IsFixAvailable { get; set; }
    }
}