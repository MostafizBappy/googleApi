using PaloVideoManager.DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PaloVideoManager.Models.ViewModel
{
    public class VideoManagerViewModel
    {
        [Display(Name ="Video Manger Id")]
        public int VideoManagerId { get; set; }

        [Display(Name ="Video Summary Id")]
        public decimal VideoSummaryId { get; set; }

        [Display(Name = "Taken By Id")]
        public int? TakenByUserId { get; set; }

        [Display(Name = "Taken By")]
        public string TakenByName { get; set; }

        [Display(Name ="Taken By Time")]
        public DateTime? TakenByTime { get; set; }

        [Display(Name = "Completed Time")]
        public DateTime? CompletedTime { get; set; }

        public string Status { get; set; }

        [Display(Name = "Local File Path")]
        public string LocalCompletedFilePath { get; set; }

        [Display(Name ="Youtube Id")]
        public string YoutubeId { get; set; }
        public string StreamingLink { get; set; }

        public VideoSummaryViewModel VideoSummary { get; set; }
    }
}