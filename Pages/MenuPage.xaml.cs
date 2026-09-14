using BadIceCream.Pages.Levels;
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
            Frame.Navigate(typeof(Level1)); //מעבר לשלב הראשון
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
