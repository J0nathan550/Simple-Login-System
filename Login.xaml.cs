using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Globalization;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.IO;

namespace LoginProgram
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private int passwordAmount = 4;
        private int seconds = 30;

        private DispatcherTimer? restartLoginTimer = null;
        private string accountsPath = AppDomain.CurrentDomain.BaseDirectory + @"/accounts";

        public Login()
        {
            InitializeComponent();
        }

        private void loginButton(object sender, RoutedEventArgs e)
        {
            if (passwordAmount <= 0)
            {
                return;
            }
            string info = "";
            PlayerAccount account = new PlayerAccount();
            if (File.Exists(accountsPath + $@"/{loginTextBox.Text}.json"))
            {
                info = File.ReadAllText(accountsPath + $@"/{loginTextBox.Text}.json");
            }
            else { 
                passwordAmount -= 1;
                if (passwordAmount <= 0)
                {
                    if (restartLoginTimer == null)
                    {
                        passwordTries.Content = $"Повторите попытку через 30 с.";
                        restartLoginTimer = new();
                        restartLoginTimer.Interval = new TimeSpan(0, 0, 1);
                        restartLoginTimer.Tick += new EventHandler(restartLogin);
                        restartLoginTimer.Start();
                    }
                }
                else
                {
                    passwordTries.Visibility = Visibility.Visible;
                    passwordTries.Content = $"Осталось попыток: {passwordAmount}.";
                }
                return;
            }
            account = JsonConvert.DeserializeObject<PlayerAccount>(info);
            if (passwordTextBox.Password == account.Password)
            {
                loginTab.Visibility = Visibility.Hidden;
                playerInfo.Visibility = Visibility.Visible;
                loadPlayerStats(account);
            }
            else
            {
                passwordAmount -= 1;
                if (passwordAmount <= 0)
                {
                    if (restartLoginTimer == null)
                    {
                        passwordTries.Content = $"Повторите попытку через 30 с.";
                        restartLoginTimer = new();
                        restartLoginTimer.Interval = new TimeSpan(0, 0, 1);
                        restartLoginTimer.Tick += new EventHandler(restartLogin);
                        restartLoginTimer.Start();
                    }
                }
                else
                {
                    passwordTries.Visibility = Visibility.Visible;
                    passwordTries.Content = $"Осталось попыток: {passwordAmount}.";
                }
            }
        }

        private void restartLogin(object? sender, EventArgs e)
        {
            seconds -= 1;
            passwordTries.Content = $"Повторите попытку через {seconds} с.";
            if (seconds <= 0)
            {
                passwordTries.Visibility = Visibility.Hidden;
                passwordAmount = 4;
                seconds = 30;
                restartLoginTimer.Stop();
                restartLoginTimer = null;
                GC.Collect();
            }
        }

        private void loadPlayerStats(PlayerAccount account)
        {
            playerNickname.Content = $"Имя: {account.NickName}";
            playerLevel.Content = $"Уровень: {account.Level}";    
            playerHealth.Content = $"Здоровье: {account.Health}";  
            playerHunger.Content = $"Голод: {account.Hunger}";  
            playerMoney.Content = $"Деньги: {account.Money.ToString("C2")}";    
        }

        private void labelCreateAccount(object sender, System.Windows.Input.MouseEventArgs e)
        {
            loginTab.Visibility = Visibility.Hidden;
            createAccountTab.Visibility = Visibility.Visible;
        }

        private void labelLoginAccount(object sender, System.Windows.Input.MouseEventArgs e)
        {
            loginTab.Visibility = Visibility.Visible;
            createAccountTab.Visibility = Visibility.Hidden;
        }

        private void labelLoginAccount()
        {
            loginTab.Visibility = Visibility.Visible;
            createAccountTab.Visibility = Visibility.Hidden;
        }

        private void createAccountButton(object sender, RoutedEventArgs e)
        {
            errorHandler.Visibility = Visibility.Hidden;

            PlayerAccount account = new PlayerAccount();
            account.NickName = loginCreateTextBox.Text;
            account.Password = passwordCreateTextBox.Password;

            if (!Directory.Exists(accountsPath))
            {
                Directory.CreateDirectory(accountsPath);
            }
            if (File.Exists(accountsPath + @$"/{account.NickName}.json"))
            {
                errorHandler.Visibility = Visibility.Visible;
                errorHandler.Content = "Аккаунт с таким логином уже существует!";
                return;
            }
            string json = JsonConvert.SerializeObject(account, Formatting.Indented);
            FileStream file = File.Create(accountsPath + @$"/{account.NickName}.json");
            file.Close();
            File.WriteAllText(accountsPath + $@"/{account.NickName}.json", json);
            labelLoginAccount();
        }

        private void exitAccount(object sender, RoutedEventArgs e)
        {
            playerInfo.Visibility = Visibility.Hidden;
            loginTab.Visibility = Visibility.Visible;   
            loginTextBox.Text = "";
            passwordTextBox.Password = "";
            passwordCreateTextBox.Password = "";
            loginCreateTextBox.Text = "";
            GC.Collect();
        }
    }

    [Serializable]
    public class PlayerAccount
    {
        public string NickName { get; set; }
        public string Password { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int Hunger { get; set; }
        public decimal Money { get; set; }
        
        public PlayerAccount()
        {
            NickName = "";
            Level = 0;
            Health = 0;
            Hunger = 0;
            Money = 0;
            Password = "";
        }

    }

}
