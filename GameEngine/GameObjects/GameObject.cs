using GameEngine.GameServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace GameEngine.GameObjects
{
    //המחלקה לא מיועדת ליצירת עצמים, אלא היא תהווה בסיס ליצירת מחלקות היורשות ממנה
    //המחלקה תכיל את התכונות ואת כל הפעולות המשותפות לכל העצמים העתידיים שיהיו בפרויקט
    public abstract class GameObject
    {
        protected double _X;                    //מיקום נוכחי
        protected double _Y;                    //מיקום נוכחי

        protected double _placeX;     //מיקום התחלתי
        protected double _placeY;     //מיקום התחלתי
        public Image Image { get; set; }   //מראה
        protected string _fileName;          //שם הקובץ של התמונה
        public double Width => Image.ActualWidth;   //קיצור דרך לכתיבה נוחה וקצרה יותר
        public double Height => Image.ActualHeight;   //קיצור דרך לכתיבה נוחה וקצרה יותר
        public virtual Rect Rect => new Rect(_X, _Y, Width, Height); //המלבן שמקיף את העצם

        public bool Collisional { get; set; } = true; //אפשר להתנגש בו, לא שקוף

        protected Scene _scene;  //זירת המשחק

        public GameObject(Scene scene, string fileName, double placeX, double placeY)
        {
            _scene = scene;
            _fileName = fileName;
            _X = placeX;
            _Y = placeY;
            _placeX = placeX;          //כך יזכור העצם איכן נוצר
            _placeY = placeY;          //כך יזכור העצם איכן נוצר
            Image = new Image();
            SetImage(fileName);
            Render();
        }
        public virtual void Render()      //הפעולה מציירת את האובקט על המסך
        {
            Canvas.SetLeft(Image, _X);
            Canvas.SetTop(Image, _Y);
        }
        protected void SetImage(string fileName)        //הפעולה קובעת את תמונת האובייקט
        {
            Image.Source = new BitmapImage(new Uri($"ms-appx:///Assets/{fileName}"));
        }
        public virtual void Init()    //הפעולה מחזירה את האובייקט למיקום התחלתי
        {
            _X = _placeX;
            _Y = _placeY;
        }
        public virtual void Collide(GameObject gameObject)
        {
        }
    }
}
