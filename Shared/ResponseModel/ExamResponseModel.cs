using Shared.ModelView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseModel
{
    public class ExamResponseModel
    {
        public List<ExamView> ExamViews { get; set; }
        public int TotalCount { get; set; }
    }
}
