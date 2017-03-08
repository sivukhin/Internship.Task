using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace DataCore
{
    public class ServerInfoConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var serverInfo = value as ServerInfo;
            if (serverInfo == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            serializer.Serialize(writer, new
            {
                serverInfo.ServerId, serverInfo.Name,
                GameModes = serverInfo.GameModes.Select(mode => mode.ModeName).ToList()
            });
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ServerInfo).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override bool CanRead => false;
        public override bool CanWrite => true;
    }


    [JsonConverter(typeof(ServerInfoConverter))]
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