using Android.Support.V7.Widget;
using System.Collections.Generic;

namespace Idunas.DanceMusicPlayer.Framework.ListView
{
    public abstract class RecyclerViewAdapterBase<T> : RecyclerView.Adapter where T : class
    {
        protected abstract IList<T> Items { get; }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ((IRecyclerViewViewHolder<T>)holder).BindData(GetItem(position));
        }

        public override int ItemCount
        {
            get
            {
                return Items.Count;
            }
        }

        protected T GetItem(int position)
        {
            if (Items.Count > position)
            {
                return Items[position];
            }

            return null;
        }
    }
}