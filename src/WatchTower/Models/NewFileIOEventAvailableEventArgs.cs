namespace WatchTower
{
    public class NewFileIOEventAvailableEventArgs : System.EventArgs
    {
        public RawEventDetails RawEventDetails { get; }

        public NewFileIOEventAvailableEventArgs(RawEventDetails rawEventDetails)
        {
            RawEventDetails = rawEventDetails;
        }
    }
}
