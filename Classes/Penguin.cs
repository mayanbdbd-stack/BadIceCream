using GameEngine.GameServices;

namespace BadIceCream.Classes
{
    //פינגווין: הולך לאט ורודף אחרי השחקן, אבל על שלולית הוא מחליק על הבטן מהר מאוד ולא יכול לפנות.
    //פינגווין שמחליק לתוך קרח מסתחרר ועומד במקום 2 שניות. יורש ממפלצת
    public class Penguin : Monster
    {
        const int DizzyTicks = 60; //כמה טיקים הפינגווין מסתחרר (בערך 2 שניות)

        int dizzyTicks; //כמה טיקים נשארו לסחרחורת (0 = לא מסוחרר)

        public Penguin(Scene scene, int row, int col) : base(scene, "Penguin", row, col)
        {
        }

        protected override string ChooseDirection()
        {
            //מסוחרר: עומד במקום ומהבהב
            if (dizzyTicks > 0)
            {
                dizzyTicks--;
                if (dizzyTicks / 5 % 2 == 0)
                    Image.Opacity = 1;
                else
                    Image.Opacity = 0.5;
                return "";
            }

            //על שלולית הפינגווין ממשיך להחליק ישר
            if (Direction != "" && board.HasPuddle(board.RowOf(this), board.ColOf(this)))
            {
                int row = GameBoard.NextRow(board.RowOf(this), Direction);
                int col = GameBoard.NextCol(board.ColOf(this), Direction);
                if (board.IsFree(row, col))
                    return Direction;       //ממשיך להחליק
                if (board.IceAt(row, col) != null)
                {
                    dizzyTicks = DizzyTicks; //החליק לתוך קרח: מסתחרר
                    return "";
                }
            }

            //על הרצפה (או שההחלקה נעצרה בקיר): רודף אחרי השחקן כמו הרודף
            return base.ChooseDirection();
        }

        //פינגווין שקופא מפסיק להסתחרר
        public override void Freeze()
        {
            base.Freeze();
            dizzyTicks = 0;
            Image.Opacity = 1;
        }
    }
}
