using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Pages
{
    public sealed partial class Register : Page
    {
        public Register()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack(); //חזרה לדף ההתחברות
        }
    }
}
