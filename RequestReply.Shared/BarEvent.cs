using System;

namespace RequestReply.Shared
{
    public interface IBarEvent
    {
        Guid Id { get; set; }
        string Text { get; set; }
        DateTime TimeStampSent { get; set; }
    }

    public class BarEvent : IBarEvent
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStampSent { get; set; }
    }
}
