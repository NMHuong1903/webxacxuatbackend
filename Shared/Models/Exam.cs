using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Exam
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int TotalQuestions { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Question> Questions { get; set; } = new List<Question>();
    }
}
