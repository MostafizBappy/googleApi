using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloVideoManager.Models
{
    public class SessionDataModel
    {
        public string UserId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string DisplayName{ get; set; }
        public string PublicationCode { get; set; }
        public string DeskCode { get; set; }
        public string Email { get; set; }
        public string ServicePermission { get; set; }
        public string CenterCode { get; internal set; }
    }
}