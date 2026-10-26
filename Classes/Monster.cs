using GameEngine.GameObjects;
using GameEngine.GameServices;

namespace BadIceCream.Classes
{
    //מפלצת. יורשת מ-GameMovingObject של המנוע. יש 3 סוגים:
    //Runner - רץ מהר לכיוונים אקראיים
    //Chaser - רודף אחרי השחקן
    //IceBreaker - רודף אחרי השחקן ושובר קרח שחוסם אותו
    public class Monster : GameMovingObject
    {
        public string Type { get; set; }  //הסוג של המפלצת
        public double Speed { get; set; } //המהירות (המנוע מכפיל אותה ב-Constants.SpeedUnit)
        public int ChopTime { get; set; } //כמה טיקים נשארו עד ששובר הקרח שובר את הקרח (0 = לא שובר עכשיו)

        //לכל סוג יש תמונה בשם שלו: Runner.png, Chaser.png, IceBreaker.png
        public Monster(Scene scene, string type, int row, int col)
            : base(scene, "Game/" + type + ".png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Type = type;
            ChopTime = 0;
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;

            //הרץ הוא המהיר ביותר, הרודף ושובר הקרח איטיים יותר
            if (type == "Runner")
                Speed = 0.5;
            else
                Speed = 0.25;
        }
    }
}
