using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;

namespace Idunas.DanceMusicPlayer.Framework.ListView
{
    public class SimpleItemTouchHelperCallback : ItemTouchHelper.Callback
    {
        private readonly ITouchableListViewAdapter _adapter;

        public SimpleItemTouchHelperCallback(ITouchableListViewAdapter adapter)
        {
            _adapter = adapter;
        }

        public override bool IsLongPressDragEnabled
        {
            get
            {
                return true;
            }
        }

        public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            var dragFlags = ItemTouchHelper.Up | ItemTouchHelper.Down;
            return MakeMovementFlags(dragFlags, 0);
        }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            _adapter.ItemMoved(viewHolder.AdapterPosition, target.AdapterPosition);
            return true;
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
        {

        }
    }
}