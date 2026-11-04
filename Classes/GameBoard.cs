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
        public string Goal { get; set; }             //המטרה של השלב, למשל "Collect" או "Escape" (ראו Stages.Goals)
        public bool RunawayGrapes { get; set; }      //האם הענבים של הגל השני בורחים מהשחקן (מעולם 2 והלאה)

        Exit exit;                               //היציאה של השלב (null אם אין)
        Cone cone;                               //הגביע שצריך להגן עליו (null אם אין)
        int fruitsLeft;                          //כמה פירות נשארו לאסוף בגל הנוכחי
        List<Fruit> waveTwo = new List<Fruit>(); //הפירות של הגל השני. נכנסים ללוח כשהגל הראשון נאסף
        bool waveTwoStarted;                     //האם הגל השני כבר התחיל

        //קו הקרח שנבנה או נשבר עכשיו - מתקדם משבצת אחת בכל טיק
        string iceAction = "";                   //"Build" = בונים, "Break" = שוברים, "Bridge" = בונים גשר, "BreakBridge" = שוברים גשר, "" = אין קו
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
                    {
                        Fruit grapes = new Fruit(this, "Grapes", row, col);
                        grapes.RunsAway = RunawayGrapes;
                        waveTwo.Add(grapes); //לא נכנס ללוח עד הגל השני
                    }
                    if (letter == '~')
                        AddObject(new Puddle(this, row, col));
                    if (letter == 'W')
                        AddObject(new Water(this, row, col));
                    if (letter == 'E')
                    {
                        exit = new Exit(this, row, col);
                        AddObject(exit);
                    }
                    if (letter == 'K')
                    {
                        cone = new Cone(this, row, col);
                        AddObject(cone);
                    }
                    if (letter == 'R')
                        AddObject(new Monster(this, "Runner", row, col));
                    if (letter == 'C')
                        AddObject(new Monster(this, "Chaser", row, col));
                    if (letter == 'B')
                        AddObject(new Monster(this, "IceBreaker", row, col));
                    if (letter == 'O')
                        AddObject(new Owl(this, row, col));
                    if (letter == 'T')
                        AddObject(new Bandit(this, row, col));
                    if (letter == 'D')
                        AddObject(new Penguin(this, row, col));
                    if (letter == 'J')
                        AddObject(new Leapfrog(this, row, col));
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
            exit = null;
            cone = null;
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

        //האם יש שלולית במשבצת
        public bool HasPuddle(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Puddle && RowOf(gameObject) == row && ColOf(gameObject) == col)
                    return true;
            }
            return false;
        }

        //האם יש מים במשבצת (גם אם יש מעליהם גשר)
        public bool HasWater(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Water && RowOf(gameObject) == row && ColOf(gameObject) == col)
                    return true;
            }
            return false;
        }

        //גשר הקרח שנמצא במשבצת, או null אם אין שם גשר
        public IceBridge BridgeAt(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is IceBridge && RowOf(gameObject) == row && ColOf(gameObject) == col)
                    return (IceBridge)gameObject;
            }
            return null;
        }

        //האם הגביע נמצא במשבצת
        public bool HasCone(int row, int col)
        {
            return cone != null && RowOf(cone) == row && ColOf(cone) == col;
        }

        //אחרי מה המפלצות רודפות: אחרי הגביע (בשלב "Protect"), ובכל שלב אחר - אחרי השחקן
        public GameObject HuntTarget()
        {
            if (cone != null)
                return cone;
            return Player;
        }

        //הפרי הקרוב ביותר למשבצת (לא פרי שבתוך קרח), או null אם אין פירות על הלוח
        public Fruit NearestFruit(int row, int col)
        {
            Fruit nearest = null;
            int nearestDistance = 1000;
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Fruit && IceAt(RowOf(gameObject), ColOf(gameObject)) == null)
                {
                    int distance = Math.Abs(RowOf(gameObject) - row) + Math.Abs(ColOf(gameObject) - col);
                    if (distance < nearestDistance)
                    {
                        nearest = (Fruit)gameObject;
                        nearestDistance = distance;
                    }
                }
            }
            return nearest;
        }

        //האם אפשר ללכת למשבצת: היא בתוך הלוח ואין בה קיר, קרח, מפלצת קפואה או מים בלי גשר
        public bool IsFree(int row, int col)
        {
            if (!IsInside(row, col) || HasWall(row, col) || IceAt(row, col) != null || HasFrozenMonster(row, col))
                return false;
            if (HasWater(row, col) && BridgeAt(row, col) == null)
                return false; //מים בלי גשר
            return true;
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

        //מפה קטנה של הלוח לחיפוש דרך: '#' קיר (או מפלצת קפואה), 'I' קרח, 'W' מים, '.' ריק. נבנית מרשימת העצמים של המנוע
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

            //קודם המים, ואחר כך הגשרים: מים עם גשר מעליהם הם שוב רצפה
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Water)
                    grid[RowOf(gameObject), ColOf(gameObject)] = 'W';
            }
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is IceBridge)
                    grid[RowOf(gameObject), ColOf(gameObject)] = '.';
            }

            //ובסוף כל מה שחוסם
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
        //canJump = true: מותר לקפוץ מעל קוביית קרח אחת או משבצת מים אחת (בשביל הצפרדע)
        public string FirstStep(int fromRow, int fromCol, int toRow, int toCol, bool throughIce, bool canJump = false)
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
                    if (!IsInside(row, col))
                        continue;

                    //צפרדע: מול קרח או מים היא קופצת אל המשבצת שאחריהם
                    if (canJump && (grid[row, col] == 'I' || grid[row, col] == 'W'))
                    {
                        row = NextRow(row, direction);
                        col = NextCol(col, direction);
                        if (!IsInside(row, col) || grid[row, col] != '.')
                            continue; //אין לאן לנחות
                    }

                    //מדלגים על משבצת שכבר הגענו אליה, קיר, מים, וקרח (אם אסור לעבור בו)
                    if (firstStep[row, col] != null)
                        continue;
                    if (grid[row, col] == '#' || grid[row, col] == 'W' || (grid[row, col] == 'I' && !throughIce))
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

        //השחקן לחץ על רווח: אם מולו יש קרח שוברים את כל הקו, מול גשר שוברים את הגשר,
        //מול מים בונים גשר, ואחרת בונים קו קרח עד שמשהו עוצר אותו
        public void PressSpace(int row, int col, string direction)
        {
            if (iceAction != "")
                return; //הקו הקודם עוד לא נגמר

            iceRow = NextRow(row, direction);
            iceCol = NextCol(col, direction);
            iceDirection = direction;
            if (IceAt(iceRow, iceCol) != null)
                iceAction = "Break";
            else if (BridgeAt(iceRow, iceCol) != null)
                iceAction = "BreakBridge";
            else if (HasWater(iceRow, iceCol))
                iceAction = "Bridge";
            else
                iceAction = "Build";
        }

        //גשר נשבר: מפלצת שעמדה עליו (אפילו רק בחלק ממנו, באמצע הליכה) נופלת למים ונעלמת (50 נקודות),
        //וענב שברח אליו חוזר למקום שלו
        private void Sink(int row, int col)
        {
            foreach (GameObject gameObject in _gameObjectsSnapshot)
            {
                if (gameObject is Monster && IsOnSquare(gameObject, row, col))
                {
                    RemoveObject(gameObject);
                    GameManager.AddScore(50);
                }
                if (gameObject is Fruit && IsOnSquare(gameObject, row, col))
                {
                    ((Fruit)gameObject).Stop();
                    gameObject.Init(); //המנוע מחזיר את הפרי למקום ההתחלתי שלו
                }
            }
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
                else if (Goal == "Collect" || Goal == "Protect")
                {
                    GameManager.EndGame(true); //כל הפירות נאספו - ניצחון (ובשלב "Protect" גם הגביע שרד)
                }
                else if (Goal == "Escape")
                {
                    exit.Open(); //כל הפירות נאספו - היציאה נפתחת, ועכשיו צריך להגיע אליה
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
                //הקו נעצר בסוף הלוח, בקיר, בקרח, במים, במפלצת או בגביע
                if (IsFree(iceRow, iceCol) && !HasMonster(iceRow, iceCol) && !HasCone(iceRow, iceCol))
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
            else if (iceAction == "Bridge")
            {
                //בונים גשר מעל המים, עד שהמים נגמרים
                if (HasWater(iceRow, iceCol) && BridgeAt(iceRow, iceCol) == null)
                {
                    AddObject(new IceBridge(this, iceRow, iceCol));
                    iceRow = NextRow(iceRow, iceDirection);
                    iceCol = NextCol(iceCol, iceDirection);
                }
                else
                {
                    iceAction = "";
                }
            }
            else if (iceAction == "BreakBridge")
            {
                //שוברים את הגשר עד שהוא נגמר, ומה שעמד עליו נופל למים
                IceBridge bridge = BridgeAt(iceRow, iceCol);
                if (bridge != null)
                {
                    RemoveObject(bridge);
                    Sink(iceRow, iceCol);
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
