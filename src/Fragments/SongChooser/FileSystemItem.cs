namespace Idunas.DanceMusicPlayer.Fragments.SongChooser
{
    public class FileSystemItem
    {
        private bool _isSelected;

        public string Name { get; set; }

        public string Path { get; set; }

        public bool IsSelected
        {
            get { return IsSelectable ? _isSelected : false; }
            set { _isSelected = value; }
        }

        public bool IsSelectable { get; set; }

        public bool IsDirectory { get; set; }

        public int Icon { get; set; }

        public int Size { get; set; }
    }
}