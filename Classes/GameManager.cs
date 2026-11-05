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
        public int Lives { get; set; }    //כמה חיים נשארו לשחקן בשלב
        public bool Won { get; set; }     //האם השחקן ניצח בשלב

        public GameManager(Scene scene) : base(scene)
        {
            ((GameBoard)scene).GameManager = this; //הלוח צריך להכיר את מנהל המשחק
            TimeLeft = 120;                        //2 דקות לכל שלב
            Lives = 3;                             //3 חיים בכל שלב

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

            //נגמר הזמן: בשלב "Survive" זה ניצחון (השחקן שרד), ובכל שלב אחר זה הפסד
            if (TimeLeft == 0)
                EndGame(((GameBoard)Scene).Goal == "Survive");
        }

        //מוסיפה נקודות ומודיעה לדף להציג את הניקוד החדש
        public void AddScore(int points)
        {
            Score += points;
            if (GameEvent.OnScoreRefresh != null)
                GameEvent.OnScoreRefresh();
        }

        //השחקן נתפס (מפלצת נגעה בו, הינשוף ראה אותו, או מפלצת הגיעה לגביע): הוא מאבד חיים אחד.
        //אם נשארו חיים - כולם חוזרים למקום שבו התחילו, ואם לא - השלב נגמר
        public void LoseLife()
        {
            if (GameState != GameState.Started)
                return; //השלב כבר נגמר

            Lives--;
            if (GameEvent.OnRemoveLife != null)
                GameEvent.OnRemoveLife(Lives); //הדף מציג כמה חיים נשארו

            if (Lives == 0)
                EndGame(false);
            else
                Scene.Init(); //המנוע מחזיר כל עצם למקום ההתחלתי שלו (פירות שנאספו כבר לא על הלוח, אז הם נשארים אצל השחקן)
        }

        //סוף השלב. won = true אם השחקן ניצח
        public void EndGame(bool won)
        {
            if (GameState != GameState.Started)
                return; //השלב כבר נגמר

            Won = won;
            if (won)
            {
                AddScore(TimeLeft); //בונוס: נקודה על כל שנייה שנשארה
                if (((GameBoard)Scene).Goal == "Survive")
                    AddScore(60);   //בשלב "Survive" לא נשאר זמן בסוף, אז מקבלים בונוס קבוע על ששרדו
            }
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
