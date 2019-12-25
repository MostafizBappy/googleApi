using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloVideoManager.Models.ViewModel
{
    public class VideoSummaryViewModel
    {
        public decimal Id { get; set; }
        public string VideoName { get; set; }
        public string VideoPath { get; set; }
        public int? UserId { get; set; }
        public string SubmittedByUserName { get; set; }
        public int? CenterCode { get; set; }
        public DateTime? VideoDate { get; set; }
        public string Description { get; set; }
        public string VideoSize { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string GoogleDriveId { get; set; }
        public string GoogleStreamingLink { get; set; }
    }
}