using GameEngine.GameObjects;
using GameEngine.GameServices;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using static GameEngine.GameServices.Constants;

namespace BadIceCream.Classes
{
    //לוח המשחק: 18 על 12 משבצות. יורש מ-Scene של המנוע (שומר את כל העצמים של המשחק ומזיז אותם)
    public class GameBoard : Scene
    {
        public const int Size = 40; //גודל של משבצת בלוח (בפיקסלים)
        public const int Rows = 12; //מספר השורות בלוח
        public const int Cols = 18; //מספר העמודות בלוח

        public GameManager GameManager { get; set; } //מנהל המשחק של השלב
        public Player Player { get; set; }           //השחקן

        int fruitsLeft;                          //כמה פירות נשארו לאסוף בגל הנוכחי
        List<Fruit> waveTwo = new List<Fruit>(); //הפירות של הגל השני. נכנסים ללוח כשהגל הראשון נאסף
        bool waveTwoStarted;                     //האם הגל השני כבר התחיל

        //קו הקרח שנבנה או נשבר עכשיו - מתקדם משבצת אחת בכל טיק
        string iceAction = "";                   //"Build" = בונים, "Break" = שוברים, "" = אין קו
        int iceRow;                              //השורה של המשבצת הבאה בקו
        int iceCol;                              //העמודה של המשבצת הבאה בקו
        string iceDirection;                     //לאן הקו מתקדם

        List<IceBlock> chopped = new List<IceBlock>(); //קוביות ששובר הקרח סיים לשבור

        public GameBoard()
        {
            //בכל טיק, אחרי שכל העצמים זזו, הלוח מקדם את קו הקרח ומוחק קרח שנשבר.
            //אסור להוסיף או למחוק עצמים בתוך Render (המנוע עובר אז על רשימת העצמים), ולכן עושים את זה כאן
            Manager.GameEvent.OnRun += Update;
        }

