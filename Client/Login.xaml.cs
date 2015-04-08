using ClientService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;


namespace Client
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {

        public Login()
        {
            InitializeComponent();
            this.Loaded += Login_Loaded;
        }

        void Login_Loaded(object sender, RoutedEventArgs e)
        {
            //检查更新
            CheckUpgrate();
        }

        private async void CheckUpgrate()
        {
            var client = ClientHelpers.Instance.GetHttpClient();
            try
            {
                var response = await client.GetAsync("api/ClientVersion/LastestVersion");

                var clientVersion = await response.Content.ReadAsAsync<dynamic>();
                //_products.CopyFrom(products);
                var currentVersion = ClientHelpers.Instance["CurrentVersion"].ToString();

                if (currentVersion != clientVersion.CurrentVersion.ToString())
                {
                    //启动更新程序
                    Process.Start("ClientLaunch.exe"); //, "HasUpgrade"
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


    }
}
