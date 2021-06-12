using System;

namespace Ixfleura.Data.Entities
{
    public class Tag
    {
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset EditedAt { get; set; }
        public uint Uses { get; set; }
    }
}
