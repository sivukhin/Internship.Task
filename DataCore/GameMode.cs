namespace DataCore
{
    public class GameMode
    {
        public virtual string ModeName { get; set; }

        public GameMode() : this("") { }

        public GameMode(string modeName)
        {
            ModeName = modeName;
        }
    }
}
