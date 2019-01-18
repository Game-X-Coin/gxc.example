using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Threading;

namespace GameXCoinExample
{
   


    public partial class MainWindow : Window
    {
        private const string API_END_POINT = "http://localhost:5001/api";
        DispatcherTimer _timer;
        TimeSpan _time;
        string gxcAccoutName;
        public MainWindow()
        {
            InitializeComponent();
        }

        private static JObject MakeRequest(string httpMethod, string route, Dictionary<string, string> postParams = null)
        {
            using (var client = new HttpClient())
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod(httpMethod), $"{API_END_POINT}/{route}");
                if (postParams != null)
                    requestMessage.Content = new FormUrlEncodedContent(postParams);   // This is where your content gets added to the request body


                HttpResponseMessage response = client.SendAsync(requestMessage).Result;

                string apiResponse = response.Content.ReadAsStringAsync().Result;
                try
                {
                    // Attempt to deserialise the reponse to the desired type, otherwise throw an expetion with the response from the api.
                    if (apiResponse != "")
                        return JObject.Parse(apiResponse);
                    else
                        throw new Exception();
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error ocurred while calling the API. It responded with the following message: {response.StatusCode} {response.ReasonPhrase}");
                }
            }
        }

   
        private void Button_Click_Login_Success(object sender, RoutedEventArgs e)
        {
            var res = MakeRequest("GET", "user/login_verify/?gxc_account_name=" + gxcAccoutName);
            if (res["success"].ToObject<bool>())
            {
                MessageBoxResult result = MessageBox.Show("Login Success");
            } else
            {
                MessageBoxResult result = MessageBox.Show("Login Failed.");
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            gxcAccoutName = GXCId.Text;
            var res = MakeRequest("GET", "user/login/?gxc_account_name=" + gxcAccoutName );


            QRBox.Visibility = Visibility.Visible;
            Otp.Content = res["otp"].ToObject<string>();
            string qrcode = res["qrcode"].ToObject<string>();
            byte[] binaryData = Convert.FromBase64String(qrcode);

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(binaryData);
            bi.EndInit();
            QRImage.Source = bi;
            Debug.WriteLine(res);

            // init timer

            _time = TimeSpan.FromSeconds(180);

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                Timer.Text = _time.ToString("c");
                if (_time == TimeSpan.Zero) _timer.Stop();
                _time = _time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            _timer.Start();
        }
    }
}
