using System;

namespace Ixfleura.Data.Entities
{
    /// <summary>
    /// The model for a tag.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// The guild id of the tag.
        /// </summary>
        public ulong GuildId { get; set; }
        
        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The content of the tag.
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// The <see cref="DateTimeOffset"/> when the tag was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
        
        /// <summary>
        /// The <see cref="DateTimeOffset"/> when the tag was edited.
        /// </summary>
        public DateTimeOffset EditedAt { get; set; }
        
        /// <summary>
        /// The number of uses this tag has.
        /// </summary>
        public uint Uses { get; set; }
    }
}
