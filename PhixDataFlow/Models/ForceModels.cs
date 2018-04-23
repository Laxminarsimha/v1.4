using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhixDataFlow;

namespace PhixDataFlow.Models
{
    
    public class ForceLoginModel
    {

        public string UserName = "laxminarsimha.maringanti@realpage.com";

        //public string Password = "Member$100614Vijhyv4AE4a0QQlzwo3vWwhb";
        public string Password = "Member$1234";

        //public string EndPoint = "https://www.salesforce.com/services/Soap/u/3.0";

        public string EndPoint = "https://login.salesforce.com/services/Soap/u/26.0";
        
        public string Proxy { get; set; }
        public string ClientID { get; set; }

    }

    /// <summary>
	/// Summary description for PartnerSample.
	/// </summary>
	public class SalesForcePartner
    {
        public SalesForcePartner()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        static sforce.SforceService _binding = new PhixDataFlow.sforce.SforceService();
        static sforce.LoginResult _loginResult = null;


        public static sforce.LoginResult loginResult
        {
            get { return _loginResult; }
            set { _loginResult = value; }
        }

        public static sforce.SforceService binding
        {
            get { return _binding; }
            set { _binding = value; }

        }


        public static string GetFieldValue(string fieldName, System.Xml.XmlElement[] any)
        {
            for (int i = 0; i < any.Length; i++)
            {
                if (any[i].LocalName == fieldName)
                    return any[i].InnerText;
            }
            return null;
        }

        public static System.Data.DataTable CreateDataTable(string objectName, sforce.DescribeSObjectResult dsr)
        {
            System.Data.DataTable dt = new System.Data.DataTable(objectName);
            //build column list
            string selectList = "";
            for (int i = 0; i < dsr.fields.Length; i++)
            {
                System.Data.DataColumn dc = new System.Data.DataColumn(dsr.fields[i].name, GetSystemType(dsr.fields[i].type));
                dc.ReadOnly = !dsr.fields[i].updateable;
                dc.AllowDBNull = dsr.fields[i].nillable;
                if (dsr.fields[i].length > 0)
                    dc.MaxLength = dsr.fields[i].length;
                if (dsr.fields[i].label != null)
                    dc.Caption = dsr.fields[i].label;
                else
                    dc.Caption = dsr.fields[i].name;
                dt.Columns.Add(dc);
                selectList += dsr.fields[i].name + ", ";
            }
            return dt;
        }

        public static string CreateSelectList(sforce.DescribeSObjectResult dsr)
        {
            string selectList = "";
            for (int i = 0; i < dsr.fields.Length; i++)
            {
                selectList += dsr.fields[i].name + ", ";
            }
            selectList = selectList.Substring(0, selectList.Length - 2);
            return selectList;
        }

        public static string FormatISO861(System.DateTime dt)
        {
            string output = dt.Year + "-" + dt.Month.ToString().PadLeft(2, Convert.ToChar("0")) + "-" + dt.Day.ToString().PadLeft(2, Convert.ToChar("0"));
            output += "T" + dt.Hour.ToString().PadLeft(2, Convert.ToChar("0")) + ":" + dt.Minute.ToString().PadLeft(2, Convert.ToChar("0")) + ":" + dt.Second.ToString().PadLeft(2, Convert.ToChar("0")) + "Z";
            return output;
        }

        public static sforce.sObject[] RowsToObjects(System.Data.DataSet ds)
        {
            try
            {
                System.Collections.ArrayList flds = new System.Collections.ArrayList();
                System.Xml.XmlDocument xdoc = new System.Xml.XmlDocument();
                sforce.sObject[] records = new sforce.sObject[ds.Tables[0].Rows.Count];
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    sforce.sObject record = new sforce.sObject();
                    record.type = ds.Tables[0].TableName;
                    for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                    {
                        if (!ds.Tables[0].Columns[j].ReadOnly)
                        {
                            if (ds.Tables[0].Rows[i][ds.Tables[0].Columns[j].ColumnName] != null && ds.Tables[0].Rows[i][ds.Tables[0].Columns[j].ColumnName] != DBNull.Value)
                            {
                                System.Xml.XmlElement xEl = xdoc.CreateElement(ds.Tables[0].Columns[j].ColumnName);
                                if (ds.Tables[0].Columns[j].DataType == typeof(System.DateTime))
                                    xEl.InnerText = FormatISO861((DateTime)ds.Tables[0].Rows[i][ds.Tables[0].Columns[j].ColumnName]);
                                else
                                    xEl.InnerText = ds.Tables[0].Rows[i][ds.Tables[0].Columns[j].ColumnName].ToString();
                                flds.Add(xEl);
                            }
                        }
                        else if (ds.Tables[0].Columns[j].ColumnName.Equals("Id"))
                        {
                            record.Id = ds.Tables[0].Rows[i][ds.Tables[0].Columns[j].ColumnName].ToString();
                        }
                    }
                    System.Xml.XmlElement[] xarray = new System.Xml.XmlElement[flds.Count];
                    flds.CopyTo(xarray);
                    record.Any = xarray;
                    records[i] = record;
                }
                return records;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                return null;
            }
        }

