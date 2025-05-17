using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.ModelView;

namespace Shared.ResponseModel
{
    public class QuestionResponseModel
    {
        public List<QuestionOptionView> questionOptionViews { get; set; }
        public int TotalCount { get; set; }
    }
}
