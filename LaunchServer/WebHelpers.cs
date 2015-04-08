using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace LaunchServer
{
    public class WebHelpers
    {
        private static string GetMD5FromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
            }
        }

        public static Dictionary<string, string> GetAllFilesInfo(string directoryPath)
        {
            DirectoryInfo clientDirect = new DirectoryInfo(directoryPath);

            var files = clientDirect.GetFiles().ToList();

       
            //读取JSON配置文件
            var jsonFile = new FileInfo(HttpContext.Current.Server.MapPath("~/UpgradeConfig.json"));

            var jsonStr = jsonFile.OpenText().ReadToEnd();

            var filesObj = JsonConvert.DeserializeObject<UpgradeFileInfo>(jsonStr);

            //转换为大写后分割
            var ignoreExtensions = filesObj.ExtensionToIgnore;
            var ingoreFiles = filesObj.FileToIgnore;

            //过滤需要忽略的文件和后缀名
            files = files.Where(m => filesObj.fileNames.Exists(fileName => fileName.ToUpper().Contains(m.Name.ToUpper())) && !ingoreFiles.Contains(m.Name.ToUpper()) && !ignoreExtensions.Contains(m.Extension.ToUpper())).ToList();

            Dictionary<string, string> fileDict = new Dictionary<string, string>();

            files.ForEach(m => fileDict.Add(m.FullName.Remove(0, directoryPath.Length + 1), GetMD5FromFile(m.FullName)));

            return fileDict;
        }

        internal class UpgradeFileInfo
        {
            //需要更新的文件,模糊
            public List<String> fileNames { get; set; }
            //需要更新的文件中需要忽略的后缀名
            public List<String> ExtensionToIgnore { get; set; }
            //需要更新的文件中需要忽略的文件名
            public List<String> FileToIgnore { get; set; }
        }
    }
}