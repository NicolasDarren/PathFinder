using System;
using System.Windows.Forms;
using Algorithms.Domain;

namespace Algorithms.Visuals
{
    public class WasdActuator : MapVisualActuator
    {
        public override bool NotifyKeyPress(KeyPressEventArgs args)
        {
            var movedCenter = Owner.ComputedCenter;

            switch (args.KeyChar)
            {
                case 'W':
                case 'w': movedCenter = movedCenter.Move(MapDirection.North); break;
                case 'A':
                case 'a': movedCenter = movedCenter.Move(MapDirection.West); break;
                case 'S':
                case 's': movedCenter = movedCenter.Move(MapDirection.South); break;
                case 'D':
                case 'd': movedCenter = movedCenter.Move(MapDirection.East); break;
            }

            bool handled = Owner.Center != movedCenter;
            movedCenter = Owner.Value.ConstrainToBounds(movedCenter);
            Owner.Center = movedCenter;
            return handled;
        }
    }
}