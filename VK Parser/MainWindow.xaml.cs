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
    public partial class MainWindow : Window
    {
        public string CaptchaSid { get; set; }
        /*Fields for response users json*/
        private string _searchFields = "uid,first_name,last_name,sex,bdate,can_write_private_message,relation,country,city,contacts,last_seen,relation";
        /*List of founded users*/
        private List<User> _usersData;
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

        /*event for button Login*/
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            /*Change interface after click*/
            LoginInterfaceChanges();
            /*Get Response*/
            JObject AuthResponse = new JObject();
            AuthResponse = JObject.Parse(await API.Authorize(textLogin.Text, textPassword.Password, CaptchaSid, textCaptcha.Text));

            lbLoginError.Visibility = Visibility.Collapsed;
            lbLoginError.Content = "";
            imageCaptcha.Visibility = Visibility.Collapsed;
            textCaptcha.Visibility = Visibility.Collapsed;
            textCaptcha.Text = "";
            CaptchaSid = null;

            JToken token;
            /*If response contains access token*/
            if (AuthResponse.TryGetValue("access_token", out token))
            {
                API.AccessToken = AuthResponse["access_token"].ToString();
                LoginSuccessed();
                return;
            }
            /*If error was occured because it needs captcha*/
            if (AuthResponse.TryGetValue("error", out token) && AuthResponse["error"].ToString() == "need_captcha")
            {
                /*save captchasid in propery, for use later*/
                CaptchaSid = AuthResponse["captcha_sid"].ToString();
                /*Get captcha image from response*/
                var captchaImgPath = AuthResponse["captcha_img"].ToString();

                /*set captcha image on interface*/
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(captchaImgPath, UriKind.Absolute);
                bitmap.EndInit();
                imageCaptcha.Source = bitmap;

                /*change interface for showing captcha and error message*/
                lbLoginError.Visibility = Visibility.Visible;
                lbLoginError.Content = AuthResponse["error"].ToString();
                imageCaptcha.Visibility = Visibility.Visible;
                textCaptcha.Visibility = Visibility.Visible;
            }
            /*show error if login or password invalid*/
            else if (AuthResponse.TryGetValue("error", out token) && AuthResponse["error"].ToString() == "invalid_client")
            {
                lbLoginError.Visibility = Visibility.Visible;
                lbLoginError.Content = AuthResponse["error"].ToString();
            }
            /*the others occasions*/
            else
            {
                MessageBox.Show(AuthResponse.ToString());
            }
            /*change interface if login was not successed*/
            LogoutInterfaceChanges();
        }
        /*click on logout button*/
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            /*set access token in null*/
            API.AccessToken = null;
            //TODO: Kill threads
            /*set interface in logout possition*/
            LogoutInterfaceChanges();
        }
        /*set interface in possition login, it executes after clicking on login button*/
        private void LoginInterfaceChanges()
        {
            textLogin.IsEnabled = false;
            textPassword.IsEnabled = false;
            checkRemember.IsEnabled = false;
            btnLogin.IsEnabled = false;
            btnLogout.IsEnabled = true;
            
        }
        /*set interface in posstion logout, it executes after clicking on logout button or if login was failed*/
        private void LogoutInterfaceChanges()
        {
            textLogin.IsEnabled = true;
            textPassword.IsEnabled = true;
            checkRemember.IsEnabled = true;
            btnLogin.IsEnabled = true;
            btnLogout.IsEnabled = false;

            groupOptions.IsEnabled = false;

            btnSearch.IsEnabled = false;
            btnStop.IsEnabled = false;
            btnClear.IsEnabled = false;
            btnSave.IsEnabled = false;
        }
        /*change interface after successed login*/
        private void LoginSuccessed()
        {
            groupOptions.IsEnabled = true;

            btnSearch.IsEnabled = true;
            btnClear.IsEnabled = true;
            btnSave.IsEnabled = true;
        }
        /*loud contiries from api*/
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
        /*load cities from api*/
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
        /*load sex*/
        private void LoadSex()
        {
            cbSex.ItemsSource = CollectionData.CollectionSex();
            cbSex.DisplayMemberPath = "Value";
            cbSex.SelectedValuePath = "Key";
            cbSex.SelectedValue = "0";
        }
        /*excecutes after loading window*/
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogoutInterfaceChanges();
            await LoadCountries();
            await LoadCities();
            dgUsers.ItemsSource = UsersData;

        }
        /*change data in cities comboBox after changing selected value in countries comboBox*/
        private async void OnCountryChanged(object sender, EventArgs e)
        {
            await LoadCities();
        }
        /*changes text on progress bar*/
        private  void changeProgrText()
        {
            progrText.Text = "Кол-во записей: " + UsersData.Count;
        }
        /*block interface when searching in progress, needs true for disabling interface or false for enabling*/
        public void SearchInProgress(bool state)
        {
            groupOptions.IsEnabled = !state;
            btnSearch.IsEnabled = !state;
            btnStop.IsEnabled = state;
            btnClear.IsEnabled = !state;
            btnSave.IsEnabled = !state;
        }
        /*executes after clicking on search Button*/
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            /*Disable options after clicking*/
            SearchInProgress(true);
            
            /*get urls from groups box*/
            string[] urls = textListOfGroups.Text.Split(',');

            /*set progress bar in 0*/
            progrBar.Value = 0;
            /*set porgressBar`s maximum rely on qty days in search`s options*/
            progrBar.Maximum = (DateEnd.DisplayDate.Subtract(DateStart.DisplayDate).Days + 1);

            /*general search if checkBox inGroups unchecked*/
            if (!(checkInGroups.IsChecked ?? false))
            {
                /*pass null if it doesnt need to search in groups*/
                await SearchGroup(null);
            }
            /*search in groups*/
            else
            {
                /*it gets group ids from group urls*/
                string[] groupsID = await ExpMethods.GroupUrlToId(urls);
                /*increase progressBar maximum in groups.lenght times*/
                progrBar.Maximum *= groupsID.Length;
                /*pass each group id in SearchGroup*/
                foreach (var group_id in groupsID)
                {
                    await SearchGroup(group_id);
                }
            }
            /*unblock options interface after searching*/
            SearchInProgress(false);
        }
        /*search in group or if pass null it searches everywhere*/
        private async Task SearchGroup(string group_id)
        {
            /*get delay from slider, needs for setting delay between queries, otherwise there is a chance getting ban*/
            int delay = Convert.ToInt32(sliderDelay.Value);
            /*needs setting birthday dates, it goes from start date to end date*/
            for (DateTime date = DateStart.DisplayDate; date <= DateEnd.DisplayDate; date = date.AddDays(1))
            {
                try
                {
                    /*wait between queries*/
                    await Task.Delay(delay);
                    /*searching users with */
                    await SearchUsers(date, group_id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                /*increase value of progress*/
                progrBar.Value += 1;
                /*change text on progress bar*/
                changeProgrText();
            }
        }
        private async Task SearchUsers(DateTime date, string group_id)
        {
            /*parse JSON string in JSON Object, responseUsers has link on this object*/
            dynamic responseUsers = JObject.Parse(await API.users.search(
                (checkCountry.IsChecked ?? false) ? cbCountry.SelectedValue.ToString() : null,
                (checkCity.IsChecked ?? false) && (checkCountry.IsChecked ?? false) ? cbCity.SelectedValue.ToString() : null,
                (checkUniversity.IsChecked == true && cbUniversity.SelectedItem != null) ? cbUniversity.SelectedValue.ToString() : null,
                cbSex.SelectedValue.ToString(),
                checkBRelation.IsChecked == true ? cbRelationStatus.SelectedValue.ToString() : null,
                "1000",
                _searchFields,
                date,
                group_id
                ));

            /*if country, city or realation are known, we will write it in user`s properties, because sometimes it`s hidden*/
            string countryFromCB = null;
            string cityFromCB = null;
            string realationFromCB = null;
            /*of country is known*/
            if (checkCountry.IsChecked ?? false)
            {
                countryFromCB = ((KeyValuePair<string, string>)cbCountry.SelectedItem).Value;
            }
            /*if city is known*/
            if (checkCity.IsChecked ?? false)
            {
                cityFromCB = ((KeyValuePair<string, string>)cbCity.SelectedItem).Value;
            }
            /*if relation is known*/
            if (checkBRelation.IsChecked ?? false)
            {
                realationFromCB = cbRelationStatus.SelectedValue.ToString();
            }

            try
            {
                object lockMe = new object();
                /*add users from query to list*/
                Parallel.ForEach((IEnumerable<dynamic>)responseUsers.response.items, item =>
                {
                    lock (lockMe)
                    {
                        UsersData.Add(new User
                        {
                            Id = ExpMethods.UrlFromID(item.id.ToString()),/*transform user id to link*/
                            FirstName = item.first_name,
                            LastName = item.last_name,
                            Sex = ExpMethods.SexFromNumber(item.sex.ToString()),
                            BDate = date.ToShortDateString(),
                            /*if country is known write it from comboBox, otherwise from JSON*/
                            Country = countryFromCB != null ? countryFromCB : (item["country"] != null ? item.country.title : null),
                            /*if city is known write it from comboBox, otherwise from JSON*/
                            City = cityFromCB != null ? cityFromCB : (item["city"] != null ? item.city.title : null),
                            PrivateMessage = item.can_write_private_message,
                            MobilePhone = item["mobile_phone"] != null ? item.mobile_phone : null,
                            HomePhone = item["home_phone"] != null ? item.home_phone : null,
                            /*convert unix format to datetime*/
                            Time = item["last_seen"] != null ? ExpMethods.UnixTimeToDateTime(item.last_seen.time.ToString()) : null,
                            /*if relation is known write it from comboBox, otherwise from JSON*/
                            Relation = realationFromCB != null ? realationFromCB : (item.relation != null ? item.relation : null),
                            /*transform partner id to link*/
                            Partner = item["relation_partner"] != null ? ExpMethods.UrlFromID(item.relation_partner.id.ToString()) : null
                        });
                    }
                });
            }
            catch (Exception ex)
            {

            }
            /*refresh datagrid after adding users*/
            dgUsers.Items.Refresh();
        }
        /*clear button click*/
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            /*delete all record from list*/
            UsersData.Clear();
            /*refresh datagrid*/
            dgUsers.Items.Refresh();
        }
        /*save button click*/
        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document";
            dlg.DefaultExt = ".csv";
            dlg.Filter = "(.csv)|*.csv";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                bool successed = await ExpMethods.WriteToCSV(UsersData, dlg.FileName);
                MessageBox.Show(successed == true ? "Файл успешно сохранен" : "Произошла ошибка");
            }
        }
        /*download data about universities after changing text in comboBox university*/
        private async void University_KeyUp(object sender, KeyEventArgs e)
        {
            /*store text in combobox*/
            string heldText = cbUniversity.Text;

            /*query with name of university, city id, country id if was setted*/
            dynamic countriesResponse = JObject.Parse(await API.database.getUniversities(heldText, checkCountry.IsChecked == true ? cbCountry.SelectedValue.ToString() : null, checkCity.IsChecked == true ? cbCity.SelectedValue.ToString() : null, null, "10000"));

            /*dictionary for combobox source, contains ids and universties` names*/
            Dictionary<string, string> universities = new Dictionary<string, string>();
            object lockMe = new object();
            try
            {
                Parallel.ForEach((IEnumerable<dynamic>)countriesResponse.response.items,
                    item => { lock (lockMe) { universities.Add(item.id.ToString(), item.title.ToString()); } });

                cbUniversity.ItemsSource = universities;
                cbUniversity.DisplayMemberPath = "Value";
                cbUniversity.SelectedValuePath = "Key";
            }
            catch (Exception ex)
            {

            }
            /*after setting itemsource text was deleted for combobox, it returns text to combobox*/
            cbUniversity.Text = heldText;
        }
    }
    
}
