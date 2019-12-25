using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using AutoMapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using PaloVideoManager.DataLayer;
using PaloVideoManager.Models.ViewModel;
using static PaloVideoManager.BusinessLayer.Common;
using System.Data.Entity;

namespace PaloVideoManager.BusinessLayer
{
    public class VideoRepository
    {
        public static string[] Scopes = { DriveService.Scope.Drive };
        Common cm = new Common();
        ReporterEntities _context = new ReporterEntities();

        public void DeleteFile(GoogleDriveFileViewModel file)
        {
            DriveService service = GetService();
            try
            {
                // Initial validation.
                if (service == null)
                    throw new ArgumentNullException("service");

                if (file == null)
                    throw new ArgumentNullException(file.Id);

                // Make the request.
                service.Files.Delete(file.Id).Execute();

                VideoSummary fileToDelete = _context.VideoSummaries.Single(v => v.GoogleDriveId == file.Id);
                _context.VideoSummaries.Remove(fileToDelete);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Request Files.Delete failed.", ex);
            }
        }

        public void SubmitVideo(string location, string videoId)
        {
            int managerId = Convert.ToInt32(videoId);
            VideoManager video = _context.VideoManagers.Single(v => v.VideoManagerId == managerId);
            video.Status = Enum.GetName(typeof(VideoManagerStatus), 3);
            video.CompletedTime = DateTime.Now;
            video.LocalCompletedFilePath = location;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                //
            }
        }

