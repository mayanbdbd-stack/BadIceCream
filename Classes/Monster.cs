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
    public class Monster : GameMovingObject
    {
        const int ChopTicks = 30;            //כמה טיקים לוקח לשובר הקרח לשבור קובייה (בערך שנייה)
        static Random random = new Random(); //מגריל כיוונים לכל המפלצות

        public string Type { get; set; }      //הסוג של המפלצת
        public string Direction { get; set; } //לאן המפלצת הולכת

        GameBoard board;    //הלוח שהמפלצת נמצאת בו
        IceBlock chopping;  //קוביית הקרח ששובר הקרח שובר עכשיו (null = לא שובר)
        int chopTicks;      //כמה טיקים נשארו עד שהקובייה תישבר

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

        //המנוע קורא לפעולה הזו כל הזמן (בכל טיק של הטיימר)
        public override void Render()
        {
            base.Render(); //המנוע מזיז את המפלצת, ועוצר אותה כשהיא מגיעה למשבצת

            if (Manager.GameState != GameState.Started)
                return; //המשחק לא רץ

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
                if (Type == "Runner")
                    Direction = RunnerDirection();
                else
                    Direction = ChaserDirection();

                if (Direction != "")
                    Step();
            }
        }

        //הרץ ממשיך ישר. כשהדרך חסומה (ולפעמים סתם, כדי שלא יחזור על אותו מסלול) הוא פונה לכיוון אקראי
        private string RunnerDirection()
        {
            if (CanGo(Direction) && random.Next(5) != 0)
                return Direction;
            return RandomDirection();
        }

        //הרודף ושובר הקרח: צעד שמקרב אל השחקן. קודם בכיוון שבו המרחק גדול יותר
        private string ChaserDirection()
        {
            int rowDistance = board.RowOf(board.Player) - board.RowOf(this);
            int colDistance = board.ColOf(board.Player) - board.ColOf(this);

            string upDown = "";
            if (rowDistance > 0)
                upDown = "Down";
            if (rowDistance < 0)
                upDown = "Up";

            string leftRight = "";
            if (colDistance > 0)
                leftRight = "Right";
            if (colDistance < 0)
                leftRight = "Left";

            string first = leftRight;
            string second = upDown;
            if (Math.Abs(rowDistance) > Math.Abs(colDistance))
            {
                first = upDown;
                second = leftRight;
            }

            if (first != "" && CanGo(first))
                return first;
            if (second != "" && CanGo(second))
                return second;
            return RandomDirection(); //שתי הדרכים חסומות - הולכים לכיוון אקראי
        }

        //כיוון אקראי מבין הכיוונים הפתוחים, או "" אם כולם חסומים
        private string RandomDirection()
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
                return board.IsInside(row, col) && !board.HasWall(row, col);
            return board.IsFree(row, col);
        }

        //הולכת משבצת אחת לכיוון. אם שובר הקרח מגיע לקרח, הוא מתחיל לשבור אותו
        private void Step()
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
                double speed = 0.4;  //הרודף ושובר הקרח איטיים
                if (Type == "Runner")
                    speed = 0.6;     //הרץ מהיר יותר
                MoveTo(col * GameBoard.Size, row * GameBoard.Size, speed); //המנוע יזיז את המפלצת עד המשבצת
            }
        }
    }
}
