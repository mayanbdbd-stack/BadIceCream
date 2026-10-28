using GameEngine.GameObjects;
using GameEngine.GameServices;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Classes
{
    //קוביית קרח - השחקן בונה ושובר אותה עם רווח, ומפלצות לא עוברות דרכה. יורשת מ-GameObject של המנוע (עצם שלא זז)
    public class IceBlock : GameObject
    {
        public IceBlock(Scene scene, int row, int col)
            : base(scene, "Game/IceBlock.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 2);  //מעל הפירות (פרי שמתחת לקרח קפוא), מתחת לשחקן ולמפלצות
            Image.Opacity = 0.75;        //הקרח קצת שקוף, כדי שרואים פרי שקפא בתוכו
            Collisional = false;         //הלוח לא נותן להיכנס לקרח, אז אין צורך לבדוק נגיעה בו
        }
    }
}
