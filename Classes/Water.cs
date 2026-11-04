using GameEngine.GameObjects;
using GameEngine.GameServices;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Classes
{
    //מים: אי אפשר ללכת עליהם, אבל רווח מול המים בונה עליהם גשר קרח. יורשים מ-GameObject של המנוע (עצם שלא זז)
    public class Water : GameObject
    {
        public Water(Scene scene, int row, int col)
            : base(scene, "Game/Water.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 0); //על הרצפה, מתחת לכל השאר
            Collisional = false;        //הלוח לא נותן להיכנס למים, אז אין צורך לבדוק נגיעה בהם
        }
    }
}
