using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Question
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Grade { get; set; }
        public int Unit { get; set; }
        public string ProbabilityOrStatistic { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreateBy { get; set; }
    }
}
