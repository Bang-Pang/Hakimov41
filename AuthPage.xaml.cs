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
using System.Windows.Threading;

namespace Hakimov41
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private int _failCount = 0;
        private string _captchaValue = "";
        private DispatcherTimer _blockTimer;
        public AuthPage()
        {
            InitializeComponent();
        }

        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var rnd = new Random();
            _captchaValue = new string(Enumerable.Repeat(chars, 4).Select(s => s[rnd.Next(s.Length)]).ToArray());
            capchaOneWord.Text = _captchaValue[0].ToString();
            capchaTwoWord.Text = _captchaValue[1].ToString();
            capchaThreeWord.Text = _captchaValue[2].ToString();
            capchaFourWord.Text = _captchaValue[3].ToString();

            CaptchaInput.Text = "";
            CaptchaPanel.Visibility = Visibility.Visible;
        }
       
        private void StarBlock()
        {
            LoginButton.IsEnabled = false;
            _blockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _blockTimer.Tick += (s, e) =>
            {
                _blockTimer.Stop();
                LoginButton.IsEnabled = true;
            };
            _blockTimer.Start();
        }

        
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }
            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                if(!string.Equals(CaptchaInput.Text, _captchaValue, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Капча введена неверно. Вход заблокирован на 10 секунл.");
                    StarBlock();
                    GenerateCaptcha();
                    return;
                }
            }
            User user = Hakimov41Entities1.GetContext().User.ToList().Find(p => p.UserLogin == login && p.UserPassword == password);
            if (user != null)
            {
                _failCount = 0;
                CaptchaPanel.Visibility = Visibility.Collapsed;
                Manager.MainFrame.Navigate(new ProductPage(user));
                LoginTextBox.Text = "";
                PasswordTextBox.Text = "";
            }
            else
            {
                _failCount++;
                MessageBox.Show("Введенные данные неверны");
                if (_failCount >= 1)
                {
                    GenerateCaptcha();
                }
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProductPage(null));
        }
    }
}
