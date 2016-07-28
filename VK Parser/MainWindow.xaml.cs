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
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace VK_Parser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string CaptchaSid { get; set; }

        public MainWindow()
        {

            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginInterfaceChanges();
            JObject AuthResponse = new JObject();
            AuthResponse = JObject.Parse(await API.Authorize(textLogin.Text, textPassword.Password, CaptchaSid, textCaptcha.Text));

            lbLoginError.Visibility = Visibility.Collapsed;
            lbLoginError.Content = "";
            imageCaptcha.Visibility = Visibility.Collapsed;
            textCaptcha.Visibility = Visibility.Collapsed;
            textCaptcha.Text = "";
            CaptchaSid = null;

            JToken token;
            if (AuthResponse.TryGetValue("access_token", out token))
            {
                API.Token = AuthResponse["access_token"].ToString();
                return;
            }
            if (AuthResponse.TryGetValue("error", out token) && AuthResponse["error"].ToString() == "need_captcha")
            {
                CaptchaSid = AuthResponse["captcha_sid"].ToString();
                var captchaImgPath = AuthResponse["captcha_img"].ToString();

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(captchaImgPath, UriKind.Absolute);
                bitmap.EndInit();

                imageCaptcha.Source = bitmap;
                

                lbLoginError.Visibility = Visibility.Visible;
                lbLoginError.Content = AuthResponse["error"].ToString();
                imageCaptcha.Visibility = Visibility.Visible;
                textCaptcha.Visibility = Visibility.Visible;
            }
            else if (AuthResponse.TryGetValue("error", out token) && AuthResponse["error"].ToString() == "invalid_client")
            {
                lbLoginError.Visibility = Visibility.Visible;
                lbLoginError.Content = AuthResponse["error"].ToString();
            }
            else
            {
                MessageBox.Show(AuthResponse.ToString());
            }

            LogoutInterfaceChanges();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            API.Token = "";
            //TODO: Kill threads
            LogoutInterfaceChanges();
        }

        private void LoginInterfaceChanges()
        {
            textLogin.IsEnabled = false;
            textPassword.IsEnabled = false;
            checkRemember.IsEnabled = false;
            btnLogin.IsEnabled = false;
            btnLogout.IsEnabled = true;
        }
        private void LogoutInterfaceChanges()
        {
            textLogin.IsEnabled = true;
            textPassword.IsEnabled = true;
            checkRemember.IsEnabled = true;
            btnLogin.IsEnabled = true;
            btnLogout.IsEnabled = false;
        }
    }
}
