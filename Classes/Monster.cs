using GameEngine.GameObjects;
using GameEngine.GameServices;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using static GameEngine.GameServices.Constants;

namespace BadIceCream.Classes
{
    //מפלצת. יורשת מ-GameMovingObject של המנוע. יש 3 סוגים:
    //Runner - רץ מהר לכיוונים אקראיים
    //Chaser - רודף אחרי השחקן
    //IceBreaker - רודף אחרי השחקן ושובר קרח שחוסם אותו
    //המפלצות החדשות (למשל Owl) הן מחלקות שיורשות ממפלצת ומשנות את מה שצריך
    public class Monster : GameMovingObject
    {
        const int ChopTicks = 20;            //כמה טיקים לוקח לשובר הקרח לשבור קובייה (קצת יותר מחצי שנייה)
        static Random random = new Random(); //מגריל כיוונים לכל המפלצות

        public string Type { get; set; }      //הסוג של המפלצת
        public string Direction { get; set; } //לאן המפלצת הולכת

        protected GameBoard board; //הלוח שהמפלצת נמצאת בו (protected: גם המחלקות שיורשות ממפלצת משתמשות בו)
        IceBlock chopping;  //קוביית הקרח ששובר הקרח שובר עכשיו (null = לא שובר)
        int chopTicks;      //כמה טיקים נשארו עד שהקובייה תישבר
        int frozenTicks;    //כמה טיקים נשארו עד שהמפלצת מפשירה (0 = לא קפואה)

        //האם המפלצת קפואה עכשיו
        public bool IsFrozen => frozenTicks > 0;

        //לכל סוג יש תמונה בשם שלו: Runner.png, Chaser.png, IceBreaker.png
        public Monster(Scene scene, string type, int row, int col)
            : base(scene, "Game/" + type + ".png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Type = type;
            Direction = "Down";
            board = (GameBoard)scene;
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 3); //המפלצות מעל הפירות והקרח
        }

        //אזור הנגיעה של המפלצת קטן מהתמונה, כדי שמשבצת שרק צמודה למפלצת לא תיחשב כנגיעה
        public override Rect Rect => new Rect(_X + 6, _Y + 6, GameBoard.Size - 12, GameBoard.Size - 12);

        //המפלצת קופאת (קו קרח פגע בה או שהשחקן לקח פתית שלג).
        //virtual: מפלצת מסוג אחר יכולה להוסיף משהו (למשל השודד מחזיר את הפירות שגנב)
        public virtual void Freeze()
        {
            if (frozenTicks > 0)
                return; //כבר קפואה - אי אפשר להאריך את הזמן

            frozenTicks = 90;           //בערך 3 שניות
            if (Type == "IceBreaker")
                frozenTicks = 30;       //שובר הקרח משתחרר מהר יותר (בערך שנייה)

            //עוצרת, ונכנסת בדיוק למשבצת הקרובה (אם הייתה באמצע הליכה)
            Stop();
            _X = board.ColOf(this) * GameBoard.Size;
            _Y = board.RowOf(this) * GameBoard.Size;
            base.Render();                           //המנוע מצייר את המפלצת במקום החדש
            SetImage("Game/" + Type + "Frozen.png"); //התמונה של המפלצת בתוך קוביית קרח
        }

        //המנוע קורא לפעולה הזו כל הזמן (בכל טיק של הטיימר)
        public override void Render()
        {
            base.Render(); //המנוע מזיז את המפלצת, ועוצר אותה כשהיא מגיעה למשבצת

            if (Manager.GameState != GameState.Started)
                return; //המשחק לא רץ

            //המפלצת קפואה: לא זזה עד שהיא מפשירה, ואז חוזרת לתמונה הרגילה שלה
            if (frozenTicks > 0)
            {
                frozenTicks--;
                if (frozenTicks == 0)
                    SetImage("Game/" + Type + ".png");
                return;
            }

            //שובר הקרח באמצע שבירה: עומד במקום עד שהקובייה נשברת
            if (chopping != null)
            {
                chopTicks--;
                if (chopTicks == 0)
                {
                    board.Chop(chopping);
                    chopping = null;
                }
                return;
            }

            //המפלצת עומדת במשבצת: בוחרת לאן ללכת
            if (_dX == 0 && _dY == 0)
            {
                Direction = ChooseDirection();
                if (Direction != "")
                    Step();
            }
        }