        //הופכת את המפה (12 שורות של 18 אותיות) לעצמים על הלוח
        public void LoadMap(string[] map)
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    char letter = map[row][col];
                    if (letter == '#')
                        AddObject(new Wall(this, row, col));
                    if (letter == 'I')
                        AddObject(new IceBlock(this, row, col));
                    if (letter == 'F')
                    {
                        AddObject(new Fruit(this, "Banana", row, col));
                        fruitsLeft++;
                    }
                    if (letter == 'G')
                        waveTwo.Add(new Fruit(this, "Grapes", row, col)); //לא נכנס ללוח עד הגל השני
                    if (letter == 'R')
                        AddObject(new Monster(this, "Runner", row, col));
                    if (letter == 'C')
                        AddObject(new Monster(this, "Chaser", row, col));
                    if (letter == 'B')
                        AddObject(new Monster(this, "IceBreaker", row, col));
                    if (letter == 'P')
                        Player = new Player(this, "Vanilla", row, col); //בהמשך: הטעם שנבחר בחנות
                }
            }
            AddObject(Player);
        }

        //מוחקת את כל העצמים מהלוח ומאפסת אותו לפני שטוענים שלב.
        //עוברים על העתק של רשימת העצמים, כי אסור למחוק מרשימה תוך כדי מעבר עליה
        public void Clear()
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                RemoveObject(gameObject);
            }
            Player = null;
            fruitsLeft = 0;
            waveTwo.Clear();
            waveTwoStarted = false;
            iceAction = "";
            chopped.Clear();
        }

        //השורה שבה נמצא עצם, לפי המקום של התמונה שלו
        public int RowOf(GameObject gameObject)
        {
            return (int)Math.Round(Canvas.GetTop(gameObject.Image) / Size);
        }

        //העמודה שבה נמצא עצם, לפי המקום של התמונה שלו
        public int ColOf(GameObject gameObject)
        {
            return (int)Math.Round(Canvas.GetLeft(gameObject.Image) / Size);
        }

        //השורה של המשבצת הבאה בכיוון מסוים
        public static int NextRow(int row, string direction)
        {
            if (direction == "Up")
                return row - 1;
            if (direction == "Down")
                return row + 1;
            return row;
        }

        //העמודה של המשבצת הבאה בכיוון מסוים
        public static int NextCol(int col, string direction)
        {
            if (direction == "Left")
                return col - 1;
            if (direction == "Right")
                return col + 1;
            return col;
        }

        //האם המשבצת נמצאת בתוך הלוח
        public bool IsInside(int row, int col)
        {
            return row >= 0 && row < Rows && col >= 0 && col < Cols;
        }

        //האם יש קיר במשבצת
        public bool HasWall(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Wall && RowOf(gameObject) == row && ColOf(gameObject) == col)
                    return true;
            }
            return false;
        }

        //קוביית הקרח שנמצאת במשבצת, או null אם אין שם קרח
        public IceBlock IceAt(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is IceBlock && RowOf(gameObject) == row && ColOf(gameObject) == col)
                    return (IceBlock)gameObject;
            }
            return null;
        }

        //האם אפשר ללכת למשבצת: היא בתוך הלוח ואין בה קיר או קרח
        public bool IsFree(int row, int col)
        {
            return IsInside(row, col) && !HasWall(row, col) && IceAt(row, col) == null;
        }

        //האם מפלצת נמצאת במשבצת, אפילו רק בחלק ממנה (למשל באמצע הליכה)
        public bool HasMonster(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Monster)
                {
                    double x = Canvas.GetLeft(gameObject.Image);
                    double y = Canvas.GetTop(gameObject.Image);
                    if (Math.Abs(x - col * Size) < Size && Math.Abs(y - row * Size) < Size)
                        return true;
                }
            }
            return false;
        }

        //השחקן לחץ על רווח: אם מולו יש קרח שוברים את כל הקו, ואחרת בונים קו קרח עד שמשהו עוצר אותו
        public void PressSpace(int row, int col, string direction)
        {
            if (iceAction != "")
                return; //הקו הקודם עוד לא נגמר

            iceRow = NextRow(row, direction);
            iceCol = NextCol(col, direction);
            iceDirection = direction;
            if (IceAt(iceRow, iceCol) != null)
                iceAction = "Break";
            else
                iceAction = "Build";
        }

        //שובר הקרח סיים לשבור קובייה. היא תימחק ב-Update (אסור למחוק עצמים בתוך Render)
        public void Chop(IceBlock ice)
        {
            chopped.Add(ice);
        }

        //השחקן נגע בפרי ואסף אותו
        public void CollectFruit(Fruit fruit)
        {
            RemoveObject(fruit);
            GameManager.AddScore(10);
            fruitsLeft--;

            if (fruitsLeft == 0)
            {
                if (!waveTwoStarted && waveTwo.Count > 0)
                {
                    //הגל הראשון נאסף: מתחיל הגל השני
                    waveTwoStarted = true;
                    foreach (Fruit next in waveTwo)
                    {
                        AddObject(next);
                    }
                    fruitsLeft = waveTwo.Count;
                }
                else
                {
                    GameManager.EndGame(true); //כל הפירות נאספו - ניצחון
                }
            }
        }

        //המנוע קורא לפעולה הזו בכל טיק, אחרי שכל העצמים זזו ואחרי בדיקת ההתנגשויות
        private void Update()
        {
            if (Manager.GameState != GameState.Started)
                return; //המשחק לא רץ

            //מוחקים את הקוביות ששובר הקרח שבר
            foreach (IceBlock ice in chopped)
            {
                RemoveObject(ice);
            }
            chopped.Clear();

            //קו הקרח מתקדם משבצת אחת
            if (iceAction == "Build")
            {
                //הקו נעצר בסוף הלוח, בקיר, בקרח או במפלצת
                if (IsFree(iceRow, iceCol) && !HasMonster(iceRow, iceCol))
                {
                    AddObject(new IceBlock(this, iceRow, iceCol));
                    iceRow = NextRow(iceRow, iceDirection);
                    iceCol = NextCol(iceCol, iceDirection);
                }
                else
                {
                    iceAction = "";
                }
            }
            else if (iceAction == "Break")
            {
                //שוברים קרח עד שהקו נגמר
                IceBlock ice = IceAt(iceRow, iceCol);
                if (ice != null)
                {
                    RemoveObject(ice);
                    iceRow = NextRow(iceRow, iceDirection);
                    iceCol = NextCol(iceCol, iceDirection);
                }
                else
                {
                    iceAction = "";
                }
            }
        }
    }
}
