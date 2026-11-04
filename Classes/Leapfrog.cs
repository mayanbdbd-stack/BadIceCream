using GameEngine.GameObjects;
using GameEngine.GameServices;

namespace BadIceCream.Classes
{
    //צפרדע: רודפת כמו הרודף, אבל יכולה לקפוץ מעל קוביית קרח אחת או משבצת מים אחת
    //(אם המשבצת שאחריהן פנויה). אחרי קפיצה היא נחה שנייה. יורשת ממפלצת
    public class Leapfrog : Monster
    {
        const int RestTicks = 30; //כמה טיקים הצפרדע נחה אחרי קפיצה (בערך שנייה)

        int restTicks; //כמה טיקים נשארו למנוחה (0 = לא נחה)

        public Leapfrog(Scene scene, int row, int col) : base(scene, "Leapfrog", row, col)
        {
        }

        //הצעד הראשון בדרך הקצרה ביותר אל המטרה, כשמותר גם לקפוץ
        protected override string ChooseDirection()
        {
            if (restTicks > 0)
            {
                restTicks--;
                return ""; //נחה אחרי קפיצה
            }

            int row = board.RowOf(this);
            int col = board.ColOf(this);
            GameObject target = board.HuntTarget();
            string direction = board.FirstStep(row, col, board.RowOf(target), board.ColOf(target), false, true);
            if (direction == "")
                direction = RandomDirection(); //אין דרך גם בקפיצות
            return direction;
        }

        //אם המשבצת הבאה פנויה הולכים כרגיל. אם יש בה קרח או מים - קופצים מעליה אל המשבצת שאחריה
        protected override void Step()
        {
            int row = GameBoard.NextRow(board.RowOf(this), Direction);
            int col = GameBoard.NextCol(board.ColOf(this), Direction);
            if (board.IsFree(row, col))
            {
                base.Step();
                return;
            }

            int landRow = GameBoard.NextRow(row, Direction);
            int landCol = GameBoard.NextCol(col, Direction);
            MoveTo(landCol * GameBoard.Size, landRow * GameBoard.Size, 0.9); //המנוע מזיז אותה 2 משבצות בבת אחת
            restTicks = RestTicks;
        }
    }
}
