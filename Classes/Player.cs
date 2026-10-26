using GameEngine.GameObjects;
using GameEngine.GameServices;
using System;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace BadIceCream.Classes
{
    //השחקן - גלידה שזזה על הלוח משבצת אחרי משבצת. יורש מ-GameMovingObject של המנוע
    public class Player : GameMovingObject
    {
        const double Speed = 0.8;   //המהירות של השחקן (המנוע מכפיל אותה ב-Constants.SpeedUnit)
        const double HoldTime = 0.1; //כמה שניות מחזיקים מקש עד שהשחקן מתחיל ללכת

        public string Flavor { get; set; }    //הטעם של הגלידה: Vanilla, Chocolate, Strawberry
        public string Direction { get; set; } //לאן השחקן פונה: Up, Down, Left, Right

        //4 התמונות של הכיוונים. נטענות פעם אחת, כדי שהגלידה לא תהבהב כשהיא פונה
        BitmapImage upPicture;
        BitmapImage downPicture;
        BitmapImage leftPicture;
        BitmapImage rightPicture;

        string heldKey = ""; //הכיוון של המקש שלחוץ עכשיו ("" = אין מקש לחוץ)
        DateTime pressTime;  //מתי התחילו ללחוץ על המקש
        bool mustHold;       //האם צריך להחזיק את המקש לפני שהולכים (כשהשחקן עמד בזמן הלחיצה)

        public Player(Scene scene, string flavor, int row, int col)
            : base(scene, "Game/" + flavor + "Down.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Flavor = flavor;
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;

            upPicture = LoadPicture("Up");
            downPicture = LoadPicture("Down");
            leftPicture = LoadPicture("Left");
            rightPicture = LoadPicture("Right");
            Turn("Down");

            //המנוע יודיע לשחקן על כל לחיצה ועזיבה של מקש
            Manager.GameEvent.OnKeyDown += KeyDown;
            Manager.GameEvent.OnKeyUp += KeyUp;
        }

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
            string direction = KeyToDirection(key);

            //לחיצה חדשה על מקש כיוון
            if (direction != "" && direction != heldKey)
            {
                heldKey = direction;
                pressTime = DateTime.Now;

                //השחקן עומד: הלחיצה רק מסובבת אותו, והוא ילך רק אם ממשיכים להחזיק את המקש
                //השחקן באמצע הליכה: ימשיך מיד בכיוון החדש
                mustHold = IsStanding();
                if (mustHold)
                    Turn(direction);
            }
        }

        private void KeyUp(VirtualKey key)
        {
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

            //השחקן עומד ומקש לחוץ: הולך למשבצת הבאה אם החזיקו מספיק זמן, או אם השחקן כבר הלך
            if (IsStanding() && heldKey != "")
            {
                double heldSeconds = (DateTime.Now - pressTime).TotalSeconds;
                if (!mustHold || heldSeconds >= HoldTime)
                    StepForward();
            }
        }

        //השחקן פונה לכיוון המקש והולך משבצת אחת, אם היא בתוך הלוח
        private void StepForward()
        {
            if (Direction != heldKey)
                Turn(heldKey);

            double toX = _X;
            double toY = _Y;
            if (Direction == "Up")
                toY = _Y - GameBoard.Size;
            if (Direction == "Down")
                toY = _Y + GameBoard.Size;
            if (Direction == "Left")
                toX = _X - GameBoard.Size;
            if (Direction == "Right")
                toX = _X + GameBoard.Size;

            //המשבצת בתוך הלוח
            if (toX >= 0 && toY >= 0 && toX + GameBoard.Size <= _scene.Width && toY + GameBoard.Size <= _scene.Height)
                MoveTo(toX, toY, Speed); //המנוע יזיז את השחקן עד המשבצת
        }
    }
}
