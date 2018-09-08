using System;

namespace Idunas.DanceMusicPlayer.Fragments
{
    public class NavigationEventArgs : EventArgs
    {
        public NavDirection Direction { get; private set; }

        public Type Target { get; private set; }

        public Action<object> Initializer { get; private set; }

        public NavigationEventArgs(Type fragmentType, NavDirection direction, Action<object> initializer)
        {
            Target = fragmentType;
            Direction = direction;
            Initializer = initializer;
        }
    }
}