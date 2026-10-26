using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Pages
{
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack(); //חזרה לתפריט
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Register)); //מעבר לדף ההרשמה
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //בהמשך: בדיקת שם המשתמש והסיסמה מול מסד הנתונים
            Frame.Navigate(typeof(MenuPage)); //מעבר לתפריט
        }

        private void ForgotPasswordLink_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ForgotPassword)); //מעבר לדף שחזור הסיסמה
        }
    }
}
