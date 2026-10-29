using BadIceCream.Classes;
using GameEngine.GameServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BadIceCream.Pages
{
    public sealed partial class GamePage : Page
    {
        GameManager manager; //מנהל המשחק של השלב
        int stage = 0;       //מספר השלב ברשימה Stages.Maps (מ-0 עד 11). בהמשך: השלב השמור של השחקן

        public GamePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //הדף נרשם לאירועים של המנוע כדי להציג את הניקוד, הזמן וסוף השלב
            Manager.GameEvent.OnScoreRefresh += ShowScore;
            Manager.GameEvent.OnUpdateTime += ShowTime;
            Manager.GameEvent.OnGameOver += ShowEndPanel;

            StartStage();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //יוצאים מהדף: עוצרים את הטיימרים ואת המקלדת, ומבטלים את כל ההרשמות לאירועים
            manager.GameOver();
            Manager.GameEvent.ResetAllActions();
        }

        //מתחילה את השלב הנוכחי מההתחלה
        private void StartStage()
        {
            Board.Clear();

            //השחקן הקודם מפסיק להקשיב למקלדת (השחקן החדש יירשם כשהמפה נטענת)
            Manager.GameEvent.OnKeyDown = null;
            Manager.GameEvent.OnKeyUp = null;

            manager = new GameManager(Board);
            Board.Goal = Stages.Goals[stage];
            Board.LoadMap(Stages.Maps[stage]);

            //שם השלב והמטרה שלו, למשל "Stage 1-3: Freeze all the monsters at once!"
            string goalText = "Collect all the fruit!";
            if (Board.Goal == "DeepFreeze")
                goalText = "Freeze all the monsters at once!";
            StageText.Text = "Stage " + (stage / Stages.StagesPerWorld + 1) + "-" + (stage % Stages.StagesPerWorld + 1) + ": " + goalText;
            ShowScore();
            ShowTime(manager.TimeLeft);
            EndPanel.Visibility = Visibility.Collapsed;

            manager.Start();
        }

        //מציגה את הניקוד (המנוע מפעיל אותה דרך האירוע OnScoreRefresh)
        private void ShowScore()
        {
            ScoreText.Text = "Score: " + manager.Score;
        }

        //מציגה את הזמן שנשאר בדקות ושניות, למשל 1:05 (המנוע מפעיל אותה דרך האירוע OnUpdateTime)
        private void ShowTime(int seconds)
        {
            TimeText.Text = "Time: " + seconds / 60 + ":" + (seconds % 60).ToString("00");
        }

        //סוף השלב: מציגה אם ניצחת או הפסדת (המנוע מפעיל אותה דרך האירוע OnGameOver)
        private void ShowEndPanel()
        {
            bool lastStage = stage == Stages.Maps.Length - 1;

            if (manager.Won && lastStage)
                EndTitle.Text = "You finished the game!";
            else if (manager.Won)
                EndTitle.Text = "Stage complete!";
            else if (manager.TimeLeft == 0)
                EndTitle.Text = "Time's up!";
            else
                EndTitle.Text = "Game over";

            EndScore.Text = "Score: " + manager.Score;

            //"הבא" מופיע רק אחרי ניצחון, ורק אם יש עוד שלב
            if (manager.Won && !lastStage)
                NextButton.Visibility = Visibility.Visible;
            else
                NextButton.Visibility = Visibility.Collapsed;

            EndPanel.Visibility = Visibility.Visible;
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            StartStage(); //אותו שלב מההתחלה
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            stage++;      //השלב הבא
            StartStage();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack(); //חזרה לתפריט
        }
    }
}
