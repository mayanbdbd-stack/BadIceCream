using GameEngine.GameObjects;
using GameEngine.GameServices;
using System;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using static GameEngine.GameServices.Constants;

namespace BadIceCream.Classes
{
    //השחקן - גלידה שזזה על הלוח משבצת אחרי משבצת. יורש מ-GameMovingObject של המנוע
    public class Player : GameMovingObject
    {
        const int HoldTicks = 4; //כמה טיקים של המנוע מחזיקים מקש עד שהשחקן מתחיל ללכת

        public string Flavor { get; set; }    //הטעם של הגלידה: Vanilla, Chocolate, Strawberry
        public string Direction { get; set; } //לאן השחקן פונה: Up, Down, Left, Right

        //4 התמונות של הכיוונים. נטענות פעם אחת, כדי שהגלידה לא תהבהב כשהיא פונה
        BitmapImage upPicture;
        BitmapImage downPicture;
        BitmapImage leftPicture;
        BitmapImage rightPicture;

        GameBoard board;      //הלוח שהשחקן נמצא בו
        string heldKey = "";  //הכיוון של המקש שלחוץ עכשיו ("" = אין מקש לחוץ)
        int holdTicks;        //כמה טיקים של המנוע המקש כבר לחוץ
        bool spaceDown;       //האם הרווח לחוץ עכשיו (כדי שלחיצה ארוכה לא תבנה ותשבור שוב ושוב)

        public Player(Scene scene, string flavor, int row, int col)
            : base(scene, "Game/" + flavor + "Down.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Flavor = flavor;
            board = (GameBoard)scene;
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 3); //השחקן מעל הפירות והקרח

            upPicture = LoadPicture("Up");
            downPicture = LoadPicture("Down");
            leftPicture = LoadPicture("Left");
            rightPicture = LoadPicture("Right");
            Turn("Down");

            //המנוע יודיע לשחקן על כל לחיצה ועזיבה של מקש
            Manager.GameEvent.OnKeyDown += KeyDown;
            Manager.GameEvent.OnKeyUp += KeyUp;
        }

        //אזור הנגיעה של השחקן קטן מהתמונה,
        //כדי שעצם שנמצא רק במשבצת שליד השחקן לא ייחשב כנגיעה
        public override Rect Rect => new Rect(_X + 6, _Y + 6, GameBoard.Size - 12, GameBoard.Size - 12);

        //טוענת את התמונה של הטעם בכיוון מסוים, למשל VanillaUp.png
        private BitmapImage LoadPicture(string direction)
        {
            return new BitmapImage(new Uri("ms-appx:///Assets/Game/" + Flavor + direction + ".png"));
        }

        //השחקן פונה לכיוון חדש ומקבל את התמונה של הכיוון הזה
        public void Turn(string direction)
        {
            Direction = direction;

            if (direction == "Up")
                Image.Source = upPicture;
            if (direction == "Down")
                Image.Source = downPicture;
            if (direction == "Left")
                Image.Source = leftPicture;
            if (direction == "Right")
                Image.Source = rightPicture;
        }

        private void KeyDown(VirtualKey key)
        {
            //רווח: בונים או שוברים קו קרח מול השחקן (רק כשהוא עומד, ופעם אחת לכל לחיצה)
            if (key == VirtualKey.Space)
            {
                if (!spaceDown && IsStanding() && Manager.GameState == GameState.Started)
                    board.PressSpace(board.RowOf(this), board.ColOf(this), Direction);
                spaceDown = true;
            }

            string direction = KeyToDirection(key);

            //לחיצה חדשה על מקש כיוון
            if (direction != "" && direction != heldKey)
            {
                heldKey = direction;

                if (IsStanding())
                {
                    //השחקן עומד: הלחיצה רק מסובבת אותו, והוא ילך רק אם ממשיכים להחזיק את המקש
                    Turn(direction);
                    holdTicks = 0;
                }
                else
                {
                    //השחקן באמצע הליכה: ממשיך מיד בכיוון החדש, בלי לחכות
                    holdTicks = HoldTicks;
                }
            }
        }

        private void KeyUp(VirtualKey key)
        {
            if (key == VirtualKey.Space)
                spaceDown = false;

            if (KeyToDirection(key) == heldKey)
                heldKey = ""; //המקש שוחרר - השחקן עוצר במשבצת הבאה
        }

        //מחזירה את הכיוון של המקש (חצים או WASD), או "" אם זה מקש אחר
        private string KeyToDirection(VirtualKey key)
        {
            if (key == VirtualKey.Up || key == VirtualKey.W)
                return "Up";
            if (key == VirtualKey.Down || key == VirtualKey.S)
                return "Down";
            if (key == VirtualKey.Left || key == VirtualKey.A)
                return "Left";
            if (key == VirtualKey.Right || key == VirtualKey.D)
                return "Right";
            return "";
        }

        //האם השחקן עומד במשבצת (ולא באמצע מעבר בין משבצות)
        private bool IsStanding()
        {
            return _dX == 0 && _dY == 0;
        }

        //המנוע קורא לפעולה הזו כל הזמן (בכל טיק של הטיימר)
        public override void Render()
        {
            base.Render(); //המנוע מזיז את התמונה, ועוצר אותה כשהיא מגיעה למשבצת

            if (Manager.GameState != GameState.Started)
                return; //המשחק לא רץ (למשל לפני שהשלב התחיל או אחרי שנגמר)

            if (heldKey != "")
            {
                holdTicks++; //עוד טיק שהמקש לחוץ

                //השחקן עומד והמקש מוחזק מספיק טיקים: הולך למשבצת הבאה
                if (IsStanding() && holdTicks >= HoldTicks)
                    StepForward();
            }
        }

        //השחקן פונה לכיוון המקש והולך משבצת אחת, אם אין שם קיר או קרח
        private void StepForward()
        {
            if (Direction != heldKey)
                Turn(heldKey);

            int row = GameBoard.NextRow(board.RowOf(this), Direction);
            int col = GameBoard.NextCol(board.ColOf(this), Direction);
            if (board.IsFree(row, col))
                MoveTo(col * GameBoard.Size, row * GameBoard.Size, 0.8); //המנוע יזיז את השחקן עד המשבצת. 0.8 = המהירות (המנוע מכפיל אותה ב-Constants.SpeedUnit)
        }

        //המנוע קורא לפעולה הזו כשהשחקן נוגע בעצם אחר
        public override void Collide(GameObject gameObject)
        {
            if (gameObject is Fruit)
                board.CollectFruit((Fruit)gameObject);     //אוספים את הפרי
            if (gameObject is Snowflake)
                board.UseSnowflake((Snowflake)gameObject); //כל המפלצות קופאות
            if (gameObject is Monster && !((Monster)gameObject).IsFrozen)
                board.GameManager.EndGame(false);          //מפלצת תפסה את השחקן (מפלצת קפואה לא מזיקה)
        }
    }
}
