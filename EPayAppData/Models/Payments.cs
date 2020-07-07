using System;
using System.Collections.Generic;
using System.Text;

namespace EPayAppData.Models
{
    public class Payments
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CapturedAt { get; set; }
        public bool Status { get; set; }
        public decimal Amount { get; set; }
        public long ChatId { get; set; }
    }
}
