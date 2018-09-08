using Idunas.DanceMusicPlayer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Idunas.DanceMusicPlayer.Services
{
    public class PlaylistsService
    {
        #region --- Singleton

        private static Lazy<PlaylistsService> _instance = new Lazy<PlaylistsService>(() => new PlaylistsService());

        public static PlaylistsService Instance { get { return _instance.Value; } }

        private PlaylistsService()
        {
            _saveFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "playlists.json");
        }

        #endregion

        private string _saveFilePath;
        private IList<Playlist> _playlists;

        public IList<Playlist> Playlists
        {
            get
            {
                if (_playlists == null)
                {
                    _playlists = LoadPlaylists();
                }

                return _playlists;
            }
        }

        private IList<Playlist> LoadPlaylists()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    var tmpFileContent = File.ReadAllText(_saveFilePath);
                    return JsonConvert.DeserializeObject<IList<Playlist>>(tmpFileContent);
                }
                catch (Exception ex)
                {
                    ErrorService.Instance.ShowError(ex, "Failed to load playlists");
                }
            }

            // Default:
            return new List<Playlist>();
        }

        public void Save()
        {
            try
            {
                var tmpFileContent = JsonConvert.SerializeObject(Playlists);
                File.WriteAllText(_saveFilePath, tmpFileContent);
            }
            catch (Exception ex)
            {
                ErrorService.Instance.ShowError(ex, "Failed to save playlists");
            }
        }
    }
}