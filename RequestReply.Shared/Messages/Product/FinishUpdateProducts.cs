using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestReply.Shared.Messages.Product
{
    public class FinishUpdateProducts
    {
        public Guid CorrelationId { get; set; }
        public string CorrelateUniqueName { get; set; }

    }
}
