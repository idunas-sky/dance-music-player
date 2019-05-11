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
                    if (!File.Exists(_saveFilePath))
                    {
                        _playlists = new List<Playlist>();
                    }
                    else
                    {
                        _playlists = LoadInternal(_saveFilePath);
                    }
                }

                return _playlists;
            }
        }

        public void Save()
        {
            try
            {
                SaveInternal(_saveFilePath);
            }
            catch (Exception ex)
            {
                MessageService.ShowError(ex, "Failed to save playlists");
            }
        }

        public void Export(string filePath)
        {
            try
            {
                SaveInternal(filePath);
            }
            catch (Exception ex)
            {
                MessageService.ShowError(ex, "Failed to export playlists");
            }
        }

        public void Import(string filePath)
        {
            _playlists = LoadInternal(filePath);
            Save();
        }

        private void SaveInternal(string path)
        {
            var tmpFileContent = JsonConvert.SerializeObject(Playlists);
            File.WriteAllText(path, tmpFileContent);
        }

        private IList<Playlist> LoadInternal(string filePath)
        {
            try
            {
                var tmpFileContent = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<IList<Playlist>>(tmpFileContent);
            }
            catch (Exception ex)
            {
                MessageService.ShowError(ex, "Failed to load playlists");
                return new List<Playlist>();
            }
        }
    }
}