using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhixDataFlow.Controllers
{
    using Models;
    using System.Net;

    public class ForceLoginController : Controller
    {
        // GET: ForceLogin
        public ActionResult Index()
        {
            return View();
        }

        public bool SaleForceLoginAuthentication()
        {
            try
            {
                ForceLoginModel froceLoginModel = new ForceLoginModel();
                if (froceLoginModel.EndPoint.ToString().Length > 0)
                {
                    SalesForcePartner.binding.Url = froceLoginModel.EndPoint.ToString();
                }
                //if (froceLoginModel.Proxy.ToString().Length > 0)
                //{
                //    SalesForcePartner.binding.Url = SalesForcePartner.binding.Url.Replace("https:", "http:");
                //    SalesForcePartner.binding.Proxy = new System.Net.WebProxy(froceLoginModel.Proxy.ToString());
                //}
                //else
                //{
                //    SalesForcePartner.binding.Proxy = null;
                //}

                //if (froceLoginModel.ClientID.ToString().Length > 0)
                //{
                //    SalesForcePartner.binding.CallOptionsValue = new sforce.CallOptions();
                //    SalesForcePartner.binding.CallOptionsValue.client = froceLoginModel.ClientID.ToString();
                //}
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls    | SecurityProtocolType.Tls11
       | SecurityProtocolType.Tls12
       | SecurityProtocolType.Ssl3;

                SalesForcePartner.loginResult = SalesForcePartner.binding.login(froceLoginModel.UserName, froceLoginModel.Password);
                SalesForcePartner.binding.SessionHeaderValue = new sforce.SessionHeader();
                SalesForcePartner.binding.SessionHeaderValue.sessionId = SalesForcePartner.loginResult.sessionId;
                SalesForcePartner.binding.Url = SalesForcePartner.loginResult.serverUrl;
                return true;
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Login failed: " + ex.Message, "Partner Sample", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return false;
            }
        }
    }
}