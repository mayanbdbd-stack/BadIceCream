using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Pages
{
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack(); //חזרה לדף הקודם
        }

        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            //מחליפים בין כפתור צליל דלוק לכפתור צליל כבוי (בהמשך: גם לכבות את הצליל באמת)
            if (SoundOnButton.Visibility == Visibility.Visible)
            {
                SoundOnButton.Visibility = Visibility.Collapsed;
                SoundOffButton.Visibility = Visibility.Visible;
            }
            else
            {
                SoundOnButton.Visibility = Visibility.Visible;
                SoundOffButton.Visibility = Visibility.Collapsed;
            }
        }

        private void MusicButton_Click(object sender, RoutedEventArgs e)
        {
            //מחליפים בין כפתור מוזיקה דלוקה לכפתור מוזיקה כבויה (בהמשך: גם לכבות את המוזיקה באמת)
            if (MusicOnButton.Visibility == Visibility.Visible)
            {
                MusicOnButton.Visibility = Visibility.Collapsed;
                MusicOffButton.Visibility = Visibility.Visible;
            }
            else
            {
                MusicOnButton.Visibility = Visibility.Visible;
                MusicOffButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
