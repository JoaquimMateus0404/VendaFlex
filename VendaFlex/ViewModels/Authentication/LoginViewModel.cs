using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendaFlex.Core.Interfaces;

namespace VendaFlex.ViewModels.Authentication
{
    public class LoginViewModel
    {
        private readonly IUserService _userService;
        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
        }
    }
}
