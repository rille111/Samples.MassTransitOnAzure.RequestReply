using System.Collections.Generic;

namespace RequestReply.Shared.Messages
{
    /// <summary>
    /// One command containing a number of bars to serve to some consumer..
    /// </summary>
    public class ServeBarsCommand
    {
        public string BarOwner { get; set; }
        public List<Bar> Bars { get; set; } = new List<Bar>();
    }

    public class Bar
    {
        public string Flavour { get; set; } = "ChocolateBar";
    }
}
