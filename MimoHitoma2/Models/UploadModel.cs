using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using MySql.Data.MySqlClient;


namespace MimoHitoma2.Models
{
    public class UploadModel : CommonUsedModel
    {
        string hostRawImagePath;
        string hostPreviewImagePath;

        public UploadModel()
        {
            this.hostRawImagePath = @"~\..\data\Images\Raw\";
            this.hostPreviewImagePath = @"~\..\data\Images\Preview\";
        }

        public void SaveImage(HttpPostedFileBase file, string extension, string userInputTags, string uploader)
        {
            try
            {
                string newFileName = CreateRandomCode(32) + extension;
                List<string> imageTags = GetImageTags(userInputTags);
                if (imageTags.Count == 0)
                    throw new Exception("Tag欄位不符合規定，請重新輸入");
                SaveRawImage(file, newFileName);
                CreatePreviewImage(newFileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void SaveRawImage(HttpPostedFileBase file, string newFileName)
        {
            string saveFullPath = this.hostRawImagePath + GetSubFolderNames(newFileName) + newFileName;
            file.SaveAs(saveFullPath);
        }

        protected void CreatePreviewImage(string newFileName)
        {
            // 利用System.Drawing.Image轉換成解析度較低的圖片，再儲存成預覽檔
            System.Drawing.Image srcImage = System.Drawing.Image.FromFile(this.hostRawImagePath + GetSubFolderNames(newFileName) + newFileName);
            // 根據來源圖片長寬，決定縮放後的圖片大小
            int srcImgWidth = srcImage.Width;
            int srcImgHeight = srcImage.Height;
            int resizeWidth, resizeHeight = 0;

            if (srcImgWidth > srcImgHeight)
            {
                resizeWidth = 153;
                resizeHeight = 88;
            }
            else if (srcImgWidth == srcImgHeight)
            {
                resizeWidth = 150;
                resizeHeight = 150;
            }
            else
            {
                resizeWidth = 120;
                resizeHeight = 150;
            }

            System.Drawing.Image zoomImage = new System.Drawing.Bitmap(resizeWidth, resizeHeight);
            System.Drawing.Graphics resizeGraphic = System.Drawing.Graphics.FromImage(zoomImage);
            resizeGraphic.DrawImage(srcImage, 0, 0, resizeWidth, resizeHeight);
            resizeGraphic.Dispose();

            string previewFileFullPath = this.hostPreviewImagePath + GetSubFolderNames(newFileName) + newFileName;
            // 根據來源，儲存成不同檔案類型
            switch (Path.GetExtension(newFileName))
            {
                case ".jpg":
                    zoomImage.Save(previewFileFullPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    break;
                case ".png":
                    zoomImage.Save(previewFileFullPath, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case ".bmp":
                    zoomImage.Save(previewFileFullPath, System.Drawing.Imaging.ImageFormat.Bmp);
                    break;
                default:
                    break;
            }
        }

        protected List<string> GetImageTags(string userInputTags)
        {
            /*
             * Rules: 
             *     Tags之間以 ',' 分隔
             *     每個Tag皆不包含空白字元，若有空白或"-"或連續'_'，則以單一 '_'取代之
             */

            List<string> titleCaseTags = new List<string>();
            char[] charSeparators = new char[] { ',' };
            string[] tags = userInputTags.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tags.Count(); i++)
                tags[i] = tags[i].Trim();
            // 將每個tags string之間的0~N個空白換成單一下底線
            for (int i = 0; i < tags.Count(); i++)
            {
                char[] charArray = tags[i].ToCharArray();
                bool nextIsContinuedSpaceChar = false;

                for (int index = 0; index < tags[i].Length; index++)
                {
                    // 第一個遇到的空白或"-"或下底線轉為'_'，之後遇到的連續空白&下底線就轉為空白，待刪除
                    if (charArray[index] == ' ' || charArray[index] == '-' || charArray[index] == '_')
                    {
                        if (!nextIsContinuedSpaceChar)
                        {
                            charArray[index] = '_';
                            nextIsContinuedSpaceChar = true;
                        }
                        else
                        {
                            charArray[index] = ' ';
                        }
                    }
                    else
                        nextIsContinuedSpaceChar = false;
                }
                string str = new string(charArray);
                str = str.Replace(" ", ""); // 去除剩餘的空白為重複空白or重複下底線

                // 統一格式為，將該tag中，每個以'_'分隔的字母的開頭轉為大寫                    
                System.Globalization.TextInfo ti = new System.Globalization.CultureInfo("en-US", false).TextInfo;
                string str_titleCase = ti.ToTitleCase(str);
                titleCaseTags.Add(str_titleCase);
            }
            return titleCaseTags;
        }

        protected void WriteToDatabase(string newFileName, List<string> imageTags, string uploader)
        {
            DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            string currentTimeStr = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            string tagsJoin = string.Join(",", imageTags.ToArray());
            string newImageID = "";
            string connectingDBStr = "ConnectToImageDB";
            string queryString = "";

            MySqlConnection conn = null;
            MySqlDataReader DataReader = null;
            MySqlCommand cmd = null;

            try
            {
                conn = ConnectToDatabase(connectingDBStr);

                for (int i = 0; i < imageTags.Count; i++)
                {
                    queryString = string.Format("select {0} from {1} where {2} = '{3}';", "tag_name", "image_tags", "tag_name", imageTags[i]);
                    cmd = new MySqlCommand(queryString, conn);
                    DataReader = cmd.ExecuteReader();
                    // 若該Tag不存在於資料庫，則寫進image_tags這個Table
                    if (!DataReader.HasRows) 
                    {
                        DisposeDataReaderAndCommand(DataReader, cmd);
                        queryString = string.Format("insert into {0} ({1}) values ('{2}');", "image_tags", "tag_name", imageTags[i]);
                        cmd = new MySqlCommand(queryString, conn);
                        DataReader = cmd.ExecuteReader();
                        DisposeDataReaderAndCommand(DataReader, cmd);
                    }
                    DisposeDataReaderAndCommand(DataReader, cmd);
                }

                // 寫進ImageInfo Table
                queryString = string.Format("insert into {0} ({1}) values ({2});",
                    "image_info", "filename,tags,uploader,upload_time,browsing_people,hotness,rank_point,rank_people",
                    ("'" + newFileName + "','" + tagsJoin + "','" + uploader + "','" + currentTimeStr + "',0,0,0,0"));
                cmd = new MySqlCommand(queryString, conn);
                DataReader = cmd.ExecuteReader();
                newImageID = cmd.LastInsertedId.ToString(); // 撈出前一筆新增的資料，自動添增的id

                DisposeDataReaderAndCommand(DataReader, cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnectionWithDatabase(conn, DataReader, cmd);
            }

            // 寫進Log Database的logs_image Table
            connectingDBStr = "ConnectToLogDB";
            conn = ConnectToDatabase(connectingDBStr);

            try
            {
                queryString = string.Format("insert into {0} ({1}) values ({2});",
                    "logs_image", "username,time,method,image_id,modified_tags",
                    "'" + uploader + "','" + currentTimeStr + "','" + "add" + "','" + newImageID + "','" + tagsJoin + "'");
                cmd = new MySqlCommand(queryString, conn);
                DataReader = cmd.ExecuteReader();
                DisposeDataReaderAndCommand(DataReader, cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnectionWithDatabase(conn, DataReader, cmd);
            }
        }
    }
}