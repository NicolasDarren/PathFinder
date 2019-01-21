using System.Windows.Forms;

namespace Algorithms.Visuals
{
    public abstract class MapVisualComponent
    {
        private MapVisual _owner;

        protected MapVisual Owner => _owner;

        public void NotifyActivated(MapVisual owner)
        {
            _owner = owner;
            OnActivated();
        }

        public void NotifyDeactivated()
        {
            OnDeactivated();
            _owner = null;
        }

        protected virtual void OnDeactivated()
        {
        }

        protected virtual void OnActivated()
        {
        }
    }
}