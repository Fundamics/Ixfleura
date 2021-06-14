namespace Ixfleura.Data.Entities
{
    public class Suggestion
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong SuggesterId { get; set; }
        public string Content { get; set; }
    }
}