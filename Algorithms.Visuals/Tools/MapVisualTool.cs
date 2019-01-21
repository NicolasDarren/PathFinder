using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorithms.Domain;

namespace Algorithms.Visuals
{
    public abstract class MapVisualTool : MapVisualActuator
    {
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            if (Owner?.ActiveTool == this)
                Owner.ActiveTool = null;
        }

        public void NotifyToolActivated()
        {
            OnToolActivated();
        }

        public void NotifyToolDeactivated()
        {
            OnToolDeactivated();
        }

        protected virtual void OnToolActivated()
        {
        }

        protected virtual void OnToolDeactivated()
        {
        }

        public virtual void Paint(Rectangle visibleTiles, MapLocation mouseLocationOnMap)
        {

        }
    }
}
