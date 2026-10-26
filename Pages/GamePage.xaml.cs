using BadIceCream.Classes;
using GameEngine.GameServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BadIceCream.Pages
{
    public sealed partial class GamePage : Page
    {
        GameManager manager; //מנהל המשחק: מפעיל את הטיימר ומקשיב למקלדת
        Player player;       //השחקן

        public GamePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //נכנסים לדף: יוצרים את מנהל המשחק עם הלוח (Board הוא ה-Scene של המנוע)
            manager = new GameManager(Board);

            //יוצרים את השחקן ומוסיפים אותו ללוח
            player = new Player(Board, "Vanilla", 10, 8); //בהמשך: הטעם שנבחר בחנות
            Board.AddObject(player);

            manager.Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //יוצאים מהדף: עוצרים את הטיימר ואת המקלדת, ומבטלים את כל ההרשמות לאירועים
            manager.GameOver();
            Manager.GameEvent.ResetAllActions();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack(); //חזרה לתפריט
        }
    }
}
