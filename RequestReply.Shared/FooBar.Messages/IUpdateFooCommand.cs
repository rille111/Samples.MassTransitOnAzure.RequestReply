using System;

namespace RequestReply.Shared.FooBar.Messages
{
    public interface IUpdateFooCommand
    {
        Guid Id { get; set; }
        string Text { get; set; }
        DateTime TimeStampSent { get; set; }
    }
}