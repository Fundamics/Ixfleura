namespace Ixfleura.Data.Entities
{
    public class Suggestion
    {
        public ulong Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong SuggesterId { get; set; }
        public string Content { get; set; }
    }
}