using GameEngine.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GameEngine.GameServices
{
    public abstract class Scene:Canvas
    {
        private List<GameObject> _gameObjects = new List<GameObject>();  //מאגר כל האובייקטים של המשחק
        protected List<GameObject> _gameObjectsSnapshot => _gameObjects.ToList();   //העתק
        public double Ground { get; set; } //ריצפה

        public Scene()
        {
            Manager.GameEvent.OnRun += Run;   //OnRun כך נרשמים לאירוע בשם
            Manager.GameEvent.OnRun += CheckCollisional;
        }

        private void CheckCollisional()
        {
            foreach (var gameObject in _gameObjectsSnapshot) //עוברים על כל רשימת האובייקטים
            {
                if (gameObject.Collisional)                   //אם האובייקט לא שקוף
                {
                    //מחפשים מופע ראשון של אובייקט, אשר הוא לא אותו האובייקט, הוא לא שקוף והוא נגע באובייקט הנוכחי
                    var otherObject = _gameObjectsSnapshot.FirstOrDefault(g =>
                                            !ReferenceEquals(g, gameObject) &&
                                            g.Collisional &&
                                            !RectHelper.Intersect(g.Rect, gameObject.Rect).IsEmpty);
                    if (otherObject != null)     //אם קיים אוביקט כזה
                    {
        //של אותו האובייקט, כלומר, אם הפעולה נקראת, זה אומר שבוודאות קרתה התנגשות. כל אובייקט משכתב מחדש את הפעולה מפני שמגיב אחרת Collide אם האובייקט מתנגש באובייקט אחר, נקראת הפעולה
                  //כדי שיוכל להגיב באופן מיוחד Collide כל אובייקט "ידרוס" את הפעולה

                        gameObject.Collide(otherObject);
                    }
                }
            }
        }

        private void Run() //בזכות הפעולה כל האובייקטים שאמורים לנוע ינועו
        {
            foreach (var gameObject in _gameObjects)
            {
                if(gameObject is GameMovingObject obj)
                {
                    obj.Render();
                }
            }
        }

        public void Init()         //הפעולה מחזירה את כל האובייקטים למיקום התחלתי
        {
            foreach (GameObject obj in _gameObjects)
            {
                obj.Init();
            }
        }
        public void RemoveObject(GameObject gameObject)           //הפעולה מוחקת אובייקט
        {
            if (_gameObjects.Contains(gameObject))           //האם האובייקט המבוקש נמצא ברשימה
            {
                _gameObjects.Remove(gameObject);           //מחיקתו מהמאגר
                Children.Remove(gameObject.Image);         //מחיקת המראה של האובייקט מהמסך
            }
        }
        public void RemoveAllObject()                                 //הפעולה מוחקת את כל האובייקטים
        {
            foreach (GameObject gameObject in _gameObjects)
            {
                RemoveObject(gameObject);
            }
        }
        public void AddObject(GameObject gameObject)  //הפעולה מוסיפה אובייקט אל המאגר ולמסך
        {
            _gameObjects.Add(gameObject);
            Children.Add(gameObject.Image);
        }
    }
}
