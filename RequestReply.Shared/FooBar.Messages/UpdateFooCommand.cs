﻿using System;

namespace RequestReply.Shared.FooBar.Messages
{
    public class UpdateFooCommand : IUpdateFooCommand
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStampSent { get; set; }
    }
}
