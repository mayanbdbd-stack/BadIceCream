using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace GameEngine.GameServices
{
    public class GameEvents
    {
        public Action OnRun;                        //האירוע שבזכותו העצמים הנעים ינועו
        public Action<VirtualKey> OnKeyDown;        //אירוע שבזכותו העצמים יוכלו להקשיב ללחיצת המקשים
        public Action<VirtualKey> OnKeyUp;          // האירוע שבזכותו העצמים יוכלו להקשיב לעזיבת המקש
        public Action<int> OnRemoveLife;            //האירוע שבאמצעותו נוכל למחוק לב מדף המשחק
        public Action OnScoreRefresh;               //באמצעות האירוע נוכל להציג את ההישג המעודכן
        public Action<int> OnUpdateTime;            //האריוע מאפשר להציג על המסך את משך הזמן הנותר של המשחק
        public Action OnGameOver;

        public void ResetAllActions()
        {
            OnRun = null;
            OnKeyDown = null;
            OnKeyUp = null;
            OnRemoveLife = null;
            OnScoreRefresh = null;
            OnUpdateTime = null;
            OnGameOver = null;
        }
    }
}
