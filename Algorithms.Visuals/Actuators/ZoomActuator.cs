using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Algorithms.Visuals
{
    public class ZoomActuator : MapVisualActuator
    {
        public override bool NotifyMouseWheel(MouseEventArgs args)
        {
            var position = Owner.GetMapLocationFromClient(args.Location);
            var originalTileSize = Owner.TileSize;

            if (args.Delta > 0)
            {
                Owner.TileSize *= 2;
            }
            else if (args.Delta < 0)
            {
                Owner.TileSize /= 2;
            }

            if (Owner.TileSize != originalTileSize)
                Owner.Center = position;

            return true;
        }
    }
}
