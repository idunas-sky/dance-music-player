using System;
using System.Collections.Generic;

namespace Idunas.DanceMusicPlayer.Models
{
    public class Song
    {
        public string Name { get; set; }

        public string FilePath { get; set; }

        public DateTime AddedOn { get; set; }

        public bool IsLooping { get; set; }

        public int? LoopMarkerStart { get; set; }

        public int? LoopMarkerEnd { get; set; }

        public IList<Bookmark> Bookmarks { get; set; }

        public Song()
        {
            Bookmarks = new List<Bookmark>();
        }
    }
}