using GameEngine.GameObjects;
using GameEngine.GameServices;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Classes
{
    //גביע גלידה שצריך להגן עליו (בשלב "Protect"): המפלצות רודפות אחריו, ומפלצת שנוגעת בו - הפסד.
    //יורש מ-GameObject של המנוע (עצם שלא זז)
    public class Cone : GameObject
    {
        GameBoard board; //הלוח שהגביע נמצא בו

        public Cone(Scene scene, int row, int col)
            : base(scene, "Game/Cone.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            board = (GameBoard)scene;
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 1); //למטה, כמו הפירות
        }

        //המנוע קורא לפעולה הזו כשמשהו נוגע בגביע
        public override void Collide(GameObject gameObject)
        {
            if (gameObject is Monster && !((Monster)gameObject).IsFrozen)
                board.GameManager.EndGame(false); //מפלצת הגיעה לגביע
        }
    }
}
