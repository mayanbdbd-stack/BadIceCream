using GameEngine.GameObjects;
using GameEngine.GameServices;

namespace BadIceCream.Classes
{
    //פרי שהשחקן אוסף. יורש מ-GameObject של המנוע (עצם שלא זז)
    //יש 2 גלים: הפירות של הגל השני מופיעים אחרי שאוספים את כל הפירות של הגל הראשון
    public class Fruit : GameObject
    {
        public string Type { get; set; } //סוג הפרי: Banana, Grapes
        public int Wave { get; set; }    //באיזה גל הפרי מופיע: 1 או 2

        //לכל סוג יש תמונה בשם שלו: Banana.png, Grapes.png
        public Fruit(Scene scene, string type, int wave, int row, int col)
            : base(scene, "Game/" + type + ".png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Type = type;
            Wave = wave;
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
        }
    }
}
