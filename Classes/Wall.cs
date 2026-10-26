using GameEngine.GameObjects;
using GameEngine.GameServices;

namespace BadIceCream.Classes
{
    //קיר אבן - אי אפשר לשבור אותו ואי אפשר לעבור דרכו. יורש מ-GameObject של המנוע (עצם שלא זז)
    public class Wall : GameObject
    {
        public Wall(Scene scene, int row, int col)
            : base(scene, "Game/Wall.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
        }
    }
}
