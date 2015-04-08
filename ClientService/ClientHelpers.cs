using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClientService
{
    public class ClientHelpers
    {
        private const string CONFIGEXEPATH = "";
        public Configuration Configuration = null;
        private ClientHelpers()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Client.exe.config");

            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = configPath;

            Configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
        }

        public HttpClient GetHttpClient()
        {
            var serverUrl = this["ServerUrl"];

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(serverUrl);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        public string this[string index]
        {
            get
            {
                return Configuration.AppSettings.Settings[index].Value;
            }
        }

        private static ClientHelpers _instance = null;

        public static ClientHelpers Instance
        {
            get
            {
                if (_instance == null) _instance = new ClientHelpers();
                return _instance;
            }
        }



        private  string GetMD5FromFile(string fileName)
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

        public  Dictionary<string, string> GetAllFilesInfo(string directoryPath)
        {
            DirectoryInfo clientDirect = new DirectoryInfo(directoryPath);

            var files = clientDirect.GetFiles().ToList();

            var ignoreExtensionsStr =this["ExtensionToIgnore"].ToString();
            var ingoreFilesStr = this["FileToIgnore"].ToString();

            //转换为大写后分割
            var ignoreExtensions = ignoreExtensionsStr.ToUpper().Split(',');
            var ingoreFiles = ingoreFilesStr.ToUpper().Split(',');
            //过滤需要忽略的文件和后缀名
            files = files.Where(m => !ingoreFiles.Contains(m.Name.ToUpper()) && !ignoreExtensions.Contains(m.Extension.ToUpper())).ToList();

            Dictionary<string, string> fileDict = new Dictionary<string, string>();

            files.ForEach(m => fileDict.Add(m.FullName.Remove(0, directoryPath.Length + 1), GetMD5FromFile(m.FullName)));

            return fileDict;
        }
    }
}
