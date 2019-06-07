using System;
using System.Collections.Generic;
using System.IO;

namespace Idunas.DanceMusicPlayer.Models
{
    public class Song
    {
        private string _name;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    return "";
                }

                return Path.GetFileNameWithoutExtension(_name);
            }
            set { _name = value; }
        }

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