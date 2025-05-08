using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class StudentAnswer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StudentExamId { get; set; }
        public Guid QuestionId { get; set; }
        public Guid OptionId { get; set; }
    }
}
