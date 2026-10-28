using GameEngine.GameServices;
using System;
using Windows.UI.Xaml;
using static GameEngine.GameServices.Constants;

namespace BadIceCream.Classes
{
    //מנהל המשחק. יורש מ-Manager של המנוע (הטיימר, המקלדת ומצב המשחק)
    //ומוסיף את השעון של השלב, את הניקוד ואת סוף השלב
    public class GameManager : Manager
    {
        DispatcherTimer clock;            //שעון השלב: מתקתק פעם בשנייה

        public int TimeLeft { get; set; } //כמה שניות נשארו לשלב
        public int Score { get; set; }    //הניקוד בשלב
        public bool Won { get; set; }     //האם השחקן ניצח בשלב

        public GameManager(Scene scene) : base(scene)
        {
            ((GameBoard)scene).GameManager = this; //הלוח צריך להכיר את מנהל המשחק
            TimeLeft = 120;                        //2 דקות לכל שלב

            clock = new DispatcherTimer();
            clock.Interval = TimeSpan.FromSeconds(1);
            clock.Tick += Clock_Tick;
            clock.Start();
        }

        private void Clock_Tick(object sender, object e)
        {
            if (GameState != GameState.Started)
                return; //המשחק לא רץ

            TimeLeft--;
            if (GameEvent.OnUpdateTime != null)
                GameEvent.OnUpdateTime(TimeLeft); //הדף מציג את הזמן שנשאר

            if (TimeLeft == 0)
                EndGame(false); //נגמר הזמן
        }

        //מוסיפה נקודות ומודיעה לדף להציג את הניקוד החדש
        public void AddScore(int points)
        {
            Score += points;
            if (GameEvent.OnScoreRefresh != null)
                GameEvent.OnScoreRefresh();
        }

        //סוף השלב. won = true אם השחקן ניצח
        public void EndGame(bool won)
        {
            if (GameState != GameState.Started)
                return; //השלב כבר נגמר

            Won = won;
            if (won)
                AddScore(TimeLeft); //בונוס: נקודה על כל שנייה שנשארה
            GameOver();
        }

        //המנוע עוצר את הטיימר ואת המקלדת, ואנחנו עוצרים גם את השעון ומודיעים לדף שהשלב נגמר
        public override void GameOver()
        {
            base.GameOver();
            clock.Stop();
            if (GameEvent.OnGameOver != null)
                GameEvent.OnGameOver();
        }
    }
}
