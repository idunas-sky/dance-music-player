using Idunas.DanceMusicPlayer.Models;
using System;
using System.Threading.Tasks;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    public interface IPlayerService
    {
        event EventHandler<int> DurationChanged;
        event EventHandler<int> PositionChanged;
        event EventHandler<PlayerState> StateChanged;
        event EventHandler<Song> SongChanged;

        bool IsLooping { get; set; }

        bool HasNextSong { get; }

        bool HasPreviousSong { get; }

        PlayerState State { get; }

        int Position { get; }

        int Duration { get; }

        Task Load(Song song, Playlist playlist);

        void Play();

        void Pause();

        Task PlayNextSong();

        Task PlayPreviousSong();

        void ChangeSpeed(float speed);

        void SeekTo(int position);
    }
}