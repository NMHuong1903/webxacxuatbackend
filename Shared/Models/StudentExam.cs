using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class StudentExam
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StudentId { get; set; }
        public Guid ExamId { get; set; }
        public DateTime StartAt { get; set; } = DateTime.Now;
        public DateTime CompleteAt { get; set; }
        public int Score { get; set; }
    }
}
