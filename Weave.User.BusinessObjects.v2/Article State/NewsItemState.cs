using Store = Weave.User.DataStore.v2;

namespace Weave.User.BusinessObjects.v2
{
    public class NewsItemState
    {
        Store.NewsItemState state;

        public NewsItemState() : this(new Store.NewsItemState()) { }

        public NewsItemState(Store.NewsItemState state)
        {
            this.state = state;
        }

        public Store.NewsItemState Inner { get { return state; } }

        public string Key { get; set; }

        public bool IsFavorite
        {
            get { return state.IsFavorite; }
            set { state.IsFavorite = value; }
        }

        public bool HasBeenViewed
        {
            get { return state.HasBeenViewed; }
            set { state.HasBeenViewed = value; }
        }
    }
}