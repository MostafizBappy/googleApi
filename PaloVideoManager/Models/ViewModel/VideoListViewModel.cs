using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PaloVideoManager.Models.ViewModel
{
    public class VideoListViewModel
    {
        public decimal Id { get; set; }

        [Display(Name ="Video Name")]
        public string VideoName { get; set; }
        public int? CenterCode { get; set; }

        [Display(Name = "Video Path")]
        public string VideoPath { get; set; }
        public int? UserId { get; set; }

        //[Display(Name = "Full Name")]
        //public string UserFullName { get; set; }

        [Display(Name = "Entry Date")]
        public DateTime? EntryDate { get; set; }

        [Display(Name = "Video Date")]
        public DateTime? VideoDate { get; set; }

        [Display(Name = "Drive Id")]
        public string GoogleDriveId { get; set; }
        public string Description { get; set; }

        [Display(Name = "Size")]
        public string VideoSize { get; set; }
        public string ErrorMessage { get; set; }
    }
}