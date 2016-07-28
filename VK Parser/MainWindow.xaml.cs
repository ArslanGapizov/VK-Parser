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
                API.AccessToken = AuthResponse["access_token"].ToString();
                LoginSucceded();
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
            API.AccessToken = "";
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

        private async void LoginSucceded()
        {
        }

        private async void LoadCountries()
        {
            dynamic countriesResponse = JObject.Parse(await API.database.getCountries("1", null, null, "1000"));

            Dictionary<string, string> countries = new Dictionary<string, string>();
            foreach (dynamic item in countriesResponse.response.items)
            {
                countries.Add(item.id.ToString(), item.title.ToString());
            }

            cbCountry.ItemsSource = countries;
            cbCountry.DisplayMemberPath = "Value";
            cbCountry.SelectedValuePath = "Key";
            cbCountry.SelectedValue = "1";
        }

        private async void LoadCities()
        {
            if(cbCountry.SelectedValue == null)
            {
                cbCountry.SelectedValue = "1";
            }
            var resp = await API.database.getCites(cbCountry.SelectedValue.ToString(), null, null, "1", null, "1000");

            Debug.WriteLine(resp);


            dynamic citiesReesponse = JObject.Parse(resp);

            Dictionary<string, string> cities = new Dictionary<string, string>();
            foreach(dynamic item in citiesReesponse.response.items)
            {
                cities.Add(item.id.ToString(), item.title.ToString());
            }
            cbCity.ItemsSource = cities;
            cbCity.DisplayMemberPath = "Value";
            cbCity.SelectedValuePath = "Key";
            cbCity.SelectedValue = cities.First().Key;
        }

        private void LoadSex()
        {
            Dictionary<string, string> sexDictionary = new Dictionary<string, string> { { "1", "женщина"}, { "2", "мужчина"}, { "0", "любой"} };
            cbSex.ItemsSource = sexDictionary;
            cbSex.DisplayMemberPath = "Value";
            cbSex.SelectedValuePath = "Key";
            cbSex.SelectedValue = "0";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSex();
            LoadCountries();
            LoadCities();
        }

        private void OnCountryChanged(object sender, EventArgs e)
        {
            LoadCities();
        }
    }
}
