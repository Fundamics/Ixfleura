namespace Ixfleura.Data.Entities
{
    /// <summary>
    /// The model for a suggestion.
    /// </summary>
    public class Suggestion
    {
        /// <summary>
        /// The id of the suggestion.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The Id of the guild where the suggestion was created.
        /// </summary>
        public ulong GuildId { get; set; }
        
        /// <summary>
        /// The id of the suggestion message sent by the bot.
        /// </summary>
        public ulong MessageId { get; set; }
        
        /// <summary>
        /// The id of the channel for the suggestion message.
        /// </summary>
        public ulong ChannelId { get; set; }
        
        /// <summary>
        /// The id of the user who created the suggestion
        /// </summary>
        public ulong SuggesterId { get; set; }
        
        /// <summary>
        /// The content of the suggestion
        /// </summary>
        public string Content { get; set; }
    }
}