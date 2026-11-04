using GameEngine.GameServices;
using static GameEngine.GameServices.Constants;

namespace BadIceCream.Classes
{
    //ינשוף שלג: לא זז אף פעם, ורואה את כל השורה ואת כל העמודה שלו (קיר או קרח מסתירים אותו).
    //שחקן שנשאר מול העיניים שלו שנייה אחת - נתפס. יורש ממפלצת, ולכן אפשר להקפיא אותו כמו כל מפלצת
    public class Owl : Monster
    {
        const int StareTicks = 30; //כמה טיקים הינשוף צריך לראות את השחקן כדי לתפוס אותו (בערך שנייה)

        int stareTicks; //כמה טיקים ברצף הינשוף כבר רואה את השחקן

        public Owl(Scene scene, int row, int col) : base(scene, "Owl", row, col)
        {
        }

        //הינשוף לא זז
        protected override string ChooseDirection()
        {
            return "";
        }

        public override void Render()
        {
            base.Render(); //כמו כל מפלצת: המנוע מצייר אותו, ואם הוא קפוא הוא מפשיר לאט

            if (Manager.GameState != GameState.Started)
                return; //המשחק לא רץ

            //ינשוף קפוא לא רואה כלום (העיניים שלו בתוך הקרח)
            if (IsFrozen || !SeesPlayer())
            {
                stareTicks = 0;
                Image.Opacity = 1;
                return;
            }

            //הינשוף רואה את השחקן: סופר, ומהבהב כדי להזהיר אותו
            stareTicks++;
            if (stareTicks / 5 % 2 == 0)
                Image.Opacity = 1;
            else
                Image.Opacity = 0.4;

            if (stareTicks == StareTicks)
                board.GameManager.EndGame(false); //השחקן נשאר מול הינשוף יותר מדי זמן
        }

        //האם השחקן באותה שורה או עמודה כמו הינשוף, ואין ביניהם קיר או קרח
        private bool SeesPlayer()
        {
            int row = board.RowOf(this);
            int col = board.ColOf(this);
            int playerRow = board.RowOf(board.Player);
            int playerCol = board.ColOf(board.Player);

            if (row != playerRow && col != playerCol)
                return false; //לא באותה שורה ולא באותה עמודה
            if (row == playerRow && col == playerCol)
                return true;  //השחקן ממש נוגע בינשוף

            //הכיוון מהינשוף אל השחקן
            string direction = "Right";
            if (playerRow < row)
                direction = "Up";
            if (playerRow > row)
                direction = "Down";
            if (playerCol < col)
                direction = "Left";

            //הולכים משבצת אחרי משבצת מהינשוף עד השחקן, ובודקים שאין קיר או קרח בדרך
            row = GameBoard.NextRow(row, direction);
            col = GameBoard.NextCol(col, direction);
            while (row != playerRow || col != playerCol)
            {
                if (board.HasWall(row, col) || board.IceAt(row, col) != null)
                    return false; //משהו מסתיר את השחקן
                row = GameBoard.NextRow(row, direction);
                col = GameBoard.NextCol(col, direction);
            }
            return true;
        }
    }
}
