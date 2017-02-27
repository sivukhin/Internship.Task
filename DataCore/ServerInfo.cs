using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataCore
{
    public class ServerInfo
    {
        public virtual string ServerId { get; set; }
        public virtual string Name { get; set; }
        public virtual IList<GameMode> GameModes { get; set; }

        public ServerInfo() { }

        [JsonConstructor]
        public ServerInfo(string serverId, string name, IEnumerable<string> gameModes)
        {
            ServerId = serverId;
            Name = name;
            GameModes = gameModes.Select(mode => new GameMode(mode)).ToList();
        }

        private bool Equals(ServerInfo other)
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