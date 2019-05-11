using Android.Support.V7.Widget;
using Android.Views;

namespace Idunas.DanceMusicPlayer.Framework.ListView
{
    public abstract class RecyclerViewViewHolderBase<T> : RecyclerView.ViewHolder, IRecyclerViewViewHolder<T>
    {
        public RecyclerViewViewHolderBase(View itemView) : base(itemView)
        {
        }

        public abstract void BindData(T data);
    }
}