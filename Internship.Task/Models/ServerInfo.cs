using System.Collections.Generic;

namespace Internship.Models
{
    //TODO: serversInfo now without endpoint
    public class ServerInfo
    {
        public string Name { get; set; }
        public List<string> GameModes { get; set; }
    }
}