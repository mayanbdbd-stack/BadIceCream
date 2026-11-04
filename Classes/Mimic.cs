using GameEngine.GameServices;
using System;

namespace BadIceCream.Classes
{
    //חקיין: מתחפש לפרי (בננה, ובגל השני ענבים) ולא זז, עד שהשחקן מתקרב אליו ל-2 משבצות.
    //אז הוא מתעורר ורודף אחרי השחקן. קו קרח נעצר בו ומקפיא אותו, וכך אפשר לגלות אותו מרחוק. יורש ממפלצת
    public class Mimic : Monster
    {
        bool awake;              //האם החקיין כבר התעורר
        string looks = "Mimic";  //הפרי שהחקיין מתחפש אליו עכשיו

        public Mimic(Scene scene, int row, int col) : base(scene, "Mimic", row, col)
        {
            Disguise();
        }

        //החקיין מתחפש לפרי של הגל הנוכחי (מחליפים תמונה רק כשהגל מתחלף)
        private void Disguise()
        {
            string fruit = "Banana";
            if (board.WaveTwoStarted)
                fruit = "Grapes";
            if (fruit != looks)
            {
                looks = fruit;
                SetImage("Game/" + fruit + ".png");
            }
        }

        protected override string ChooseDirection()
        {
            if (!awake)
            {
                Disguise();

                //השחקן רחוק: החקיין ממשיך להעמיד פנים שהוא פרי
                int distance = Math.Abs(board.RowOf(this) - board.RowOf(board.Player)) + Math.Abs(board.ColOf(this) - board.ColOf(board.Player));
                if (distance > 2)
                    return "";

                awake = true;                //השחקן התקרב: החקיין מתעורר
                SetImage("Game/Mimic.png");
            }
            return HuntDirection();          //רודף אחרי השחקן כמו הרודף
        }

        //חקיין שקפא כבר נחשף: כשהוא מפשיר הוא רודף אחרי השחקן
        public override void Freeze()
        {
            base.Freeze();
            awake = true;
        }
    }
}
