using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.SearchModel
{
    public class QuestionSearchModel
    {
        public int pageIndex { get; set; } = 1;
        public int Grade { get; set; }
        public int Unit { get; set; }
        public string ProbabilityOrStatistic { get; set; } = string.Empty;
        public string Content { get; set; }
    }
}
