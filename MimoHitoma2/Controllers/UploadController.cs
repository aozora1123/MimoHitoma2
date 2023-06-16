using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using MimoHitoma2.ViewModels;
using MimoHitoma2.Models;

namespace MimoHitoma2.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        //
        // GET: /Upload/
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] // will disable caching
        public ActionResult Image()
        {
            return View();
        }

        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] // will disable caching
        public ActionResult Status()
        {
            return View();
        }

        /*
         * Post後導向Upload Status頁面，透過TempData的值決定顯示內容
         *  
         * TempData的value分別代表
         * 200: 上傳成功
         * 400: 上傳失敗，稍後再試 (發生Exception)
         * 401: 未選擇要上傳的檔案
         * 402: 上傳格式錯誤
         * 403: 檔案大小超過限制
         * 404: 輸入Tag為空白 (至少需填寫一項)
         * 405: 輸入Tag不符合規定 (發生Exception)
         */

        [HttpPost]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] // will disable caching
        public ActionResult Image(HttpPostedFileBase file, UploadViewModel imageTag)
        {
            UploadModel uploadModel = new UploadModel();
            try
            {
                if (file != null)
                {
                    if (file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string extension = Path.GetExtension(fileName);

                        if ((extension != ".jpg") && (extension != ".png") & (extension != ".bmp")) // 僅接受這三種圖片檔案格式
                            TempData["UploadStatus"] = "402";
                        else if (file.ContentLength > 10000000) // Allow only files less than 10,000,000 bytes (approximately 10MB) to be uploaded.
                            TempData["UploadStatus"] = "403";
                        else if (string.IsNullOrEmpty(imageTag.imageTags))
                            TempData["UploadStatus"] = "404";
                        else
                        {
                            string uploader = User.Identity.Name.ToString();
                            uploadModel.SaveImage(file, extension, imageTag.imageTags, uploader);
                            TempData["UploadStatus"] = "200";
                        }
                    }
                }
                else
                    TempData["UploadStatus"] = "401";
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString() == "Tag欄位不符合規定，請重新輸入")
                {
                    TempData["UploadStatus"] = "405";
                }
                else
                {
                    return View("Error", new HandleErrorInfo(ex, "Post", "Search"));
                }
            }
            return RedirectToAction("Status", "Upload");
        }
    }
}