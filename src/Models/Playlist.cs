using System;
using System.Collections.Generic;

namespace Idunas.DanceMusicPlayer.Models
{
    public class Playlist
    {
        public string Name { get; set; }

        public int Speed { get; set; }

        public DateTime CreatedOn { get; set; }

        public IList<Song> Songs { get; set; }

        public Playlist()
        {
            Songs = new List<Song>();
            CreatedOn = DateTime.Now;
            Speed = 100;
        }
    }
}