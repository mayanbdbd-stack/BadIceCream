using GameEngine.GameObjects;
using GameEngine.GameServices;
using Windows.UI.Xaml.Controls;

namespace BadIceCream.Classes
{
    //יציאה: סגורה עד שכל הפירות נאספים, ואז נפתחת. שחקן שנכנס ליציאה פתוחה מנצח. יורשת מ-GameObject של המנוע (עצם שלא זז)
    public class Exit : GameObject
    {
        public bool IsOpen { get; set; } //האם היציאה פתוחה

        public Exit(Scene scene, int row, int col)
            : base(scene, "Game/ExitClosed.png", col * GameBoard.Size, row * GameBoard.Size)
        {
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 1); //למטה, כמו הפירות
        }

        //כל הפירות נאספו: היציאה נפתחת
        public void Open()
        {
            IsOpen = true;
            SetImage("Game/ExitOpen.png");
        }
    }
}
