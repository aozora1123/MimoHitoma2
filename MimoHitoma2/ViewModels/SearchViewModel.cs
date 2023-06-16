using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MimoHitoma2.Models;

namespace MimoHitoma2.ViewModels
{
    public class SearchViewModel
    {
        public ImagesInfoCollection popularImages = new ImagesInfoCollection();
        public ImagesInfoCollection relatedImages = new ImagesInfoCollection();
        public UrlAnalysisModel.QueryParameter queries = new UrlAnalysisModel.QueryParameter();
        public ImageTags imageTags = new ImageTags();
        public long currentShowPageNumber { get; set; }
        public long totalNumberOfPages_correspondingQueryTags { get; set; }
        public string queryNotFondMessage { get; set; }
        public string queryString_withoutPageParemeter { get; set; }

        public SearchViewModel()
        {
            this.currentShowPageNumber = 1; // 若URL沒填寫"page"參數，則預設從第一頁開始顯示
            this.totalNumberOfPages_correspondingQueryTags = 0;
            this.queryString_withoutPageParemeter = "";
            this.queryNotFondMessage = "";
        }

        public string GetQueryValue(UrlAnalysisModel.QueryParameter parameters, string queryStr)
        {
            string value = "";
            if (parameters.count > 0)
            {
                for (int i = 0; i < parameters.count; i++)
                {
                    if (queryStr == parameters.parameter[i])
                    {
                        value = parameters.value[i];
                        break;
                    }
                }
            }
            return value;
        }
    }
}