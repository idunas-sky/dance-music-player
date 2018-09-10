﻿using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Idunas.DanceMusicPlayer.Fragments.PlaylistDetails;
using Idunas.DanceMusicPlayer.Fragments.PlaylistEditor;
using Idunas.DanceMusicPlayer.Models;

namespace Idunas.DanceMusicPlayer.Fragments.Playlists
{
    public class PlaylistsFragment : NavFragment
    {
        private RecyclerView _rvPlaylists;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.Playlists, container, false);

            _rvPlaylists = view.FindViewById<RecyclerView>(Resource.Id.rvPlaylists);
            _rvPlaylists.HasFixedSize = true;
            _rvPlaylists.SetLayoutManager(new LinearLayoutManager(Context));

            var adapter = new PlaylistsRvAdapter();
            adapter.ItemClick += (sender, e) => NavigateTo<PlaylistDetailsFragment>(f => f.Playlist = e);
            _rvPlaylists.SetAdapter(adapter);

            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.playlists, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_new)
            {
                NavigateTo<PlaylistEditorFragment>(initalizer: f =>
                {
                    f.Playlist = new Playlist();
                    f.IsNew = true;
                });
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}