        public static void LoadDataTable(sforce.QueryResult qr, System.Data.DataTable dt)
        {
            sforce.sObject[] records = qr.records;
            for (int i = 0; i < records.Length; i++)
            {
                System.Data.DataRow dr = dt.NewRow();
                if (records[i].Id != null)
                    dr["Id"] = records[i].Id;
                System.Xml.XmlElement[] fields = records[i].Any;
                for (int j = 0; j < fields.Length; j++)
                {
                    if (!fields[j].LocalName.Equals("type"))
                    {
                        if (fields[j].InnerText.Length > 0)
                            dr[fields[j].LocalName] = fields[j].InnerText;
                        else if (!dr.Table.Columns[fields[j].LocalName].AllowDBNull)
                        {
                            if (dr.Table.Columns[fields[j].LocalName].DataType.Equals(typeof(System.DateTime)))
                                dr[fields[j].LocalName] = System.DateTime.Now;
                            else if (dr.Table.Columns[fields[j].LocalName].DataType.Equals(typeof(System.Double)))
                                dr[fields[j].LocalName] = 0.0;
                            else if (dr.Table.Columns[fields[j].LocalName].DataType.Equals(typeof(System.Int32)))
                                dr[fields[j].LocalName] = 0;
                            else
                                dr[fields[j].LocalName] = "(missing)";
                        }
                    }

                }
                dt.Rows.Add(dr);
            }
        }

        public static System.Type GetSystemType(sforce.fieldType sfdcType)
        {
            System.Type returnValue = System.Type.GetType("string");

            switch (sfdcType)
            {
                case sforce.fieldType.@string:
                    returnValue = System.Type.GetType("System.String");
                    break;
                case sforce.fieldType.picklist:
                    returnValue = System.Type.GetType("System.String");
                    break;
                case sforce.fieldType.combobox:
                    returnValue = System.Type.GetType("System.String");
                    break;
                case sforce.fieldType.reference:
                    returnValue = System.Type.GetType("System.String");
                    break;
                case sforce.fieldType.base64:
                    returnValue = System.Type.GetType("System.String");
                    break;
                case sforce.fieldType.boolean:
                    returnValue = System.Type.GetType("System.Boolean");
                    break;
                case sforce.fieldType.currency:
                    returnValue = System.Type.GetType("System.Double");
                    break;
                case sforce.fieldType.textarea:
                    returnValue = System.Type.GetType("System.String");
                    break;
                case sforce.fieldType.@int:
                    returnValue = System.Type.GetType("System.Int32");
                    break;
                case sforce.fieldType.@double:
                    returnValue = System.Type.GetType("System.Double");
                    break;
                case sforce.fieldType.percent:
                    returnValue = System.Type.GetType("System.Int32");
                    break;
                case sforce.fieldType.phone:
                    returnValue = System.Type.GetType("System.String");
                    break;
                case sforce.fieldType.id:
                    returnValue = System.Type.GetType("System.String");
                    break;
                case sforce.fieldType.date:
                    returnValue = System.Type.GetType("System.DateTime");
                    break;
                case sforce.fieldType.datetime:
                    returnValue = System.Type.GetType("System.DateTime");
                    break;
                case sforce.fieldType.url:
                    returnValue = System.Type.GetType("System.String");
                    break;
                case sforce.fieldType.email:
                    returnValue = System.Type.GetType("System.String");
                    break;
            }
            return returnValue;
        }

    }
    public class lbItem
    {
        string[] _textFields;
        sforce.sObject _item;

        public string[] textFields
        {
            get { return _textFields; }
            set { _textFields = value; }
        }
        public sforce.sObject item
        {
            get { return _item; }
            set { _item = value; }
        }

        public lbItem(sforce.sObject item, string[] textFields)
        {
            //
            // TODO: Add constructor logic here
            //
            this.item = item;
            this.textFields = textFields;
        }

        public override string ToString()
        {
            string output = "";
            for (int i = 0; i < textFields.Length; i++)
            {
                string fldValue = SalesForcePartner.GetFieldValue(textFields[i], item.Any);
                if (fldValue != null)
                    output += fldValue + " ";
            }
            return output.Trim();
        }


    }
}