using System;

namespace RequestReply.Shared.Messages
{
    public class BarEvent : IBarEvent
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStampSent { get; set; }
    }
}
