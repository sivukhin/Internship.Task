using System.Collections.Generic;
using System.Linq;

namespace DataCore
{
    //TODO: serversInfo now without endpoint
    public class ServerInfo
    {
        public virtual string ServerId { get; set; }
        public virtual string Name { get; set; }
        public virtual IList<GameMode> GameModes { get; set; }

        protected bool Equals(ServerInfo other)
        {
            return string.Equals(ServerId, other.ServerId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServerInfo)obj);
        }

        public override int GetHashCode()
        {
            return ServerId?.GetHashCode() ?? 0;
        }
    }
}