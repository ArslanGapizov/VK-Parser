using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Newtonsoft.Json;

namespace VK_Parser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _searchFields = "uid,first_name,last_name,sex,bdate,can_write_private_message,relation,country,city,contacts,last_seen,relation";
        private List<User> _usersData;
        public string CaptchaSid { get; set; }
        public List<User> UsersData
        {
            get
            {
                if (_usersData == null)
                {
                    _usersData = new List<User>();
                }
                return _usersData;
            }
        }


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

            groupOptions.IsEnabled = false;
        }

        private void LoginSucceded()
        {
            groupOptions.IsEnabled = true;
        }

        private async Task LoadCountries()
        {

            dynamic countriesResponse = JObject.Parse(await API.database.getCountries("1", null, null, "1000"));

            Dictionary<string, string> countries = new Dictionary<string, string>();
            object lockMe = new object();
            Parallel.ForEach((IEnumerable<dynamic>)countriesResponse.response.items,
                item => { lock (lockMe) { countries.Add(item.id.ToString(), item.title.ToString()); } });

            cbCountry.ItemsSource = countries;
            cbCountry.DisplayMemberPath = "Value";
            cbCountry.SelectedValuePath = "Key";
            cbCountry.SelectedValue = "1";
        }

        private async Task LoadCities()
        {
            dynamic citiesResponse;
            Dictionary<string, string> cities = new Dictionary<string, string>();

            citiesResponse = JObject.Parse(await API.database.getCites(cbCountry.SelectedValue.ToString(), null, null, "0", null, "1000"));

            object lockMe = new object();
            Parallel.ForEach((IEnumerable<dynamic>)citiesResponse.response.items,
                item => { lock (lockMe) { cities.Add(item.id.ToString(), item.title.ToString()); } });

            cbCity.ItemsSource = cities;
            cbCity.DisplayMemberPath = "Value";
            cbCity.SelectedValuePath = "Key";
            cbCity.SelectedValue = cities.FirstOrDefault().Key;
        }
        private void LoadSex()
        {
            cbSex.ItemsSource = CollectionData.CollectionSex();
            cbSex.DisplayMemberPath = "Value";
            cbSex.SelectedValuePath = "Key";
            cbSex.SelectedValue = "0";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            LogoutInterfaceChanges();
            await LoadCountries();
            await LoadCities();
            dgUsers.ItemsSource = UsersData;

        }

        private void InterfaceSetup()
        {
        }

        private async void OnCountryChanged(object sender, EventArgs e)
        {
            await LoadCities();

        }

        private async void OnRegionChanged(object sender, EventArgs e)
        {
            await LoadCities();
        }

        private async void changeProgrText()
        {
            progrText.Text = "Кол-во записей: " + UsersData.Count;
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            groupOptions.IsEnabled = false;

            progrBar.Value = 0;
            progrBar.Maximum = DateEnd.DisplayDate.Subtract(DateStart.DisplayDate).Days + 1;

            for (DateTime date = DateStart.DisplayDate; date <= DateEnd.DisplayDate; date = date.AddDays(1))
            {
                try
                {
                    await Task.Delay(1500);
                    await SearchUsers(date, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                progrBar.Value += 1;
                changeProgrText();
            }
            groupOptions.IsEnabled = true;
        }

        private async Task SearchUsers(DateTime date, string group_id)
        {
            dynamic responseUsers = JObject.Parse(await API.users.search(
                (checkCountry.IsChecked ?? false) ? cbCountry.SelectedValue.ToString() : null,
                (checkCity.IsChecked ?? false) && (checkCountry.IsChecked ?? false) ? cbCity.SelectedValue.ToString() : null,
                cbSex.SelectedValue.ToString(),
                checkBRelation.IsChecked == true ? cbRelationStatus.SelectedValue.ToString() : null,
                "1000",
                _searchFields,
                date,
                group_id
                ));

            string countryFromCB = null;
            string cityFromCB = null;
            string realtionFromCB = null;
            if (checkCountry.IsChecked ?? false)
            {
                countryFromCB = ((KeyValuePair<string, string>)cbCountry.SelectedItem).Value;
            }
            if (checkCity.IsChecked ?? false)
            {
                cityFromCB = ((KeyValuePair<string, string>)cbCity.SelectedItem).Value;
            }
            if (checkBRelation.IsChecked ?? false)
            {
                realtionFromCB = cbRelationStatus.SelectedValue.ToString();
            }
            try
            {
                object lockMe = new object();
                Parallel.ForEach((IEnumerable<dynamic>)responseUsers.response.items, item =>
                {
                    lock (lockMe)
                    {
                        UsersData.Add(new User
                        {
                            Id = ExpMethods.UrlFromID(item.id.ToString()),
                            FirstName = item.first_name,
                            LastName = item.last_name,
                            Sex = ExpMethods.SexFromNumber(item.sex.ToString()),
                            BDate = /*item["bdate"] != null ? item.bdate : null*/date.ToShortDateString(),
                            Country = countryFromCB != null ? countryFromCB : (item["country"] != null ? item.country.title : null),
                            City = cityFromCB != null ? cityFromCB : (item["city"] != null ? item.city.title : null),
                            PrivateMessage = item.can_write_private_message,
                            MobilePhone = item["mobile_phone"] != null ? item.mobile_phone : null,
                            HomePhone = item["home_phone"] != null ? item.home_phone : null,
                            Time = item["last_seen"] != null ? ExpMethods.UnixTimeToDateTime(item.last_seen.time.ToString()) : null,
                            Relation = realtionFromCB != null ? realtionFromCB : (item.relation != null ? item.relation : null),
                            Partner = item["relation_partner"] != null ? ExpMethods.UrlFromID(item.relation_partner.id.ToString()) : null
                        });
                    }
                });
            }
            catch (Exception ex)
            {

            }
            dgUsers.Items.Refresh();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            UsersData.Clear();
            dgUsers.Items.Refresh();
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document";
            dlg.DefaultExt = ".csv";
            dlg.Filter = "(.csv)|*.csv";

            bool? result = dlg.ShowDialog();

            if(result == true)
            {
                bool successed = await ExpMethods.WriteToCSV(UsersData, dlg.FileName);

                MessageBox.Show(successed == true ? "Файл успешно сохранен" : "Произошла ошибка");
            }
        }
    }



    public static class CollectionData
    {
        public static Dictionary<string, string> CollectionSex()
        {
            Dictionary<string, string> sexDictionary = new Dictionary<string, string> { { "1", "female" }, { "2", "male" }, { "0", "any" } };
            return sexDictionary;
        }
        public static Dictionary<string, string> CollectionRelationshipStatus()
        {
            Dictionary<string, string> RelationDictionary = new Dictionary<string, string> { { "1", "Not married" }, { "2", "In relationship" }, { "3", "Engaged" }, { "4", "Married" }, { "5", "It`s complicated" }, { "6", "Actively searching" }, { "7", "In love" } };
            return RelationDictionary;
        }
    }
}
