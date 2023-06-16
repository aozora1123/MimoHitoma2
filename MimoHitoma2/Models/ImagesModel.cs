using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace MimoHitoma2.Models
{
    public class ImagesInfoCollection // 用於產生/顯示多張預覽圖  ( Post / Search Action )
    {
        public List<string> filename = new List<string>();
        public List<string> href = new List<string>();
        public List<string> previewSrc = new List<string>();
        public List<string> previewTitle = new List<string>();
        public string all_tags_with_duplication { get; set; }
        public long totalNumberOfCorrespondingQueryTags { get; set; }
        public int count { get; set; }

        public ImagesInfoCollection()
        {
            this.count = 0;
            this.totalNumberOfCorrespondingQueryTags = 0;
            this.all_tags_with_duplication = "";
        }
    }

    public class PostImage  // 用於產生/顯示單張原始圖片 ( Post / Show Action )
    {
        public string href { get; set; }
        public string src { get; set; }
        public ImageTags imageTags = new ImageTags();

        public PostImage()
        {
            this.href = "";
            this.src = "";
        }
    }

    public class ImageTags
    {
        public List<string> all_tags_list_without_duplication = new List<string>(); // 為該畫面中所有出現的tags，包含popular images和related images的tags，並且不重複出現
        public string all_tags_with_duplication { get; set; }

        public ImageTags()
        {
            all_tags_with_duplication = "";
        }

        public void AddNewTags_WithDuplication(string new_tags)
        {
            this.all_tags_with_duplication += new_tags + ",";
        }

        public List<string> GetTagsList_WithDuplication(string tags)
        {
            List<string> tagList_with_duplication = new List<string>();
            char[] tags_char_separators = new char[] { ',' };
            string[] splited_tags = tags.Split(tags_char_separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string tag in splited_tags)
                tagList_with_duplication.Add(tag);

            return tagList_with_duplication;
        }

        public List<string> SortAndDeduplicateInputTags_By_TagsCountAndQueryTags(List<string> duplicatedTags, List<string> queryTags)
        {
            // 計算該頁中的所有Images的Tags(包含重複tag)次數，再去除重複Tags，並依照出現次數由高到低排序

            List<string> tags_without_duplication = new List<string>();
            List<int> tags_count = new List<int>();
            try
            {
                #region 將所有的Tags(包含重複的tag)計數，並挑選出不重複的tags
                foreach (string tag in duplicatedTags)
                {
                    if (!tags_without_duplication.Contains(tag))
                    {
                        tags_without_duplication.Add(tag);
                        tags_count.Add(1);
                    }
                    else
                    {
                        int index = tags_without_duplication.IndexOf(tag); // 指向當前tag位於list array中的位置
                        tags_count[index] += 1;
                    }
                }
                #endregion

                #region Sort， Query的Tag優先顯示，其餘的依Count次數由大到小依序顯示
                if (tags_without_duplication.Count > 1)
                {
                    string tempStr = "";
                    int tempCount = 0;
                    for (int i = 0; i < tags_without_duplication.Count; i++)
                    {
                        for (int j = 0; j < tags_without_duplication.Count - i - 1; j++)
                        {
                            if (tags_count[j] < tags_count[j + 1])
                            {
                                tempStr = tags_without_duplication[j];
                                tags_without_duplication[j] = tags_without_duplication[j + 1];
                                tags_without_duplication[j + 1] = tempStr;
                                tempCount = tags_count[j];
                                tags_count[j] = tags_count[j + 1];
                                tags_count[j + 1] = tempCount;
                            }
                        }
                    }

                    // 將Query的Tag，提到List Array的最前面
                    if ((queryTags != null) && (queryTags.Count > 0))
                    {
                        for (int i = 0; i < queryTags.Count; i++)
                        {
                            int indexOfQueryTag = tags_without_duplication.IndexOf(queryTags[i]);
                            tempStr = tags_without_duplication[indexOfQueryTag];
                            tempCount = tags_count[indexOfQueryTag];

                            for (int j = indexOfQueryTag; j > i + 0; j--)
                            {
                                tags_without_duplication[j] = tags_without_duplication[j - 1];
                                tags_count[j] = tags_count[j - 1];
                            }
                            tags_without_duplication[i] = tempStr;
                            tags_count[i] = tempCount;
                        }
                    }
                }
                #endregion
                return tags_without_duplication;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Sort_And_Dedup_Tags(List<string> queryTags)
        {
            List<string> newList = GetTagsList_WithDuplication(this.all_tags_with_duplication);
            this.all_tags_list_without_duplication = SortAndDeduplicateInputTags_By_TagsCountAndQueryTags(newList, queryTags);
        }

    }


    // 用於處理&傳遞 Post/Search 和 Post/Show所需之架構及功能
    public class ImagesModel : CommonUsedModel
    {

        public enum ImageType { previewImageType, rawImageType };
        public enum DealImageRegion { popularImageRegion, relatedImageRegion };

        public string connectingDBStr { get; set; }
        public string rawImagePath { get; set; }
        public string previewImagePath { get; set; }
        public int showPopularImagesNumbers { get; set; }
        public int showRelatedImagesNumbersPerPage { get; set; }
        public UrlAnalysisModel.QueryParameter queries = new UrlAnalysisModel.QueryParameter();

        public ImagesModel()
        {
            this.connectingDBStr = "";
            this.showPopularImagesNumbers = 4;
            this.showRelatedImagesNumbersPerPage = 20;
            this.rawImagePath = @"\data\Images\Raw\";
            this.previewImagePath = @"\data\Images\Preview\";
        }

        public ImagesInfoCollection GetSearchImagesInfo(DealImageRegion dealImageRegion, List<string> searchTags, long pageNumber)
        {
            /*
             * 每次執行為搜尋popular images region或related images region的結果
             * 若searchTags為空字串，則搜尋所有圖片，以及所有圖片中最熱門的當popular images
             * 若searchTags不為空字串，則以該searchTags中最熱門的圖片當popular images
             */
            ImagesInfoCollection queryImagesCollection = new ImagesInfoCollection();

            connectingDBStr = "ConnectToImageDB";
            MySqlConnection conn = null;
            MySqlDataReader DataReader = null;
            MySqlCommand cmd = null;
            string queryCommand = "";
            string sqlCondition_joinComplexTags = "";
            ImageTags imageTags = new ImageTags();
            try
            {
                conn = ConnectToDatabase(connectingDBStr);

                // 撈出searchTags條件或全部的圖片中，hotness最高的圖片。
                #region 確認query的tags，是否都存在於imageTags的table裡
                if (searchTags.Count > 0)
                {
                    for (int i = 0; i < searchTags.Count(); i++)
                    {
                        queryCommand = string.Format("select {0} from {1} where {2}='{3}';", "tag_name", "image_tags", "tag_name", searchTags[i]);
                        cmd = new MySqlCommand(queryCommand, conn);
                        DataReader = cmd.ExecuteReader();

                        if (DataReader.HasRows)
                        {
                            if (i != 0) //同時搜尋超過一個參數時，將結果做交集
                                sqlCondition_joinComplexTags += " and ";

                            sqlCondition_joinComplexTags += "( ";
                            sqlCondition_joinComplexTags += "tags = '" + searchTags[i] + "'";  
                            sqlCondition_joinComplexTags += " or tags like '" + searchTags[i] + ",%'";
                            sqlCondition_joinComplexTags += " or tags like '%," + searchTags[i] + "'";
                            sqlCondition_joinComplexTags += " or tags like '%," + searchTags[i] + ",%'";
                            sqlCondition_joinComplexTags += " )";
                        }
                        else
                            throw new Exception("資料庫中不包含 tag: " + searchTags[i]);
                        cmd.Dispose();
                        DataReader.Dispose();
                    }
                }
                #endregion

                if (pageNumber <= 0)
                {
                    throw new Exception("查詢不到 page: " + pageNumber + " 的相關資料");
                }

                #region 先找出符合search tags的所有圖片總數
                if (searchTags.Count == 0) // 首頁
                {
                    queryCommand = string.Format("select count({0}) from {1};", "image_id", "image_info");
                }
                else
                {
                    string tagsJoinStr = String.Join(",", searchTags);
                    queryCommand = string.Format("select count({0}) from {1} where {2};",
                        "image_id", "image_info", sqlCondition_joinComplexTags);
                }
                cmd = new MySqlCommand(queryCommand, conn);
                DataReader = cmd.ExecuteReader();
                if (DataReader.HasRows)
                {
                    DataReader.Read();
                    queryImagesCollection.totalNumberOfCorrespondingQueryTags = DataReader.GetInt64(0);
                    cmd.Dispose();
                    DataReader.Dispose();
                }
                #endregion

                #region 再找出要顯示的頁數的圖片
                if (dealImageRegion == DealImageRegion.popularImageRegion)
                {
                    if (searchTags.Count == 0)
                    {
                        queryCommand = string.Format("select {0} from {1} order by {2} desc,{3} desc limit {4};",
                            "image_id,filename,tags", "image_info", "hotness", "upload_time", showPopularImagesNumbers);
                    }
                    else
                    {
                        queryCommand = string.Format("select {0} from {1} where {2} order by {3} desc,{4} desc limit {5};",
                            "image_id,filename,tags", "image_info", sqlCondition_joinComplexTags, "hotness", "upload_time", showPopularImagesNumbers);
                    }
                }
                else if (dealImageRegion == DealImageRegion.relatedImageRegion)
                {
                    if (searchTags.Count == 0)
                    {
                        queryCommand = string.Format("select {0} from {1} order by {2} desc limit {3},{4};",
                            "image_id,filename,tags", "image_info", "upload_time", (pageNumber - 1) * showRelatedImagesNumbersPerPage, showRelatedImagesNumbersPerPage);
                    }
                    else
                    {
                        queryCommand = string.Format("select {0} from {1} where {2} order by {3} desc limit {4},{5};",
                            "image_id,filename,tags", "image_info", sqlCondition_joinComplexTags, "upload_time", (pageNumber - 1) * showRelatedImagesNumbersPerPage, showRelatedImagesNumbersPerPage); // SQL中limit資料從第0筆開始算起
                    }
                }

                cmd = new MySqlCommand(queryCommand, conn);
                DataReader = cmd.ExecuteReader();

                if (DataReader.HasRows)
                {
                    while (DataReader.Read())
                    {
                        string filename = DataReader["filename"].ToString();
                        queryImagesCollection.filename.Add(filename);
                        queryImagesCollection.href.Add(GetImageHref(Int64.Parse(DataReader["image_id"].ToString()), filename, ImageType.previewImageType));
                        queryImagesCollection.previewSrc.Add(GetImageSrc(DataReader["filename"].ToString(), ImageType.previewImageType));
                        queryImagesCollection.previewTitle.Add(DataReader["tags"].ToString());
                        queryImagesCollection.all_tags_with_duplication += DataReader["tags"].ToString() + ","; // 不同圖片的tags之間，以","分隔
                        queryImagesCollection.count += 1;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnectionWithDatabase(conn, DataReader, cmd);
            }
            return queryImagesCollection;
        }

        public PostImage GetShowImageInfo(long id)
        {
            PostImage post = new PostImage();
            connectingDBStr = "ConnectToImageDB";
            MySqlConnection conn = null;
            MySqlDataReader DataReader = null;
            MySqlCommand cmd = null;
            string queryCommand = "";

            try
            {
                conn = ConnectToDatabase(connectingDBStr);

                if (id > 0)
                {
                    queryCommand = string.Format("select {0} from {1} where {2} = {3};",
                        "image_id,filename,tags,browsing_people,hotness", "image_info", "image_id", id);
                    cmd = new MySqlCommand(queryCommand, conn);
                    DataReader = cmd.ExecuteReader();

                    if (DataReader.HasRows)
                    {
                        DataReader.Read();
                        string filename = DataReader["filename"].ToString();
                        post.href = GetImageHref(Int64.Parse(DataReader["image_id"].ToString()), filename, ImageType.rawImageType);
                        post.src = GetImageSrc(DataReader["filename"].ToString(), ImageType.rawImageType);
                        post.imageTags.all_tags_with_duplication += DataReader["tags"].ToString();
                        post.imageTags.Sort_And_Dedup_Tags(null);
                        // 將瀏覽人次 & hotness  各 + 1
                        long browsing_people = Int64.Parse(DataReader["browsing_people"].ToString());
                        long hotness = Int64.Parse(DataReader["hotness"].ToString());
                        cmd.Dispose();
                        DataReader.Dispose();

                        queryCommand = string.Format("update {0} set {1}={2}, {3}={4} where {5}={6};",
                            "image_info", "browsing_people", (browsing_people + 1).ToString(), "hotness", (hotness + 1).ToString(), "image_id", id);
                        cmd = new MySqlCommand(queryCommand, conn);
                        cmd.ExecuteReader();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnectionWithDatabase(conn, DataReader, cmd);
            }
            return post;
        }


        public string GetImageHref(long id, string filename, ImageType type)
        {
            string href = "";

            if (type == ImageType.previewImageType)
                href += "/MimoHitoma2/Post/Show?id=" + id.ToString();
            else if (type == ImageType.rawImageType)
                href += this.rawImagePath + GetSubFolderNames(filename) + filename;

            return href;
        }

        public string GetImageSrc(string filename, ImageType type)
        {
            string src = "";

            if (type == ImageType.previewImageType)
                src += this.previewImagePath;
            else if (type == ImageType.rawImageType)
                src += this.rawImagePath;

            src += GetSubFolderNames(filename);
            src += filename;

            return src;
        }
    }
}