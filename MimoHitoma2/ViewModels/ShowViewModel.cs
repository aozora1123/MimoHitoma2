using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MimoHitoma2.Models;

namespace MimoHitoma2.ViewModels
{
    public class ShowViewModel
    {
        public PostImage postImage = new PostImage();
        public UrlAnalysisModel.QueryParameter queries { get; set; }
        public ImageTags imageTags = new ImageTags();

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