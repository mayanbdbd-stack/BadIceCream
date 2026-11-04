using GameEngine.GameObjects;
using GameEngine.GameServices;
using System.Collections.Generic;

namespace BadIceCream.Classes
{
    //שודד: דביבון שגונב פירות. הולך אל הפרי הקרוב ביותר (לא פרי שבתוך קרח) ושם אותו בשק, עד 3 פירות.
    //אי אפשר לסיים את הגל כשפירות בשק. מקפיאים אותו - והפירות חוזרים למקום שלהם. יורש ממפלצת
    public class Bandit : Monster
    {
        const int SackSize = 3; //כמה פירות נכנסים לשק

        List<Fruit> sack = new List<Fruit>(); //הפירות שהשודד גנב

        public Bandit(Scene scene, int row, int col) : base(scene, "Bandit", row, col)
        {
        }

        //השודד הולך אל הפרי הקרוב ביותר. כשהשק מלא (או שאין פירות) הוא מסתובב בלוח כמו הרץ
        protected override string ChooseDirection()
        {
            int row = board.RowOf(this);
            int col = board.ColOf(this);
            Fruit fruit = board.NearestFruit(row, col);
            if (sack.Count == SackSize || fruit == null)
                return RunnerDirection();

            string direction = board.FirstStep(row, col, board.RowOf(fruit), board.ColOf(fruit), false);
            if (direction == "")
                direction = RandomDirection(); //אין דרך אל הפרי
            return direction;
        }

        //המנוע קורא לפעולה הזו כשהשודד נוגע בעצם אחר
        public override void Collide(GameObject gameObject)
        {
            //השודד נגע בפרי: גונב אותו לשק (שודד קפוא לא גונב)
            if (gameObject is Fruit && sack.Count < SackSize && !IsFrozen)
            {
                sack.Add((Fruit)gameObject);
                board.RemoveObject(gameObject);
            }
        }

        //השודד קפא: כל הפירות שבשק חוזרים למקום שבו הם היו בהתחלה
        public override void Freeze()
        {
            base.Freeze();
            foreach (Fruit fruit in sack)
            {
                fruit.Stop();           //אם זה ענב שברח, הוא מפסיק לזוז
                fruit.Init();           //המנוע מחזיר את הפרי למקום ההתחלתי שלו
                board.AddObject(fruit);
            }
            sack.Clear();
        }
    }
}
