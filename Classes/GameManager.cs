using GameEngine.GameServices;

namespace BadIceCream.Classes
{
    //מנהל המשחק. יורש מ-Manager של המנוע (הטיימר, המקלדת ומצב המשחק)
    public class GameManager : Manager
    {
        public GameManager(Scene scene) : base(scene)
        {
        }
    }
}
