using System;
using System.Windows.Forms;

namespace Algorithms.Visuals
{
    public abstract class MapVisualActuator : MapVisualComponent
    {
        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when a mouse button is depressed.
        /// Return true to indicate that this event has been handled, this will prevent this event from bubbling to other actuators.
        /// </summary>
        /// <param name="args">The event arguments</param>
        /// <returns>Return true to indicate that the event has been handled and should not be bubbled to other actuators, otherwise return false.</returns>
        public virtual bool NotifyMouseDown(MouseEventArgs args)
        {
            return false;
        }

        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when a mouse button is released.
        /// Return true to indicate that this event has been handled, this will prevent this event from bubbling to other actuators.
        /// </summary>
        /// <param name="args">The event arguments</param>
        /// <returns>Return true to indicate that the event has been handled and should not be bubbled to other actuators, otherwise return false.</returns>
        public virtual bool NotifyMouseUp(MouseEventArgs args)
        {
            return false;
        }

        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when a mouse button has been clicked.
        /// Return true to indicate that this event has been handled, this will prevent this event from bubbling to other actuators.
        /// </summary>
        /// <param name="args">The event arguments</param>
        /// <returns>Return true to indicate that the event has been handled and should not be bubbled to other actuators, otherwise return false.</returns>
        public virtual bool NotifyMouseClick(MouseEventArgs args)
        {
            return false;
        }
        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when a mouse button has been double clicked.
        /// Return true to indicate that this event has been handled, this will prevent this event from bubbling to other actuators.
        /// </summary>
        /// <param name="args">The event arguments</param>
        /// <returns>Return true to indicate that the event has been handled and should not be bubbled to other actuators, otherwise return false.</returns>
        public virtual bool NotifyMouseDoubleClick(MouseEventArgs args)
        {
            return false;
        }

        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when the scroll wheel has been moved.
        /// Return true to indicate that this event has been handled, this will prevent this event from bubbling to other actuators.
        /// </summary>
        /// <param name="args">The event arguments</param>
        /// <returns>Return true to indicate that the event has been handled and should not be bubbled to other actuators, otherwise return false.</returns>
        public virtual bool NotifyMouseWheel(MouseEventArgs args)
        {
            return false;
        }

        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when the mouse is moved. Bubbling of this event cannot be prevented.
        /// </summary>
        /// <param name="args">The event arguments</param>
        public virtual void NotifyMouseMove(MouseEventArgs args)
        {
        }

        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when a key has been depressed.
        /// Return true to indicate that this event has been handled, this will prevent this event from bubbling to other actuators.
        /// </summary>
        /// <param name="args">The event arguments</param>
        /// <returns>Return true to indicate that the event has been handled and should not be bubbled to other actuators, otherwise return false.</returns>
        public virtual bool NotifyKeyDown(KeyEventArgs args)
        {
            return false;
        }
        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when a key has been released. Bubbling of this event cannot be prevented.
        /// </summary>
        /// <param name="args">The event arguments</param>
        public virtual void NotifyKeyUp(KeyEventArgs args)
        {
        }

        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when a key has been pressed.
        /// Return true to indicate that this event has been handled, this will prevent this event from bubbling to other actuators.
        /// </summary>
        /// <param name="args">The event arguments</param>
        /// <returns>Return true to indicate that the event has been handled and should not be bubbled to other actuators, otherwise return false.</returns>
        public virtual bool NotifyKeyPress(KeyPressEventArgs args)
        {
            return false;
        }

        /// <summary>
        /// Raised by the <see cref="MapVisual"/> when the mouse capture changes. Bubbling of this event cannot be prevented.
        /// </summary>
        /// <param name="args">The event arguments</param>
        public virtual void NotifyMouseCaptureChanged(EventArgs args)
        {
        }
    }
}