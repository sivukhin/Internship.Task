using System.Collections.Generic;
using System.Linq;

namespace StatisticServer.Models
{
    //TODO: serversInfo now without endpoint
    public class ServerInfo
    {
        public string Name { get; set; }
        public List<string> GameModes { get; set; }

        protected bool Equals(ServerInfo other)
        {
            return string.Equals(Name, other.Name) && GameModes.SequenceEqual(other.GameModes);
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
            unchecked
            {
                return (Name?.GetHashCode() ?? 0) * 397;
            }
        }
    }
}