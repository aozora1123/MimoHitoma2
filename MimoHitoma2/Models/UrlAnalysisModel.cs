using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MimoHitoma2.Models
{
    public class UrlAnalysisModel
    {
        public class QueryParameter
        {
            public List<string> parameter = new List<string>();
            public List<string> value = new List<string>();
            public int count { get; set; }

            public QueryParameter()
            {
                this.count = 0;
            }
        }

        public QueryParameter GetQueryParameters(string queryStr)
        {
            QueryParameter queryParameters = new QueryParameter();

            if (!String.IsNullOrEmpty(queryStr))
            {
                if (queryStr.IndexOf('?') != -1)
                {
                    string[] parameters = queryStr.Split('?')[1].Split('&');
                    for (int i = 0; i < parameters.Count(); i++)
                    {
                        if (parameters[i].IndexOf('=') != -1)
                        {
                            queryParameters.parameter.Add(parameters[i].Split('=')[0]);
                            queryParameters.value.Add(parameters[i].Split('=')[1]);
                            queryParameters.count += 1;
                        }
                    }
                }
            }
            return queryParameters;
        }

        public string GetQueryValue(QueryParameter parameters, string paraName)
        {
            string value = "";
            if (parameters.count > 0)
            {
                for (int i = 0; i < parameters.count; i++)
                {
                    if (paraName.ToLower() == parameters.parameter[i].ToLower())
                    {
                        value = parameters.value[i];
                        break;
                    }
                }
            }
            return value;
        }

        public List<string> GetQueryTagsList_By_URL_Format(string urlQueryTags)
        {
            List<string> tags = new List<string>();
            char[] tags_url_separators = new char[] { '+' }; // URL Format中，不同Tag的分隔符號為'+'號
            string[] url_splited_tags = urlQueryTags.Split(tags_url_separators, StringSplitOptions.RemoveEmptyEntries);

            // 對應Database寫入格式，將URL中每個tags轉成TitleCase
            for (int i = 0; i < url_splited_tags.Count(); i++)
            {
                char[] titleCase_separators = new char[] { '_' };
                string[] splited_words = url_splited_tags[i].Split(titleCase_separators, StringSplitOptions.RemoveEmptyEntries);
                string titleCase_str = "";
                System.Globalization.TextInfo ti = new System.Globalization.CultureInfo("en-US", false).TextInfo;
                for (int j = 0; j < splited_words.Count() - 1; j++)
                {
                    titleCase_str += ti.ToTitleCase(splited_words[j]) + "_";
                }
                titleCase_str += ti.ToTitleCase(splited_words[splited_words.Count() - 1]);
                url_splited_tags[i] = titleCase_str;
            }
            foreach (string tag in url_splited_tags)
                tags.Add(tag);
            return tags;
        }

        public long GetValue_ByLongFormat(string value)
        {
            long longValue = 0;
            if (!String.IsNullOrEmpty(value))
            {
                try
                {
                    longValue = Int64.Parse(value);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return longValue;
        }

        public string GetQueryString_WithoutPageParameter_ByURLFormat(string queryStr)
        {
            // 為提高相容性，統一把跳轉的URL中，page參數放在最後，以便處理
            string redirectURLFormat = "";

            if (String.IsNullOrEmpty(queryStr)) // 首頁沒任何query string的情況
                return redirectURLFormat;

            string queryStr_withoutQuestionMark = "";
            char[] char_separator = new char[] { '?' };
            string[] splited_words = queryStr.Split(char_separator, StringSplitOptions.RemoveEmptyEntries);
            queryStr_withoutQuestionMark = splited_words[0];

            if (!queryStr.ToLower().Contains("page"))
                return redirectURLFormat = queryStr_withoutQuestionMark;
            else
            {
                string[] splitURL = queryStr_withoutQuestionMark.Split(new string[] {"page="}, StringSplitOptions.RemoveEmptyEntries);
                string[] behind_other_parameters;
                if (splitURL.Count() > 1) // "page"之前還有別的參數
                {
                    redirectURLFormat += splitURL[0];
                    behind_other_parameters = splitURL[1].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    behind_other_parameters = splitURL[0].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                }

                if (behind_other_parameters.Count() > 1)
                {
                    for (int i = 1; i < behind_other_parameters.Count(); i++)
                        redirectURLFormat += behind_other_parameters[i] + "&";
                }
                // 統一刪掉URL最後的&符號，由跳轉時的href部分添加
                if (redirectURLFormat[redirectURLFormat.Count() - 1] == '&')
                    redirectURLFormat = redirectURLFormat.Remove((redirectURLFormat.Count() - 1), 1);
            }

            return redirectURLFormat;
        }

    }
}