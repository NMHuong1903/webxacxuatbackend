using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseModel
{
    public class UserResponseModel
    {
        public List<User> Users { get; set; }
        public int TotalCount { get; set; }
    }
}
