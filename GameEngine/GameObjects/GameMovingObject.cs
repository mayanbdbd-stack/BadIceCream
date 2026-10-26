using GameEngine.GameServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameObjects
{
    public abstract class GameMovingObject:GameObject
    {
        protected double _dX;               //מהירות אופקית
        protected double _dY;               //מהירות אנכית
        protected double _ddX;              //תאוצה אופקית
        protected double _ddY;              //תאוצה אנכית
        protected double _toX;              //מיקום היעד בציר אופקי
        protected double _toY;              //מיקום היעד בציר אנכי

        protected GameMovingObject(Scene scene, string fileName, double placeX, double placeY) :
           base(scene, fileName, placeX, placeY)
        {
        }
        public override void Render() //מתבצעת כל הזמן
        {
            _dX += _ddX;     //שינוי מהירות אופקית
            _dY += _ddY;     //שינוי מהרות אנכית

            _X += _dX;          //שינוי מיקום אופקי
            _Y += _dY;          //שינוי מיקום אנכי

            if (Math.Abs(_X - _toX) < 4 && Math.Abs(_Y - _toY) < 4) //האם האובייקט הגיע ליעדו
            {
                Stop();              //עצירת האובייקט
                _X = _toX;           //הזזה קטנה לקיזוז חוסר דיוק בעצירת האובייקט
                _Y = _toY;
            }
            base.Render();      //קריאה לפעולת Render במחלקה GameObject
        }
        public virtual void Stop()
        {
            _dX = _dY = _ddX = _ddY = 0;
        }
        public void MoveTo(double toX, double toY, double speed = 1, double acceleration = 0)
        {
            _toX = toX;
            _toY = toY;

            var len = Math.Sqrt(Math.Pow(_toX - _X, 2) + Math.Pow(_toY - _Y, 2));
            var cos = (_toX - _X) / len;
            var sin = (_toY - _Y) / len;

            speed *= Constants.SpeedUnit;
            _dX = speed * cos;
            _dY = speed * sin;

            _ddX = acceleration * cos;
            _ddY = acceleration * sin;
        }
    }
}
