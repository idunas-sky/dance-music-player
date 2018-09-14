using System;

namespace Idunas.DanceMusicPlayer.Util
{
    public struct Location
    {
        private readonly int[] _location;

        public int X
        {
            get { return _location[0]; }
        }

        public int Y
        {
            get { return _location[1]; }
        }

        public Location(int[] location)
        {
            _location = location ?? throw new ArgumentNullException(nameof(location));
            if (_location.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(location), "A location needs to have exactly 2 entries");
            }
        }
    }
}