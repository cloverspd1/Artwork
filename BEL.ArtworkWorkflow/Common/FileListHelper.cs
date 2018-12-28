namespace BEL.ArtworkWorkflow.Common
{
    using BEL.CommonDataContract;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using BEL.ArtworkWorkflow.BusinessLayer;
    using System.Text.RegularExpressions;

    /// <summary>
    /// File List Helper
    /// </summary>
    public static class FileListHelper
    {
        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        public static string BaseUrl
        {
            get
            {
                return string.Format("{0}://{1}", System.Web.HttpContext.Current.Request.Url.Scheme, System.Web.HttpContext.Current.Request.Url.Authority);
            }
        }

        /// <summary>
        /// Gets the Application Base Url.
        /// </summary>
        /// <value>
        /// The Application Base Url.
        /// </value>
        public static string ApplicatinBaseUrl
        {
            get
            {
                var request = System.Web.HttpContext.Current.Request;
                string baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, HttpRuntime.AppDomainAppVirtualPath == "/" ? string.Empty : HttpRuntime.AppDomainAppVirtualPath);
                return baseUrl;
            }
        }

        /// <summary>
        /// Generates the file bytes.
        /// </summary>
        /// <param name="fileNameList">The file name list.</param>
        /// <returns>file list</returns>
        public static List<FileDetails> GenerateFileBytes(string fileNameList)
        {
            if (fileNameList != null)
            {
                List<FileDetails> fileList = JsonConvert.DeserializeObject<List<FileDetails>>(fileNameList);
                if (fileList != null)
                {
                    fileList = fileList.Where(f => f.Status == FileStatus.New || f.Status == FileStatus.Delete).ToList();
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        if (fileList[i].FileContent == null || fileList[i].FileContent.Length == 0)
                        {
                            ////if (fileList[i].FileURL.StartsWith(FileListHelper.BaseUrl))
                            ////{
                            ////    fileList[i].FileURL = "~/" + fileList[i].FileURL.Replace(FileListHelper.BaseUrl, string.Empty).Trim('/');
                            ////}
                            if (fileList[i].FileURL.StartsWith(FileListHelper.ApplicatinBaseUrl))
                            {
                                fileList[i].FileURL = "~/" + fileList[i].FileURL.Replace(FileListHelper.ApplicatinBaseUrl, string.Empty).Trim('/');
                            }

                            if (fileList[i].FileURL.Contains("/Uploads/") || fileList[i].FileURL.Contains("/Sample/"))
                            {
                                fileList[i].FileContent = FileListHelper.DownloadFileBytes(fileList[i].FileURL);
                            }
                            else
                            {
                                fileList[i].FileContent = CommonBusinessLayer.Instance.DownloadFile(fileList[i].FileURL, "Artworks");
                            }                            
                        }
                    }
                    fileList.RemoveAll(f => f.FileContent == null && f.Status != FileStatus.Delete);
                }
                return fileList;
            }
            else
            {
                return new List<FileDetails>();
            }
        }

        /// <summary>
        /// Generates the file bytes.
        /// </summary>
        /// <param name="fileNameList">The file name list.</param>
        /// <param name="ModelName">The Model Name.</param>
        /// <param name="artworkTypeCode">The artwork Type Code.</param>
        /// <returns>file list</returns>
        public static List<FileDetails> GenerateFileBytesForABSQTeam(string fileNameList, string modelName, string artworkTypeCode)
        {
            modelName = string.IsNullOrWhiteSpace(modelName) ? string.Empty : modelName;
            if (fileNameList != null)
            {
                List<FileDetails> fileList = JsonConvert.DeserializeObject<List<FileDetails>>(fileNameList);
                if (fileList != null)
                {
                    if (fileList.Where(f => f.Status == FileStatus.NoAction).Count() == 0)
                    {
                        fileList = fileList.Where(f => f.Status == FileStatus.New || f.Status == FileStatus.Delete).ToList();
                        for (int i = 0; i < fileList.Count; i++)
                        {
                            if (fileList[i].FileContent == null || fileList[i].FileContent.Length == 0)
                            {
                                ////if (fileList[i].FileURL.StartsWith(FileListHelper.BaseUrl))
                                ////{
                                ////    fileList[i].FileURL = "~/" + fileList[i].FileURL.Replace(FileListHelper.BaseUrl, string.Empty).Trim('/');
                                ////}
                                if (fileList[i].FileURL.StartsWith(FileListHelper.ApplicatinBaseUrl))
                                {
                                    fileList[i].FileURL = "~/" + fileList[i].FileURL.Replace(FileListHelper.ApplicatinBaseUrl, string.Empty).Trim('/');
                                }

                                if (fileList[i].FileURL.Contains("/Uploads/") || fileList[i].FileURL.Contains("/Sample/"))
                                {
                                    fileList[i].FileContent = FileListHelper.DownloadFileBytes(fileList[i].FileURL);
                                }
                                else
                                {
                                    fileList[i].FileContent = CommonBusinessLayer.Instance.DownloadFile(fileList[i].FileURL, "Artworks");
                                }
                                if (fileList[i].Status == FileStatus.New)
                                {
                                    fileList[i].FileName = artworkTypeCode + "_" + modelName.Trim() + "_" + DateTime.Now.ToString("ddMMyy") + System.IO.Path.GetExtension(fileList[i].FileName);
                                }
                            }
                        }
                        fileList.RemoveAll(f => f.FileContent == null && f.Status != FileStatus.Delete);
                    }
                    else
                    {
                        FileDetails fl = new FileDetails();
                        for (int i = 0; i < fileList.Count; i++)
                        {
                            if (fileList[i].FileContent == null || fileList[i].FileContent.Length == 0)
                            {
                                if (fileList[i].FileURL.StartsWith(FileListHelper.ApplicatinBaseUrl))
                                {
                                    fileList[i].FileURL = "~/" + fileList[i].FileURL.Replace(FileListHelper.ApplicatinBaseUrl, string.Empty).Trim('/');
                                }
                                fl.FileURL = fileList[i].FileURL;
                                fl.Status = FileStatus.New;
                                fl.FileContent = CommonBusinessLayer.Instance.DownloadFile(fileList[i].FileURL, "Artworks");
                                string[] lastDate = System.IO.Path.GetFileNameWithoutExtension(fileList[i].FileName).Split('_');

                                fl.FileName = artworkTypeCode + "_" + modelName.Trim() + "_" + lastDate[lastDate.Length - 1] + System.IO.Path.GetExtension(fileList[i].FileName);
                                fileList[i].Status = FileStatus.Delete;
                        }
                        }
                        fileList.Add(fl);
                    }
                }
                return fileList;
            }
            else
            {
                return new List<FileDetails>();
            }
        }

        /// <summary>
        /// Gets the file names.
        /// </summary>
        /// <param name="fileNameList">The file name list.</param>
        /// <param name="Model">The Model.</param>
        /// <param name="artworkTypeCode">The artwork Type Code.</param>
        /// <returns>List of filenames</returns>
        public static List<string> GetFileNamesForABSQTeam(string fileNameList, string model, string artworkTypeCode)
        {
            if (fileNameList != null)
            {
                List<FileDetails> fileList = JsonConvert.DeserializeObject<List<FileDetails>>(fileNameList);
                if (fileList != null)
                {
                    fileList = fileList.Where(p => p.Status != FileStatus.Delete).ToList();
                    List<string> str = new List<string>();
                    foreach (var item in fileList)
                    {
                        if (item.Status == FileStatus.New)
                        {
                            model = Regex.Replace(model, @"\t|\n|\r", " ");
                            str.Add(artworkTypeCode + "_" + model.Trim() + "_" + DateTime.Now.ToString("ddMMyy") + System.IO.Path.GetExtension(item.FileName));
                        }
                        else
                        {
                            string[] lastDate = System.IO.Path.GetFileNameWithoutExtension(item.FileName).Split('_');
                            str.Add(artworkTypeCode + "_" + model.Trim() + "_" + lastDate[lastDate.Length - 1] + System.IO.Path.GetExtension(item.FileName));
                        }
                    }
                    return str;
                }
            }
            return new List<string>();
        }

        /// <summary>
        /// Gets the file names.
        /// </summary>
        /// <param name="fileNameList">The file name list.</param>
        /// <returns>List of filenames</returns>
        public static List<string> GetFileNames(string fileNameList)
        {
            if (fileNameList != null)
            {
                List<FileDetails> fileList = JsonConvert.DeserializeObject<List<FileDetails>>(fileNameList);
                if (fileList != null)
                {
                    return fileList.Where(p => p.Status != FileStatus.Delete).Select(x => x.FileName).ToList();
                }
            }
            return new List<string>();
        }

        /// <summary>
        /// Downloads the file bytes.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>File Bytes</returns>
        public static byte[] DownloadFileBytes(string url)
        {
            try
            {
                return System.IO.File.ReadAllBytes(System.Web.HttpContext.Current.Server.MapPath(url));
            }
            catch
            {
                return null;
            }
        }
    }
}