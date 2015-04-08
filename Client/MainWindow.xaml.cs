using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

namespace Client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient client = new HttpClient(); 
        public MainWindow()
        {
            InitializeComponent();

            client.BaseAddress = new Uri("http://localhost:9000");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")); 
        }

        private async void GetProducts(object sender, RoutedEventArgs e)
        {
            try
            {
            //    btnGetProducts.IsEnabled = false;

                var response = await client.GetAsync("api/products");
                response.EnsureSuccessStatusCode(); // Throw on error code（有错误码时报出异常）.

              //  var products = await response.Content.ReadAsAsync<IEnumerable<Product>>();
              //  _products.CopyFrom(products);

            }
            catch (Newtonsoft.Json.JsonException jEx)
            {
                // This exception indicates a problem deserializing the request body.
                // 这个异常指明了一个解序列化请求体的问题。
                MessageBox.Show(jEx.Message);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
              //  btnGetProducts.IsEnabled = true;
            }
        }
    }
}
