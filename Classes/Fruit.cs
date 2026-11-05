using GameEngine.GameObjects;
using GameEngine.GameServices;
using System;
using Windows.UI.Xaml.Controls;
using static GameEngine.GameServices.Constants;

namespace BadIceCream.Classes
{
    //פרי שהשחקן אוסף. יורש מ-GameMovingObject של המנוע, כי ענבים יכולים לברוח מהשחקן
    public class Fruit : GameMovingObject
    {
        public bool RunsAway { get; set; } //האם הפרי בורח מהשחקן (הענבים של הגל השני, מעולם 2)

        GameBoard board; //הלוח שהפרי נמצא בו

        //לכל סוג פרי יש תמונה בשם שלו: Banana.png, Grapes.png
        public Fruit(Scene scene, string type, int row, int col)
            : base(scene, "Game/" + type + ".png", col * GameBoard.Size, row * GameBoard.Size)
        {
            board = (GameBoard)scene;
            Image.Width = GameBoard.Size;
            Image.Height = GameBoard.Size;
            Canvas.SetZIndex(Image, 1); //הפירות למטה: קרח שנבנה מעל פרי מקפיא אותו
        }

        //המנוע מחזיר את הפרי למשבצת שבה הוא התחיל (ענב שברח חוזר למקום שלו)
        public override void Init()
        {
            base.Init();
            Stop(); //ענב שהיה באמצע בריחה עוצר
        }

        //המנוע קורא לפעולה הזו כל הזמן (בכל טיק של הטיימר)
        public override void Render()
        {
            base.Render(); //המנוע מזיז את הפרי (רק ענב שבורח זז)

            if (Manager.GameState != GameState.Started || !RunsAway)
                return;
            if (_dX != 0 || _dY != 0)
                return; //באמצע בריחה למשבצת הבאה

            int row = board.RowOf(this);
            int col = board.ColOf(this);
            if (board.IceAt(row, col) != null)
                return; //ענב בתוך קרח קפוא ולא יכול לברוח

            int playerRow = board.RowOf(board.Player);
            int playerCol = board.ColOf(board.Player);
            int distance = Math.Abs(row - playerRow) + Math.Abs(col - playerCol);
            if (distance > 3)
                return; //השחקן רחוק, אין ממה לברוח

            //בורח למשבצת הפנויה שהכי רחוקה מהשחקן, רק אם היא רחוקה יותר מהמשבצת שהוא נמצא בה
            string best = "";
            int bestDistance = distance;
            foreach (string direction in new string[] { "Up", "Down", "Left", "Right" })
            {
                int nextRow = GameBoard.NextRow(row, direction);
                int nextCol = GameBoard.NextCol(col, direction);
                int nextDistance = Math.Abs(nextRow - playerRow) + Math.Abs(nextCol - playerCol);
                if (board.IsFree(nextRow, nextCol) && nextDistance > bestDistance)
                {
                    best = direction;
                    bestDistance = nextDistance;
                }
            }

            //נתקע בפינה: נשאר במקום, ועכשיו אפשר לתפוס אותו
            if (best != "")
                MoveTo(GameBoard.NextCol(col, best) * GameBoard.Size, GameBoard.NextRow(row, best) * GameBoard.Size, 0.5); //לאט יותר מהשחקן
        }
    }
}
