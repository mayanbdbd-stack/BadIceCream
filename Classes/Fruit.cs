using GameEngine.GameObjects;
using GameEngine.GameServices;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Classes
{
    //פרי שהשחקן אוסף. יורש מ-GameObject של המנוע (עצם שלא זז)
    public class Fruit : GameObject
    {
        //לכל סוג פרי יש תמונה בשם שלו: Banana.png, Grapes.png
        public Fruit(Scene scene, string type, int row, int col)
            : base(scene, "Game/" + type + ".png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 1); //הפירות למטה: קרח שנבנה מעל פרי מקפיא אותו
        }
    }
}
