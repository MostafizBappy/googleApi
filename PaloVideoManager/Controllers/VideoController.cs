using PaloVideoManager.BusinessLayer;
using PaloVideoManager.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using static PaloVideoManager.BusinessLayer.Common;

namespace PaloVideoManager.Controllers
{
    
    public class VideoController : Controller
    {
        VideoRepository _repo = new VideoRepository();
        // GET: Video
        [AuthorizeSession]
        [HttpGet]
        public ActionResult Index()
        {
            //ViewBag.Error = "";
            DateTime date = DateTime.Now;
            return View(_repo.GetUserVideoList(date));
        }

        [HttpPost]
        [AuthorizeSession]
        public ActionResult Index(string date)
        {
            if (!String.IsNullOrEmpty(date))
            {
                DateTime videodate = Convert.ToDateTime(date);
                return View(_repo.GetUserVideoList(videodate));
            }
            return RedirectToAction("Index");
        }

        [AuthorizeSessionServicePermission]
        [HttpGet]
        public ActionResult VideoManagerIndex()
        {
            DateTime date = DateTime.Now;
            return View(_repo.GetAllVideos(date));
        }

        [AuthorizeSessionServicePermission]
        [HttpPost]
        public ActionResult VideoManagerIndex(string date)
        {
            if (!String.IsNullOrEmpty(date))
            {
                DateTime videodate = Convert.ToDateTime(date);
                return View(_repo.GetAllVideos(videodate));
            }
            return RedirectToAction("VideoManagerIndex");
        }

        [HttpPost]
        public ActionResult UploadFile(string caption, HttpPostedFileBase file)
        {
            _repo.FileUpload(caption, file);
            return Json(new { success = true, responseText = "Your message successfuly sent!" }, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeSessionServicePermission]
        [HttpPost]
        public ActionResult SubmitVideo(string location, string videoId)
        {
            if(!String.IsNullOrEmpty(location) && !String.IsNullOrEmpty(videoId))
            {
                _repo.SubmitVideo(location, videoId);
            }

            return Json(new { success = true, responseText = "Your message successfuly sent!" }, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeSessionServicePermission]
        [HttpPost]
        public ActionResult SubmitVideoForYouTube(string youTubeId, string videoId)
        {
            if (!String.IsNullOrEmpty(youTubeId) && !String.IsNullOrEmpty(videoId))
            {
                _repo.SubmitVideoForYouTube(youTubeId, videoId);
            }

            return Json(new { success = true, responseText = "Your message successfuly sent!" }, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeSessionServicePermission]
        public void DownloadFile(string id)
        {
            string FilePath = _repo.DownloadGoogleFile(id);


            Response.ContentType = "application/zip";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(FilePath));
            Response.WriteFile(System.Web.HttpContext.Current.Server.MapPath("~/GoogleDriveFiles/" + Path.GetFileName(FilePath)));
            Response.End();
            Response.Flush();
        }

        [AuthorizeSessionServicePermission]
        [HttpPost]
        public ActionResult DeleteFile(GoogleDriveFileViewModel file)
        {
            _repo.DeleteFile(file);
            return RedirectToAction("Index");
        }

        [AuthorizeSessionServicePermission]
        [HttpPost]
        public ActionResult SelfAssign(string managerId)
        {
            _repo.VideoSelfAssign(managerId);
            return Json(new { success = true, responseText = "Your message successfuly sent!" }, JsonRequestBehavior.AllowGet);
        }
    }
}