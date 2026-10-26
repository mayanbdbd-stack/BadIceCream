using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Pages
{
    public sealed partial class MenuPage : Page
    {
        public MenuPage()
        {
            this.InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamePage)); //מעבר למשחק
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Login)); //מעבר לדף ההתחברות
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Register)); //מעבר לדף ההרשמה
        }

        private void ShopButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ItemShop)); //מעבר לחנות
        }

        private void LeaderboardButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Leaderboard)); //מעבר לטבלת השיאים
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings)); //מעבר להגדרות
        }
    }
}
