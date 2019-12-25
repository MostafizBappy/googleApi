
using Microsoft.AspNet.Identity;
using MySql.Data.MySqlClient;
using PaloVideoManager.DataLayer;
using PaloVideoManager.Models;
using System;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Web;
using System.Web.Mvc;

namespace PaloVideoManager.BusinessLayer
{
    public class Common
    {
        public class AuthorizeSessionAttribute : AuthorizeAttribute
        {
            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                var userId = httpContext.Session["UserId"];

                if (userId == null)
                {
                    var lo = httpContext.GetOwinContext().Authentication;
                    lo.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                    return false;
                }

                return true;
            }
        }

        public class AuthorizeSessionServicePermission : AuthorizeAttribute
        {
            //protected override void HandleUnauthorizedRequest(Htt filterContext)
            //{
            //    //filterContext.Result = new HttpUnauthorizedResult(); // Try this but i'm not sure
            //    filterContext.Result = new RedirectResult("~/Home/Unauthorized");
            //}

            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                var userId = httpContext.Session["ServicePermission"];

                if (userId == null)
                {
                    //var lo = httpContext.GetOwinContext().Authentication;
                    //lo.(DefaultAuthenticationTypes.ApplicationCookie);
                    httpContext.Response.Redirect("~/Home/Index");
                    return false;
                }

                return true;
            }
        }
        public enum VideoManagerStatus
        {
            pending = 1,
            inProgress,
            saved,
            youtubeUploaded
        }

        string mySqlCon = "server=192.168.4.21;user id=sa;password=editor@123;database=newswrap;charset=utf8;Convert Zero Datetime=True";
        ReporterEntities _context = new ReporterEntities();
        public bool DoesUserExist(string userName)
        {
            using (var domainContext = new PrincipalContext(ContextType.Domain, "palonet.org"))
            {
                using (var foundUser = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, userName))
                {
                    return foundUser != null;
                }
            }
        }

        public bool AuthenticateUser(string user, string pass)
        {
            DirectoryEntry de = new DirectoryEntry(domainPath, user, pass, AuthenticationTypes.Secure);
            try
            {

                DirectorySearcher ds = new DirectorySearcher(de);
                ds.FindOne();
                return true;
            }
            catch
            {

                return false;
            }
        }
        string domainPath = "LDAP://DC=palonet,DC=org";

        public string GetEmailFromAD(string empID)
        {
            string email = String.Empty;
            if (!String.IsNullOrEmpty(empID))
            {
                var oroot = new DirectoryEntry("LDAP://DC=palonet,DC=org");
                var osearcher = new DirectorySearcher(oroot);
                osearcher.Filter = string.Format("(&(sAMAccountName={0}))", empID);
                var oresult = osearcher.FindAll();

                if (oresult.Count == 1)
                {
                    if (oresult[0].Properties.Contains("mail"))
                        email = oresult[0].Properties["mail"][0].ToString();
                }
            }
            return email;
        }

        public SessionDataModel GetSessionDataByID(string id)
        {
            SessionDataModel model = new SessionDataModel();
            string query = "SELECT employeecode,displayname,designationcode,centercode,usercode,groupcode,publicationcode,deskcode FROM employee WHERE ldapint='" + id + "';";
            MySqlConnection MyConn2 = new MySqlConnection(mySqlCon);
            MySqlCommand MyCommand2 = new MySqlCommand(query, MyConn2);
            MyConn2.Open();
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = MyCommand2.ExecuteReader();
            if (dataReader.Read())
            {
                model.EmployeeId = id;
                model.EmployeeCode = dataReader["employeecode"].ToString();
                model.DisplayName = dataReader["displayname"].ToString();
                model.PublicationCode = dataReader["publicationcode"].ToString();
                model.DeskCode = dataReader["deskcode"].ToString();
                model.CenterCode = dataReader["centercode"].ToString();
                model.Email = GetEmailFromAD(id);
            }
            MyConn2.Close();

            return model;
        }




        public bool DoesUserExistNewswrap(string username)
        {
            string Query = "SELECT * FROM employee WHERE ldapint='" + username + "';";
            MySqlConnection MyConn2 = new MySqlConnection(mySqlCon);
            MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
            MyConn2.Open();
            MySqlDataAdapter MyAdapter = new MySqlDataAdapter();
            MyAdapter.SelectCommand = MyCommand2;
            DataTable dTable = new DataTable();
            MyAdapter.Fill(dTable);
            int count = dTable.Rows.Count;
            MyConn2.Close();
            if (count == 0)
            {
                return false;
            }
            else
                return true;
        }
    }
}