using GameEngine.GameObjects;
using GameEngine.GameServices;

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
        }
    }
}
