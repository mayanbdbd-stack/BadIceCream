//using DBProject.Models; //יחזור כשיהיה בפתרון הפרויקט DBProject
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using static GameEngine.GameServices.Constants;

namespace GameEngine.GameServices
{
    /*
       המחלקה המופשטת, שדורשת שיהיה לה יורש
       היא מחזיקה בבמה , שני טיימרים, חבילת אירועים סטטית ומצב המשחק
       המחלקה יוצרת שני טיימרים, יוצרת חבילת אירועים סטטית, מכילה פעולות התחלת המשחק, עצירה, המשך וסיום
       OnClock, OnRun שני הטיימרים במידה ופועלים מפעילים ללא הפסקה שני אירועים בהתאמה
       בנוסף, במחלקה נעשית הרשמה ללחיצה ולעזיבת המקשים. אם מתרחשת עזיבה או לחיצה על המקש, מתבצעים שני אירועים בהתאמה. כל מי שנרשם לאירועים הללו מגיב אחרת
        */
    public abstract class Manager
    {

        public Scene Scene { get; private set; }
        public static GameState GameState { get; set; } = GameState.Loaded;//מצב המשחק

        protected static DispatcherTimer _runTimer;  //OnRun הטיימר יפעיל ללא הפסקה אירוע
        public static GameEvents GameEvent { get; } = new GameEvents();  //GameEvents יצירת אירועים הנמצאים במחלקה
        public Manager(Scene scene)
        {
            Scene = scene;
            if(_runTimer == null)
            {
                _runTimer = new DispatcherTimer();
                _runTimer.Interval = TimeSpan.FromMilliseconds(1);
                _runTimer.Tick += _runTimer_Tick;
            }
            _runTimer.Start();

            //כך נרשמים לשימוש במקלדת
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (GameEvent.OnKeyUp != null)     //OnKeyUp אם יש מי שנרשם לאירוע
            {
                GameEvent.OnKeyUp(args.VirtualKey);  //מפעילים את האירוע
            }
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (GameEvent.OnKeyDown != null)     //OnKeyDown אם יש מי שנרשם לאירוע
            {
                GameEvent.OnKeyDown(args.VirtualKey);  //מפעילים את האירוע
            }
        }

        private void _runTimer_Tick(object sender, object e)
        {
            if (GameEvent.OnRun != null)
            {
                GameEvent.OnRun();     //מופעל ללא הפסקה OnRun כך האירוע
            }
        }

        public void Start()
        {
            Scene.Init();        //החזרת כל האובייקטים למיקומם ההתחלתי
            GameState = GameState.Started;
        }
        public void Pause()
        {
            GameState = GameState.Paused;
        }
        public void Resume()
        {
            GameState = GameState.Started;
        }
        public virtual void GameOver()
        {
            if (GameState != GameState.GameOver)
            {
                GameState = GameState.GameOver;
                Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
                Window.Current.CoreWindow.KeyUp -= CoreWindow_KeyUp;
                _runTimer.Stop();
            }
        }
    }
}
