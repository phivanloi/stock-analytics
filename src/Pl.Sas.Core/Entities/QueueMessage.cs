using System.Collections.Generic;

namespace Pl.Sas.Core.Entities
{
    public class QueueMessage
    {
        public string Id { get; set; }

        public Dictionary<string, string> KeyValues { get; set; } = new Dictionary<string, string>();
    }
}