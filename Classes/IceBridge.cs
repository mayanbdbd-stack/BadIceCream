using GameEngine.GameObjects;
using GameEngine.GameServices;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Classes
{
    //גשר קרח מעל מים: כולם יכולים ללכת עליו. כשהשחקן שובר אותו, מה שעומד עליו נופל למים. יורש מ-GameObject של המנוע (עצם שלא זז)
    public class IceBridge : GameObject
    {
        public IceBridge(Scene scene, int row, int col)
            : base(scene, "Game/IceBridge.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 0); //על המים (הוא נוסף אחריהם, ולכן מצויר מעליהם), מתחת לפירות
            Collisional = false;        //הולכים עליו כמו על רצפה, אין צורך לבדוק נגיעה בו
        }
    }
}
