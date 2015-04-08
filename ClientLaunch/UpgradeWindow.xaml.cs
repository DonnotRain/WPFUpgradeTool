using ClientService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientLaunch
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpgradeWindow : Window
    {

        private bool _hasUpgradeVersion = false;
        private List<string> _fileNamesToUpdate = null;
        public UpgradeWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }
        public UpgradeWindow(bool hasUpgrade)
        {
            InitializeComponent();
            _hasUpgradeVersion = hasUpgrade;
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseDown += MainWindow_MouseDown;
            tbkUpgradeInfo.Text = "检查更新中...";
            //有更新则直接更新掉
            if (_hasUpgradeVersion) GetUpgrateFiles();
            //否则检查更新
            else CheckUpgrate();
        }

        private async void CheckUpgrate()
        {
            var currentVersion = ClientHelpers.Instance["CurrentVersion"].ToString();
            tbkCurrentVersion.Text = "当前版本:" + currentVersion;

            var client = ClientHelpers.Instance.GetHttpClient();
            try
            {
                var response = await client.GetAsync("api/ClientVersion/LastestVersion");

                var clientVersion = await response.Content.ReadAsAsync<dynamic>();

                if (currentVersion != clientVersion.CurrentVersion.ToString())
                {
                    tbkTragetVersion.Text = "检查到新版本：" + clientVersion.CurrentVersion.ToString() + "  更新时间：" + clientVersion.LastUpdateTime.ToString();

                    GetUpgrateFiles();
                }
                else
                {
                    StartClient();
                }
            }
            catch (Newtonsoft.Json.JsonException jEx)
            {
                // 这个异常指明了一个解序列化请求体的问题。
                MessageBox.Show(jEx.Message, "解析JSON错误");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message, "网络请求出错");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "其他错误原因");
            }
        }

        private async void GetUpgrateFiles()
        {
            tbkUpgradeInfo.Text = "检查到更新，正在获取更新文件...";

            var client = ClientHelpers.Instance.GetHttpClient();

            //获取服务器文件信息
            try
            {
                var response = await client.GetAsync("api/ClientVersion");

                var upgradeFiles = await response.Content.ReadAsAsync<Dictionary<string, string>>();

                var localFiles = ClientHelpers.Instance.GetAllFilesInfo(AppDomain.CurrentDomain.BaseDirectory.Remove(AppDomain.CurrentDomain.BaseDirectory.Length-1));

                _fileNamesToUpdate = new List<string>();
                //对比本地文件和服务器文件版本并执行更新
                foreach (var keyValuePair in upgradeFiles)
                {
                    //先本地文件是否存在，不存在或者文件DM5值不同，都添加到待更新列表中
                    if (!localFiles.ContainsKey(keyValuePair.Key) || localFiles[keyValuePair.Key] != keyValuePair.Value)
                        _fileNamesToUpdate.Add(keyValuePair.Key);
                }

                ExcuteUpgrade();
            }

            catch (Newtonsoft.Json.JsonException jEx)
            {
                // 这个异常指明了一个解序列化请求体的问题。
                MessageBox.Show(jEx.Message, "解析JSON错误");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message, "网络请求出错");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "其他错误原因");
            }
        }

        private async void ExcuteUpgrade()
        {
            _fileNamesToUpdate.ForEach(fileName =>
            {
                var currentIndex = _fileNamesToUpdate.IndexOf(fileName);
                tbkUpgradeInfo.Text = "更新中，进度" + currentIndex.ToString() + "/" + _fileNamesToUpdate.Count.ToString() + "(" + Math.Round((double)currentIndex / (double)_fileNamesToUpdate.Count*100)+"%)";
            });
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void StartClient()
        {
            Process.Start("Client.exe");
            //当前运行WPF程序的进程实例
            Process process = Process.GetCurrentProcess();
            //遍历WPF程序的同名进程组
            foreach (Process p in Process.GetProcessesByName(process.ProcessName))
            {
                //关闭全部进程
                p.Kill();//这个地方用kill 而不用Shutdown();的原因是,Shutdown关闭程序在进程管理器里进程的释放有延迟不是马上关闭进程的
                //Application.Current.Shutdown();
                return;
            }
        }
    }
}
