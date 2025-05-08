using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.ModelView;

namespace Shared.ModelView
{
    public class ExamView
    {
        public Guid ExamId { get; set; } 
        public string Name { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int TotalQuestions { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<QuestionOptionView> Questions { get; set; } = new List<QuestionOptionView>();
    }
}
