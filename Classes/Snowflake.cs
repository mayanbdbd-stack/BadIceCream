using GameEngine.GameObjects;
using GameEngine.GameServices;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Classes
{
    //פתית שלג: כשהשחקן לוקח אותו, כל המפלצות קופאות. יורש מ-GameObject של המנוע (עצם שלא זז)
    public class Snowflake : GameObject
    {
        public Snowflake(Scene scene, int row, int col)
            : base(scene, "Game/Snowflake.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 1); //למטה, כמו הפירות
        }
    }
}
