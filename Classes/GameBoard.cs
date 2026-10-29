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
        public string Goal { get; set; }             //המטרה של השלב: "Collect" או "DeepFreeze" (ראו Stages.Goals)

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
                    if (letter == 'S')
                        AddObject(new Snowflake(this, row, col));
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

        //האם אפשר ללכת למשבצת: היא בתוך הלוח ואין בה קיר, קרח או מפלצת קפואה
        public bool IsFree(int row, int col)
        {
            return IsInside(row, col) && !HasWall(row, col) && IceAt(row, col) == null && !HasFrozenMonster(row, col);
        }

        //האם עצם נמצא במשבצת, אפילו רק בחלק ממנה (למשל מפלצת באמצע הליכה)
        private bool IsOnSquare(GameObject gameObject, int row, int col)
        {
            double x = Canvas.GetLeft(gameObject.Image);
            double y = Canvas.GetTop(gameObject.Image);
            return Math.Abs(x - col * Size) < Size && Math.Abs(y - row * Size) < Size;
        }

        //האם יש מפלצת במשבצת
        public bool HasMonster(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Monster && IsOnSquare(gameObject, row, col))
                    return true;
            }
            return false;
        }

        //האם יש מפלצת קפואה במשבצת (מפלצת קפואה חוסמת את הדרך כמו קרח)
        public bool HasFrozenMonster(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Monster && ((Monster)gameObject).IsFrozen && IsOnSquare(gameObject, row, col))
                    return true;
            }
            return false;
        }

        //מקפיאה את המפלצות שבמשבצת (קו קרח נעצר בהן)
        private void FreezeMonstersAt(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Monster && IsOnSquare(gameObject, row, col))
                    ((Monster)gameObject).Freeze();
            }
            CheckDeepFreeze();
        }

        //השחקן לקח פתית שלג: כל המפלצות קופאות
        public void UseSnowflake(Snowflake snowflake)
        {
            RemoveObject(snowflake);
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Monster)
                    ((Monster)gameObject).Freeze();
            }
            CheckDeepFreeze();
        }

        //בשלב עם המטרה "DeepFreeze": מנצחים כשכל המפלצות קפואות באותו זמן
        private void CheckDeepFreeze()
        {
            if (Goal != "DeepFreeze")
                return;

            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Monster && !((Monster)gameObject).IsFrozen)
                    return; //עוד יש מפלצת שלא קפואה
            }
            GameManager.EndGame(true);
        }

        //מפה קטנה של הלוח לחיפוש דרך: '#' קיר (או מפלצת קפואה), 'I' קרח, '.' ריק. נבנית מרשימת העצמים של המנוע
        private char[,] MakeGrid()
        {
            char[,] grid = new char[Rows, Cols];
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    grid[row, col] = '.';
                }
            }

            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Wall)
                    grid[RowOf(gameObject), ColOf(gameObject)] = '#';
                if (gameObject is IceBlock)
                    grid[RowOf(gameObject), ColOf(gameObject)] = 'I';
                if (gameObject is Monster && ((Monster)gameObject).IsFrozen)
                    grid[RowOf(gameObject), ColOf(gameObject)] = '#'; //מפלצת קפואה חוסמת כמו קיר
            }
            return grid;
        }

        //מחפשת את הדרך הקצרה ביותר ממשבצת אחת למשבצת אחרת (חיפוש לרוחב - BFS),
        //ומחזירה את הכיוון של הצעד הראשון בדרך, או "" אם אין דרך.
        //throughIce = true: מותר לעבור דרך קרח (בשביל שובר הקרח, שיכול לשבור אותו)
        public string FirstStep(int fromRow, int fromCol, int toRow, int toCol, bool throughIce)
        {
            char[,] grid = MakeGrid();
            string[,] firstStep = new string[Rows, Cols]; //לכל משבצת: הכיוון של הצעד הראשון בדרך אליה (null = עוד לא הגענו אליה)
            Queue<int[]> queue = new Queue<int[]>();      //המשבצות שהגענו אליהן ועוד צריך להמשיך מהן, לפי הסדר

            firstStep[fromRow, fromCol] = "Start";
            queue.Enqueue(new int[] { fromRow, fromCol });

            while (queue.Count > 0)
            {
                int[] square = queue.Dequeue();
                foreach (string direction in new string[] { "Up", "Down", "Left", "Right" })
                {
                    int row = NextRow(square[0], direction);
                    int col = NextCol(square[1], direction);

                    //מדלגים על משבצת מחוץ ללוח, משבצת שכבר הגענו אליה, קיר, וקרח (אם אסור לעבור בו)
                    if (!IsInside(row, col) || firstStep[row, col] != null)
                        continue;
                    if (grid[row, col] == '#' || (grid[row, col] == 'I' && !throughIce))
                        continue;

                    //הצעד הראשון בדרך: אם יצאנו מנקודת ההתחלה זה הכיוון הזה, ואחרת זה הצעד הראשון של המשבצת הקודמת
                    if (firstStep[square[0], square[1]] == "Start")
                        firstStep[row, col] = direction;
                    else
                        firstStep[row, col] = firstStep[square[0], square[1]];

                    if (row == toRow && col == toCol)
                        return firstStep[row, col]; //מצאנו את הדרך
                    queue.Enqueue(new int[] { row, col });
                }
            }
            return ""; //אין דרך
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
                else if (Goal == "Collect")
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
                    //מלכודת קרח: קו שנעצר במפלצת מקפיא אותה
                    if (IsInside(iceRow, iceCol) && HasMonster(iceRow, iceCol))
                        FreezeMonstersAt(iceRow, iceCol);
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
