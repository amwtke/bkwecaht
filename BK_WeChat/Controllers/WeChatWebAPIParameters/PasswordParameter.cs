﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BK.WeChat.Controllers.WeChatWebAPIParameters
{
    public class PasswordParameter: BaseParameter
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
    }
}