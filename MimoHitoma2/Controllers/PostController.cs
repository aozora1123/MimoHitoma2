using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MimoHitoma2.ViewModels;
using MimoHitoma2.Models;

namespace MimoHitoma2.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        public ActionResult Search()
        {
            ImagesModel images = new ImagesModel();
            SearchViewModel searchVM = new SearchViewModel();
            UrlAnalysisModel urlAnalysor = new UrlAnalysisModel();
            try
            {
                images.queries = urlAnalysor.GetQueryParameters(Request.Url.Query);
                searchVM.queries = images.queries;

                string queryTags = urlAnalysor.GetQueryValue(images.queries, "tags");
                List<string> query_tags_list = urlAnalysor.GetQueryTagsList_By_URL_Format(queryTags);
                string pageStr = urlAnalysor.GetQueryValue(images.queries, "page");
                long queryPageNumber = urlAnalysor.GetValue_ByLongFormat(pageStr);
                if (queryPageNumber == 0) 
                    queryPageNumber = 1; //預設從第一頁開始顯示

                searchVM.popularImages = images.GetSearchImagesInfo(ImagesModel.DealImageRegion.popularImageRegion, query_tags_list, queryPageNumber);
                searchVM.imageTags.AddNewTags_WithDuplication(searchVM.popularImages.all_tags_with_duplication);

                searchVM.relatedImages = images.GetSearchImagesInfo(ImagesModel.DealImageRegion.relatedImageRegion, query_tags_list, queryPageNumber);
                searchVM.imageTags.AddNewTags_WithDuplication(searchVM.relatedImages.all_tags_with_duplication);

                // 將popular images和related iages的所有tags，優先判斷是否為query tags、其次判斷出現總數(多到少)，進行排序
                if (!String.IsNullOrEmpty(searchVM.imageTags.all_tags_with_duplication))
                {
                    searchVM.imageTags.Sort_And_Dedup_Tags(urlAnalysor.GetQueryTagsList_By_URL_Format(queryTags));
                }
                // 計算換頁資訊
                searchVM.queryString_withoutPageParemeter = urlAnalysor.GetQueryString_WithoutPageParameter_ByURLFormat(Request.Url.Query);
                searchVM.currentShowPageNumber = queryPageNumber;
                searchVM.totalNumberOfPages_correspondingQueryTags = (searchVM.relatedImages.totalNumberOfCorrespondingQueryTags / images.showRelatedImagesNumbersPerPage);
                if ((searchVM.relatedImages.totalNumberOfCorrespondingQueryTags % images.showRelatedImagesNumbersPerPage) > 0)
                    searchVM.totalNumberOfPages_correspondingQueryTags += 1;

                return View(searchVM);
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Post", "Search"));
            }
        }

        public ActionResult Show()
        {
            ImagesModel images = new ImagesModel();
            ImageTags imageTags = new ImageTags();
            ShowViewModel showVM = new ShowViewModel();
            UrlAnalysisModel urlAnalysor = new UrlAnalysisModel();

            try
            {
                images.queries = urlAnalysor.GetQueryParameters(Request.Url.Query);
                showVM.queries = images.queries;

                if (images.queries.count > 0)
                {
                    if (!String.IsNullOrEmpty(urlAnalysor.GetQueryValue(images.queries, "id")))
                    {
                        string idStr = urlAnalysor.GetQueryValue(images.queries, "id");
                        long queryID = urlAnalysor.GetValue_ByLongFormat(idStr);

                        if (queryID > 0) // Database中，id的auto increment由1開始遞增
                        {
                            try
                            {
                                showVM.postImage = images.GetShowImageInfo(queryID);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Post", "Search"));
            }
            return View(showVM);
        }
    }
}