        public void SubmitVideoForYouTube(string youTubeId, string videoId)
        {
            int managerId = Convert.ToInt32(videoId);
            VideoManager video = _context.VideoManagers.Single(v => v.VideoManagerId == managerId);
            video.Status = Enum.GetName(typeof(VideoManagerStatus), 4);
            video.CompletedTime = DateTime.Now;
            video.YoutubeId = youTubeId;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                //
            }
        }

        public string DownloadGoogleFile(string fileId)
        {
            DriveService service = GetService();

            string FolderPath = System.Web.HttpContext.Current.Server.MapPath("/GoogleDriveFiles/");
            FilesResource.GetRequest request = service.Files.Get(fileId);

            string FileName = request.Execute().Name;
            string FilePath = System.IO.Path.Combine(FolderPath, FileName);

            MemoryStream stream1 = new MemoryStream();

            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            //Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            //Console.WriteLine("Download complete.");
                            SaveStream(stream1, FilePath);
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            //Console.WriteLine("Download failed.");
                            break;
                        }
                }
            };
            request.Download(stream1);
            return FilePath;
        }

        public void VideoSelfAssign(string managerId)
        {
            int videoManagerId = Convert.ToInt32(managerId);
            VideoManager manager = _context.VideoManagers.Single(m => m.VideoManagerId == videoManagerId);
            manager.Status = Enum.GetName(typeof(VideoManagerStatus), 2);
            manager.TakenByUserId = Convert.ToInt32(HttpContext.Current.Session["UserId"]);
            manager.TakenByTime = DateTime.Now;
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                
            }
        }

        public void FileUpload(string videoCaption, HttpPostedFileBase file)
        {
            string gdId = "";
            string streamingLink = "";
            string pathDelete = string.Empty;
            #region new
            List<string> parents = new List<string>();
            //file.FileName = HttpContext.Current.User.Identity.Name + DateTime.Now.ToString("yyyy") +
            //                    DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") +
            //                    "_" + Path.GetFileName(file.FileName).Replace(",", string.Empty).ToString();
            string destfilename = HttpContext.Current.User.Identity.Name + DateTime.Now.ToString("yyyy") +
                                DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") +
                                "_" + Path.GetFileName(file.FileName).Replace(",", string.Empty).ToString();
            parents.Add("root");
            // Prepare the JSON metadata
            string json = "{\"name\":\"" + destfilename + "\"";
            if (parents.Count > 0)
            {
                json += ", \"parents\": [";
                foreach (string parent in parents)
                {
                    json += "\"" + parent + "\", ";
                }
                json = json.Remove(json.Length - 2) + "]";
            }
            json += "}";
            Debug.WriteLine(json);

            Google.Apis.Drive.v3.Data.File uploadedFile = null;
            #endregion

            if (file != null && file.ContentLength > 0)
            {
                DriveService service = GetService();

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/GoogleDriveFiles"),
                Path.GetFileName(file.FileName));
                file.SaveAs(path);
                try
                {
                    System.IO.FileInfo info = new System.IO.FileInfo(path);

                    ulong fileSize = (ulong)info.Length;

                    var uploadStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                    var insert = service.Files.Create(new Google.Apis.Drive.v3.Data.File {
                                Name = HttpContext.Current.User.Identity.Name + DateTime.Now.ToString("yyyy") +
                                DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") +
                                "_" + Path.GetFileName(file.FileName).Replace(",", string.Empty).ToString(),
                        Parents = new List<string> { "root" } }, uploadStream, "application/octet-stream");

                    Uri uploadUri = insert.InitiateSessionAsync().Result;

                    int chunk_size = ResumableUpload.MinimumChunkSize;
                    int bytesSent = 0;
                    while (uploadStream.Length != uploadStream.Position)
                    {
                        byte[] temp = new byte[chunk_size];
                        int cnt = uploadStream.Read(temp, 0, temp.Length);
                        if (cnt == 0)
                            break;

                        HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(uploadUri);

                        httpRequest.Method = "PUT";
                        httpRequest.Headers["Authorization"] = "Bearer " + ((UserCredential)service.HttpClientInitializer).Token.AccessToken;
                        httpRequest.ContentLength = (long)cnt;
                        httpRequest.Headers["Content-Range"] = string.Format("bytes {0}-{1}/{2}", bytesSent, bytesSent + cnt - 1, fileSize);

                        using (System.IO.Stream requestStream = httpRequest.GetRequestStreamAsync().Result)
                        {
                            requestStream.Write(temp, 0, cnt);
                        }

                        HttpWebResponse httpResponse;
                        try
                        {
                            httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                        }
                        catch (WebException ex)
                        {
                            httpResponse = (HttpWebResponse)ex.Response;
                        }

                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                        {

                        }
                        else if ((int)httpResponse.StatusCode != 308)
                            break;

                        bytesSent += cnt;

                        Debug.WriteLine("Uploaded " + bytesSent.ToString());

                        if (bytesSent == uploadStream.Length)
                        {
                            FilesResource.ListRequest request = service.Files.List();
                            request.Fields = "nextPageToken, files(id, name, size, version, trashed, createdTime, webViewLink)";
                            if (parents.Count > 0)
                                request.Q += "'" + parents[0] + "' in parents and ";
                            request.Q += "name = '" + destfilename + "'";
                            FileList result = request.Execute();
                            if (result.Files.Count > 0)
                            {
                                gdId = result.Files[0].Id;
                                streamingLink = result.Files[0].WebViewLink;
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(gdId))
                    {
                        if (!String.IsNullOrEmpty(gdId))
                        {
                            VideoSummary newVideo = new VideoSummary();
                            newVideo.VideoName = Path.GetFileName(file.FileName).Replace(",", string.Empty).ToString();
                            string fileName = HttpContext.Current.User.Identity.Name + DateTime.Now.ToString("yyyy") + 
                                DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") + 
                                "_" + Path.GetFileName(file.FileName).Replace(",", string.Empty).ToString();
                            newVideo.VideoPath = fileName;
                            newVideo.GoogleDriveId = gdId;
                            newVideo.VideoDate = DateTime.Now;
                            newVideo.EntryDate = DateTime.Now;
                            newVideo.GoogleStreamingLink = streamingLink;

                            newVideo.VideoSize = file.ContentLength.ToString();

                            newVideo.UserId = Convert.ToInt32(HttpContext.Current.Session["UserId"].ToString());
                            newVideo.CenterCode = Convert.ToInt32(HttpContext.Current.Session["CenterCode"].ToString());
                            newVideo.Description = videoCaption;
                            _context.VideoSummaries.Add(newVideo);

                            try
                            {
                                _context.SaveChanges();

                                VideoManager newVideoManager = new VideoManager();
                                newVideoManager.VideoSummaryId = newVideo.Id;
                                newVideoManager.Status = Enum.GetName(typeof(VideoManagerStatus), 1);
                                _context.VideoManagers.Add(newVideoManager);
                                _context.SaveChanges();

                            }
                            catch (Exception ex)
                            {
                                //
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public List<GoogleDriveFileViewModel> GetDriveFiles()
        {
            DriveService service = GetService();
            // Define parameters of request.
            FilesResource.ListRequest FileListRequest = service.Files.List();

            //listRequest.PageSize = 10;
            //listRequest.PageToken = 10;
            FileListRequest.Fields = "nextPageToken, files(id, name, size, version, trashed, createdTime, webViewLink)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            List<GoogleDriveFileViewModel> FileList = new List<GoogleDriveFileViewModel>();

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    GoogleDriveFileViewModel File = new GoogleDriveFileViewModel
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime,
                        StreamingLink = file.WebViewLink
                    };
                    FileList.Add(File);
                }
            }
            return FileList;
        }

        public DriveService GetService()
        {
            UserCredential credential;
            using (var stream = new FileStream(@"F:\Projects\PaloVideoManager\credentials.json", FileMode.Open, FileAccess.Read))
            {
                String FolderPath = @"F:\Projects\PaloVideoManager\";
                String FilePath = Path.Combine(FolderPath, "DriveServiceCredentials.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;
            }

            //Create Drive API service.
            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveRestAPI-v3",
            });

            return service;
        }

        public void SaveStream(MemoryStream stream, string FilePath)
        {
            using (System.IO.FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                stream.WriteTo(file);
            }
        }

        public List<VideoListViewModel> GetUserVideoList(DateTime date)
        {
            int userId = Convert.ToInt32(HttpContext.Current.Session["UserId"].ToString());
            List<VideoSummary> dbList = _context.VideoSummaries.ToList().
                Where(v => v.UserId == userId && !String.IsNullOrEmpty(v.GoogleDriveId) && Convert.ToDateTime(v.EntryDate).Date == date.Date)
                .OrderByDescending(m => m.EntryDate).ToList();
            List<VideoListViewModel> viewList = Mapper.Map< List < VideoSummary > ,List <VideoListViewModel>>(dbList);
            return viewList;

        }

        public List<VideoManagerViewModel> GetAllVideos(DateTime date)
        {
            List<GoogleDriveFileViewModel> driveList = GetDriveFiles();
            List<VideoManager> dbList = _context.VideoManagers.Include(c => c.VideoSummary).ToList()
                .Where(w => !String.IsNullOrEmpty(w.VideoSummary.GoogleDriveId) && Convert.ToDateTime(w.VideoSummary.EntryDate).Date == date.Date)
                .OrderByDescending(o => o.VideoSummary.EntryDate.Value.TimeOfDay).ToList();

            List<VideoManagerViewModel> returnList = Mapper.Map<List<VideoManager>, List<VideoManagerViewModel>>(dbList);
            foreach (var item in returnList)
            {
                if(item.TakenByUserId > 0)
                {
                    tbl_User user = _context.tbl_User.Single(u => u.UserId == item.TakenByUserId);
                    item.TakenByName = user.UserName;
                }
                
                tbl_User VideoUser = _context.tbl_User.Single(u => u.UserId == item.VideoSummary.UserId);
                item.VideoSummary.SubmittedByUserName = VideoUser.UserName;
                
            }
            return returnList;
        }
    }
}