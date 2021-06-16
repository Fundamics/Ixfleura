using System;

namespace Ixfleura.Data.Entities
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public ulong MessageId { get; set; }
        public ulong CandidateId { get; set; }
        public ulong AdvocateId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
