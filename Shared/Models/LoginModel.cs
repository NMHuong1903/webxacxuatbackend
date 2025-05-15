using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class LoginModel
    {
        [DisplayName("Tên đăng nhập")]
        public string UserName { get; set; }
        [DisplayName("Mật khẩu")]
        public string Password { get; set; }
    }
}
