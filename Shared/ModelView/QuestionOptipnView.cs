using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models;

namespace Shared.ModelView
{
    public class QuestionOptionView
    {
        public Guid QuestionId { get; set; }
        public string Content { get; set; }
        public int Grade { get; set; }
        public int Unit { get; set; }
        public string ProbabilityOrStatistic { get; set; } = string.Empty;
        public Guid CreateBy { get; set; }
        public List<Option> Options { get; set; }
    }
}
