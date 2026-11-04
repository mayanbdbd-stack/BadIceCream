using GameEngine.GameServices;
using System;
using Windows.UI.Xaml.Media;
using static GameEngine.GameServices.Constants;

namespace BadIceCream.Classes
{
    //היטי: המפלצת הגדולה של השלב האחרון. רודף אחרי השחקן, וכל 5 שניות פונה אליו ולוחץ "רווח" בדיוק כמו השחקן
    //(שובר קרח שמולו, או בונה קו קרח שנעצר לפני השחקן). מקפיאים אותו 3 פעמים - ומנצחים. יורש ממפלצת
    public class Yeti : Monster
    {
        const int SpaceTicks = 150; //כל כמה טיקים היטי לוחץ רווח (בערך 5 שניות)
        const int Hits = 3;         //כמה פעמים צריך להקפיא את היטי כדי לנצח

        int spaceTicks; //כמה טיקים עברו מהרווח האחרון של היטי
        int hits;       //כמה פעמים כבר הקפיאו את היטי

        public Yeti(Scene scene, int row, int col) : base(scene, "Yeti", row, col)
        {
            //התמונה של היטי גדולה פי 1.5 ממשבצת, אבל במשחק הוא תופס משבצת אחת (כמו כל מפלצת)
            Image.RenderTransform = new ScaleTransform { ScaleX = 1.5, ScaleY = 1.5, CenterX = GameBoard.Size / 2, CenterY = GameBoard.Size / 2 };
        }

        public override void Render()
        {
            bool wasFrozen = IsFrozen;
            base.Render(); //כמו כל מפלצת: זז, רודף, ומפשיר לאט כשהוא קפוא

            if (Manager.GameState != GameState.Started || IsFrozen)
                return;

            //היטי הפשיר עכשיו: הוא נשאר סדוק, כדי שרואים כמה פגיעות הוא כבר ספג
            if (wasFrozen)
                SetImage("Game/YetiCracked.png");

            spaceTicks++; //סופרים את הזמן עד הרווח הבא
        }

        //היטי עומד במשבצת ובוחר מה לעשות: כל 5 שניות הוא פונה אל השחקן ולוחץ רווח, ובשאר הזמן רודף אחריו
        protected override string ChooseDirection()
        {
            if (spaceTicks >= SpaceTicks)
            {
                spaceTicks = 0;
                board.PressSpace(board.RowOf(this), board.ColOf(this), DirectionToPlayer());
                return ""; //עומד רגע במקום בזמן שהוא לוחץ רווח
            }
            return base.ChooseDirection();
        }

        //הכיוון אל השחקן: לפי הציר שבו השחקן רחוק יותר
        private string DirectionToPlayer()
        {
            int rows = board.RowOf(board.Player) - board.RowOf(this);
            int cols = board.ColOf(board.Player) - board.ColOf(this);
            if (Math.Abs(rows) > Math.Abs(cols))
            {
                if (rows < 0)
                    return "Up";
                return "Down";
            }
            if (cols < 0)
                return "Left";
            return "Right";
        }

        //קו קרח פגע ביטי: עוד פגיעה. אחרי 3 פגיעות השחקן מנצח
        public override void Freeze()
        {
            if (IsFrozen)
                return; //כבר קפוא - הפגיעה לא נחשבת שוב

            base.Freeze();
            hits++;
            if (hits == Hits)
                board.GameManager.EndGame(true);
        }
    }
}
