﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Models
{
    internal class LoginRequestModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