        //בוחרת לאן ללכת, או "" כדי לא לזוז. מחלקה שיורשת ממפלצת יכולה לדרוס את הפעולה ולבחור אחרת
        protected virtual string ChooseDirection()
        {
            if (Type == "Runner")
                return RunnerDirection();
            return HuntDirection();
        }

        //הרץ ממשיך ישר. כשהדרך חסומה (ולפעמים סתם, כדי שלא יחזור על אותו מסלול) הוא פונה לכיוון אקראי
        protected string RunnerDirection()
        {
            if (CanGo(Direction) && random.Next(5) != 0)
                return Direction;
            return RandomDirection();
        }

        //הרודף ושובר הקרח: הצעד הראשון בדרך הקצרה ביותר אל המטרה (השחקן, או הגביע בשלב "Protect"), מסביב לקירות ולקרח
        protected string HuntDirection()
        {
            int row = board.RowOf(this);
            int col = board.ColOf(this);
            GameObject target = board.HuntTarget();
            int targetRow = board.RowOf(target);
            int targetCol = board.ColOf(target);

            string direction = board.FirstStep(row, col, targetRow, targetCol, false);

            //שובר הקרח: אם אין דרך מסביב, הוא הולך דרך הקרח ושובר אותו
            if (direction == "" && Type == "IceBreaker")
                direction = board.FirstStep(row, col, targetRow, targetCol, true);

            //אין שום דרך אל המטרה (למשל השחקן הסתגר בקרח): מסתובבים לכיוון אקראי
            if (direction == "")
                direction = RandomDirection();
            return direction;
        }

        //כיוון אקראי מבין הכיוונים הפתוחים, או "" אם כולם חסומים
        protected string RandomDirection()
        {
            List<string> open = new List<string>();
            foreach (string direction in new string[] { "Up", "Down", "Left", "Right" })
            {
                if (CanGo(direction))
                    open.Add(direction);
            }
            if (open.Count == 0)
                return "";
            return open[random.Next(open.Count)];
        }

        //האם המפלצת יכולה ללכת לכיוון: אין שם קיר וגם לא קרח (חוץ משובר הקרח, שיכול לשבור אותו)
        private bool CanGo(string direction)
        {
            int row = GameBoard.NextRow(board.RowOf(this), direction);
            int col = GameBoard.NextCol(board.ColOf(this), direction);
            if (Type == "IceBreaker")
                return board.IsInside(row, col) && !board.HasWall(row, col) && !board.HasFrozenMonster(row, col);
            return board.IsFree(row, col);
        }

        //הולכת משבצת אחת לכיוון. אם שובר הקרח מגיע לקרח, הוא מתחיל לשבור אותו.
        //virtual: מפלצת מסוג אחר יכולה לזוז אחרת (למשל הצפרדע קופצת)
        protected virtual void Step()
        {
            int row = GameBoard.NextRow(board.RowOf(this), Direction);
            int col = GameBoard.NextCol(board.ColOf(this), Direction);
            IceBlock ice = board.IceAt(row, col);

            if (ice != null)
            {
                chopping = ice;
                chopTicks = ChopTicks;
                ice.Image.Opacity = 0.5; //רואים שהקובייה נשברת
            }
            else
            {
                //המהירות של כל סוג (של השחקן: 0.8, כדי שתמיד אפשר לברוח)
                double speed = 0.55;       //הרודף
                if (Type == "Runner")
                    speed = 0.7;           //הרץ הכי מהיר
                if (Type == "IceBreaker")
                    speed = 0.5;           //שובר הקרח הכי איטי, אבל קרח לא עוצר אותו
                if (Type == "Bandit")
                    speed = 0.6;           //השודד זריז
                if (Type == "Yeti")
                    speed = 0.5;           //היטי כבד ואיטי
                if (Type == "Penguin")
                {
                    speed = 0.45;          //הפינגווין הולך לאט,
                    if (board.HasPuddle(board.RowOf(this), board.ColOf(this)))
                        speed = 1.2;       //אבל על שלולית הוא מחליק על הבטן מהר מאוד
                }
                MoveTo(col * GameBoard.Size, row * GameBoard.Size, speed); //המנוע יזיז את המפלצת עד המשבצת
            }
        }
    }
}
