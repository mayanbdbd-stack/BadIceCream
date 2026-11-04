using GameEngine.GameObjects;
using GameEngine.GameServices;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Classes
{
    //שלולית: משבצת חלקלקה. השחקן (והפינגווין) מחליקים עליה עד שהם מגיעים למשבצת רגילה. יורשת מ-GameObject של המנוע (עצם שלא זז)
    public class Puddle : GameObject
    {
        public Puddle(Scene scene, int row, int col)
            : base(scene, "Game/Puddle.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 0); //על הרצפה, מתחת לכל השאר (אפשר גם לבנות עליה קרח)
            Collisional = false;        //הולכים עליה כמו על רצפה, אין צורך לבדוק נגיעה בה
        }
    }
}
