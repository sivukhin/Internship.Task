namespace DataCore
{
    public class PlayerInfo
    {
        public virtual int PlayerId { get; set; }
        public virtual string Name { get; set; }
        public virtual int Frags { get; set; }
        public virtual int Kills { get; set; }
        public virtual int Deaths { get; set; }
    }
}