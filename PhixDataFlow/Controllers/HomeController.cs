using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using PhixDataFlow.Models;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;

namespace PhixDataFlow.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        List<TicketData> Last24HrsData;
        List<TicketData> MyDefectsData;
        public ActionResult GetLast24HrsDeftecsList()
        {
            ForceLoginController forceLogin = new ForceLoginController();
            if (forceLogin.SaleForceLoginAuthentication())
            {
                DataTable dt = Get24HrsDataSOQL();
                if (dt != null)
                {
                    Last24HrsData = LoadData(dt);
                    ViewBag.Last24HrsCount = dt.Rows.Count;
                    int numberOfClosedRecords = dt.Select("Escalation_Status__c = 'Closed'").Length;
                    ViewBag.TodayClosedDefects = numberOfClosedRecords;
                    //List<TicketData> ticketdata = LoadData(Server.MapPath("~/Source/TodaysDefects.csv"));
                    var resultdata = Last24HrsData.GroupBy(t => t.EscalationStatus).Select(k => new { name = k.Key, y = k.Count() });
                    return this.Json(resultdata, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        public ActionResult GetMyDefectsList()
        {
            ForceLoginController forceLogin = new ForceLoginController();
            if (forceLogin.SaleForceLoginAuthentication())
            {
                DataTable dt = GetMyDefectsData();
                if (dt != null)
                {
                    List<TicketData> ticketdata = LoadData(dt);
                    //List<TicketData> ticketdata = LoadData(Server.MapPath("~/Source/Tickets.csv"));
                    var resultdata = ticketdata.GroupBy(t => t.EscalationStatus).Select(k => new { name = k.Key, y = k.Count() });
                    return this.Json(resultdata, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
            else
            {

                return null;

            }
        }

        public DataTable GetMyDefectsData()
        {
            string soqlQuery = "Select  Description__c,    " +
            " Escalation_ID__c,    Name, OwnerId,   " +
            "  Priority__c,   " +
            " Escalation_Status__c, Summary__c, Support_Product__c " +
            "from Problem_Management_Escalation__c  where Support_Product__c in ('OneSite Affordable', 'OneSite Tax Credits', 'OneSite Leasing and Rents Rural Housing') " +
            "and OwnerId ='00537000002yoU5AAI' order by Name desc";

            try
            {

                sforce.QueryResult qr = SalesForcePartner.binding.query(
                    soqlQuery);
                if (qr.size > 0)
                {
                    DataTable dt = CreateDataTable(ParseFieldList(soqlQuery), qr);
                    return dt;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
                //throw ex;
            }
        }


        private DataTable Get24HrsDataSOQL()
        {
            //Cursor = Cursors.WaitCursor;
            try
            {
                string soqlQuery = "Select  Description__c,    " +
            " Escalation_ID__c,    Name, OwnerId,  " +
            "  Priority__c,  " +
            "  Escalation_Status__c, Summary__c, Support_Product__c " +
            "from Problem_Management_Escalation__c  where Support_Product__c in ('OneSite Affordable', 'OneSite Tax Credits', 'OneSite Leasing and Rents Rural Housing') " +
            "and DAY_ONLY(Date_Escalated__c)=LAST_N_DAYS:1 order by Name desc";
                //this.btnMore.Visible = false;
                //PartnerSample.binding.QueryOptionsValue = new sforce.QueryOptions();
                //PartnerSample.binding.QueryOptionsValue.batchSize = Convert.ToInt16(Application.UserAppDataRegistry.GetValue("batchSize", "500"));
                //PartnerSample.binding.QueryOptionsValue.batchSizeSpecified = true;

                sforce.QueryResult qr = SalesForcePartner.binding.query(
                    soqlQuery);
                if (qr.size > 0)
                {

                    DataTable dt = CreateDataTable(ParseFieldList(soqlQuery), qr);
                    return dt;
                }
                else
                {
                    return null;
                    //System.Windows.Forms.MessageBox.Show("No records matched query.", "Patner Sample", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                return null;
                //System.Windows.Forms.MessageBox.Show("Query failed: " + ex.Message, "Partner Sample", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            //Cursor = Cursors.Default;
        }

        private ArrayList ParseFieldList(String soqlText)
        {
            //remove the select statement
            String temp = soqlText.Substring(soqlText.ToLower().IndexOf(" ")).Trim();
            //remove the everything after the field list
            temp = temp.Substring(0, temp.ToLower().IndexOf("from") - 1);
            //remove all the spaces
            temp = temp.Replace(" ", "");
            //return the tokenized array
            String[] temparray = temp.Split(",".ToCharArray());
            ArrayList al = new ArrayList();
            for (int i = 0; i < temparray.Length; i++)
            {
                al.Add(temparray[i]);
            }
            return al;
        }

        private DataTable CreateDataTable(ArrayList fieldList, sforce.QueryResult qr)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < fieldList.Count; i++)
            {
                dt.Columns.Add(fieldList[i].ToString());
            }
            for (int i = 0; i < qr.records.Length; i++)
            {
                DataRow dr = dt.NewRow();
                sforce.sObject record = qr.records[i];
                if (dr.Table.Columns.Contains("ID"))
                    dr["Id"] = record.Id;
                for (int j = 0; j < record.Any.Length; j++)
                {
                    System.Xml.XmlElement xEl = (System.Xml.XmlElement)record.Any[j];
                    dr[xEl.LocalName] = xEl.InnerText;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }


        public ActionResult GetLast24HrsStatusData(string status)
        {

            DataTable dt = Get24HrsDataSOQL();
            if (dt != null)
            {
                Last24HrsData = LoadData(dt);
            }
            if (Last24HrsData.Count > 0)
            {
                ViewBag.FilteredData = Last24HrsData.Where(t => t.EscalationStatus == status).ToList();
                return PartialView("~/Views/Home/_FilteredView.cshtml");
            }
            else
            {
                return null;
            }
        }

        public ActionResult GetMyDefectsStatusData(string status)
        {

            DataTable dt = GetMyDefectsData();
            if (dt != null)
            {

                MyDefectsData = LoadData(dt);
            }
            if (MyDefectsData.Count > 0)
            {
                ViewBag.FilteredData = MyDefectsData.Where(t => t.EscalationStatus == status).ToList();
                return PartialView("~/Views/Home/_FilteredView.cshtml");
            }
            else
            {
                return null;
            }
        }
        public List<TicketData> LoadData(string Filepath)
        {
            List<TicketData> ticketdata = new List<TicketData>();
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
                    ticketdata.Add(currentData);
                }
            }
            return ticketdata;
        }
        public List<TicketFixesData> LoadPossibleFixData(DataTable dtRecords)
        {
            List<TicketFixesData> ticketdata = new List<TicketFixesData>();
            if (dtRecords.Columns.Count > 0)
            {

                for (int i = 0; i <= dtRecords.Rows.Count - 1; i++)
                {
                    var ticket = dtRecords.Rows[i];
                    TicketFixesData currentData = new TicketFixesData();
                    currentData.EscalationId = ticket["Name"].ToString();
                    currentData.Summary = ticket["Summary__c"].ToString();
                    currentData.Priority = ticket["Priority__c"].ToString();
                    currentData.EscalationStatus = ticket["Escalation_Status__c"].ToString();
                    if (currentData.EscalationStatus == "Closed" && (IsAffordableDataFix(currentData.EscalationId)))
                    {
                        currentData.IsFixAvailable = true;
                        //GetAffordableDataFixes
                    }
                    else
                    {
                        currentData.IsFixAvailable = false;
                    }

                    ticketdata.Add(currentData);
                }
            }
            return ticketdata;
        }
        private string EnumerateFilesByFilter(String path, string contain)
        {
            try
            {
                String[] files = Directory.EnumerateFiles(path).Where(f => f.Contains(contain)).ToArray();
                foreach (string fileName in files)
                {
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        return fileName;
                    }
                    else
                    {
                        return "";
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }


        }

        private bool IsAffordableDataFix(string escalationId)
        {
            try
            {
                string path = @"\\RPIPL2-HE-DV059\Laxminarsimha\AffordableDataFixes";
                //\\rpidaldbf001\dcms\DataFixes\OneSite\SQLFiles

                bool contains = Directory.EnumerateFiles(path).Any(f => f.Contains(escalationId));
                if (!contains)
                {

                    contains = Directory.EnumerateFiles(path).Any(f => f.Contains(escalationId.Replace("PME-", "").Trim().ToString()));
                }
                return contains;
                //GetAffordableDataFixesList();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //private void GetAffordableDataFixesList()
        //{
        //    string[] files = Directory.GetFiles("\\rpidaldbf001\\dcms\\DataFixes\\OneSite\\SQLFiles");
        //    //Affordable - PME -
        //}

        public List<TicketData> LoadData(DataTable dtRecords)
        {
            List<TicketData> ticketdata = new List<TicketData>();
            if (dtRecords.Columns.Count > 0)
            {

                for (int i = 0; i <= dtRecords.Rows.Count - 1; i++)
                {
                    var ticket = dtRecords.Rows[i];
                    TicketData currentData = new TicketData();
                    currentData.EscalationId = ticket["Name"].ToString();
                    currentData.Summary = ticket["Summary__c"].ToString();
                    currentData.Priority = ticket["Priority__c"].ToString();
                    currentData.EscalationStatus = ticket["Escalation_Status__c"].ToString();
                    ticketdata.Add(currentData);
                }
            }
            return ticketdata;
        }

        public ActionResult Search()
        {
            ViewBag.Message = "";

            return View();
        }

        [HttpGet]
        public ActionResult ShowFile(string EscalationId)
        {
            string path = @"\\RPIPL2-HE-DV059\Laxminarsimha\AffordableDataFixes";
            string filePath = EnumerateFilesByFilter(path, EscalationId);
            // Get file full name with PME ID
            string content = string.Empty;

            try
            {
                string InitialText = System.IO.File.ReadAllText(filePath);
                content = InitialText;
            }
            catch (Exception exc)
            {
                content = "Uh oh!";
            }

            ViewBag.ContentData = content;
            return PartialView("~/Views/Home/_Download.cshtml");
        }

        public ActionResult GetSearchedData(string search)
        {
            DataTable dtOutcome = new DataTable();
            DataTable dt = GetSearchedDataSOQL(search);
            if (dt.Rows.Count >= 1)
            {
                dtOutcome = dt.AsEnumerable().OrderBy(x => x["Name"]).Take(10).AsEnumerable().CopyToDataTable();
                dtOutcome.DefaultView.ToTable( /*distinct*/ true);
            }
            List<TicketFixesData> searchedQuery = new List<TicketFixesData>();
            if (dtOutcome != null)
            {
                searchedQuery = LoadPossibleFixData(dtOutcome);
            }

            //List<TicketData> ticketdata = LoadData(Server.MapPath("~/Source/TodaysDefects.csv"));
            ViewBag.FilteredData = searchedQuery;
            return PartialView("~/Views/Home/_PossibleFix.cshtml");

        }

        private static HashSet<String> s_StopWords =
  new HashSet<String>(StringComparer.OrdinalIgnoreCase) {
    "is", "am", "are", "were", "was", "do", "does", "to", "from", // etc.
};

        private static Char[] s_Separators = new Char[] {
  '\r', '\n', ' ', '\t', '.', ',', '!', '?', '"', //TODO: check this list 
};


        private DataTable GetSearchedDataSOQL(string search)
        {
            //Cursor = Cursors.WaitCursor;
            try
            {
                string soqlQuery = "Select Description__c,     " +
              " Escalation_ID__c,    Name, OwnerId,  " +
              "  Priority__c, Problem_Mgmt_Action_Pending_Date__c, Related_PME__c, " +
              " Escalation_Status__c, Summary__c, Support_Product__c " +
              "from Problem_Management_Escalation__c  where Support_Product__c in ('OneSite Affordable', 'OneSite Tax Credits', 'OneSite Leasing and Rents Rural Housing') " +
              "order by Name desc";

                sforce.QueryResult qr = SalesForcePartner.binding.query(
                    soqlQuery);
                if (qr.size > 0)
                {
                    String[] words = search
  .Split(s_Separators, StringSplitOptions.RemoveEmptyEntries)
  .Where(word => !s_StopWords.Contains(word))
  .ToArray();

                    // Combine back: "I go school"
                    String result = String.Join(" ", words);

                    DataTable dt = CreateDataTable(ParseFieldList(soqlQuery), qr);
                    //string[] words = search.Split(' ');

                    string[] sentences = Regex.Split(search, @"(?<=[.!?])\s+(?=\p{Lt})");
                    DataTable dtResult = new DataTable();
                    DataTable dtDescriptionQuery = new DataTable();
                    if (dt == null)
                    {
                        return null;
                    }
                    DataTable dtSummaryQuery = new DataTable();
                    if (dt.Select("Summary__c LIKE '%" + search + "%'").Count() > 0)
                    {
                        dtSummaryQuery = dt.Select("Summary__c LIKE '%" + search + "%'").CopyToDataTable();
                    }


                    DataTable dtOutput = new DataTable();
                    for (int i = 0; i <= words.Length - 1; i++)
                    {
                        if (dt.Select("Description__c LIKE '%" + words[i] + "%'") != null)
                        {
                            dt = dt.Select("Description__c LIKE '%" + words[i] + "%'").CopyToDataTable();
                            if (dt.Rows.Count < 20)
                            {
                                dtDescriptionQuery.Merge(dt);
                                break;
                            }
                        }
                    }
                    DataTable dtResultsData = new DataTable();
                    if (dtSummaryQuery.Rows.Count >= 1)
                    {
                        dtResult.Merge(dtSummaryQuery);
                    }
                    if (dtDescriptionQuery.Rows.Count >= 1)
                    {
                        dtResult.Merge(dtDescriptionQuery);
                    }
                    if (dtResult.Rows.Count > 1)
                    {
                        dtResult.DefaultView.Sort = "Name desc";
                        dtResultsData = dtResult.DefaultView.ToTable();
                    }
                    return dtResultsData;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }


        public ActionResult GetPossibleDataFixes(string search)
        {
            List<TicketData> ticketdata = LoadData(Server.MapPath("~/Source/Tickets.csv"));
            ViewBag.FilteredData = ticketdata.Where(t => t.Summary.ToLower().Contains(search.ToLower())).ToList();
            return PartialView("~/Views/Home/_FilteredView.cshtml");
        }


        // New Implementation

        public ActionResult GetTodaysEscalations()
        {
            List<TicketData> ticketdata = LoadData(Server.MapPath("~/Source/Tickets.csv"));
            var resultdata = ticketdata.GroupBy(t => t.EscalationStatus).Select(k => new { name = k.Key, y = k.Count() });
            return this.Json(resultdata, JsonRequestBehavior.AllowGet);
        }

        public void GetHighPriorityDefects()
        {
            ForceLoginController forceLogin = new ForceLoginController();
            if (forceLogin.SaleForceLoginAuthentication())
            {
                DataTable dt = GetHighPriorityDefectsSOQL();
                int numberOfRecords = dt.AsEnumerable().Where(x => x["Priority__c"].ToString() == "High").ToList().Count;
                ViewBag.HighPriorityDefects = numberOfRecords;
                //ViewData["HighPriorityDefects"] = numberOfRecords;

            }
            else
            {
                ViewBag.HighPriorityDefects = "0";
            }

        }

        public void GetOverallNewDefects()
        {
            ForceLoginController forceLogin = new ForceLoginController();
            if (forceLogin.SaleForceLoginAuthentication())
            {
                DataTable dt = GetOverallNewDefectsSOQL();
                if (dt != null)
                {
                    int numberOfRecords = dt.AsEnumerable().Where(x => x["Escalation_Status__c"].ToString() == "New").ToList().Count;
                    ViewBag.OverallNewDefects = numberOfRecords;
                    //ViewData["OverallNewDefects"] = numberOfRecords;

                }
            }
            else
            {
                ViewBag.OverallNewDefects = "0";
            }
        }

        private DataTable GetOverallNewDefectsSOQL()
        {
            try
            {
                string soqlQuery = "Select  Description__c,    " +
            " Escalation_ID__c,    Name, OwnerId,  " +
            "  Priority__c,  " +
            "  Escalation_Status__c, Summary__c, Support_Product__c " +
            "from Problem_Management_Escalation__c  where Escalation_Status__c='New' and Support_Product__c in ('OneSite Affordable', 'OneSite Tax Credits', 'OneSite Leasing and Rents Rural Housing') " +
            " order by Name desc";


                sforce.QueryResult qr = SalesForcePartner.binding.query(
                    soqlQuery);
                if (qr.size > 0)
                {

                    DataTable dt = CreateDataTable(ParseFieldList(soqlQuery), qr);
                    return dt;
                }
                else
                {
                    return null;
                    //System.Windows.Forms.MessageBox.Show("No records matched query.", "Patner Sample", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                return null;
                //System.Windows.Forms.MessageBox.Show("Query failed: " + ex.Message, "Partner Sample", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        private DataTable GetHighPriorityDefectsSOQL()
        {
            try
            {
                string soqlQuery = "Select  Description__c,    " +
            " Escalation_ID__c,    Name, OwnerId,  " +
            "  Priority__c,  " +
            "  Escalation_Status__c, Summary__c, Support_Product__c " +
            "from Problem_Management_Escalation__c  where Priority__c ='P2 - High' and Support_Product__c in ('OneSite Affordable', 'OneSite Tax Credits', 'OneSite Leasing and Rents Rural Housing') " +
            "  order by Name desc";
                //this.btnMore.Visible = false;
                //PartnerSample.binding.QueryOptionsValue = new sforce.QueryOptions();
                //PartnerSample.binding.QueryOptionsValue.batchSize = Convert.ToInt16(Application.UserAppDataRegistry.GetValue("batchSize", "500"));
                //PartnerSample.binding.QueryOptionsValue.batchSizeSpecified = true;

                sforce.QueryResult qr = SalesForcePartner.binding.query(
                    soqlQuery);
                if (qr.size > 0)
                {

                    DataTable dt = CreateDataTable(ParseFieldList(soqlQuery), qr);
                    return dt;
                }
                else
                {
                    return null;
                    //System.Windows.Forms.MessageBox.Show("No records matched query.", "Patner Sample", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                return null;
                //System.Windows.Forms.MessageBox.Show("Query failed: " + ex.Message, "Partner Sample", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        [HttpGet]
        public ActionResult CheckSalesForceUserExists(DateTime param1, DateTime param2, string param3)
        {
            return Json(new { redirecturl = "" }, JsonRequestBehavior.AllowGet);

        }
      
    }
}