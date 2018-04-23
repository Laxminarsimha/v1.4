using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhixDataFlow.Models
{
    public static class StaticData
    {

        public static List<TicketData> TicketData { get; set; }

        public static void Loaddata(string Filepath)
        {
            TicketData = new List<Models.TicketData>();
            List<TicketData> ticketDataList = new List<TicketData>();
            if (System.IO.File.Exists(Filepath))
            {
                var data = System.IO.File.ReadAllLines(Filepath);
                for (int i = 1; i < data.Length; i++)
                {
                    var ticket = data[i];
                    TicketData currentData = new TicketData();
                    currentData.EscalationId = ticket.Split(',')[0];
                    currentData.Summary = ticket.Split(',')[1];
                    currentData.Priority = ticket.Split(',')[2];
                    currentData.EscalationStatus = ticket.Split(',')[3];
                    TicketData.Add(currentData);
                }
            }
        }
    }
}