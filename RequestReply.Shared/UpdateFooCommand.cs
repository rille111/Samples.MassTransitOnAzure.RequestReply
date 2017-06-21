using System;

namespace RequestReply.Shared
{
    public interface IUpdateFooCommand
    {
        Guid Id { get; set; }
        string Text { get; set; }
        DateTime TimeStampSent { get; set; }
    }

    public class UpdateFooCommand : IUpdateFooCommand
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStampSent { get; set; }
    }

    public class UpdateFooVersion2Command : IUpdateFooCommand
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStampSent { get; set; }
    }

}
