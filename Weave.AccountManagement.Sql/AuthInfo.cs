﻿using System;

namespace Weave.AccountManagement.Sql
{
    public class AuthInfo
    {
        public Guid UserId { get; set; }
        public string FacebookAuthToken { get; set; }
        public string TwitterAuthToken { get; set; }
        public string MicrosoftAuthToken { get; set; }
        public string GoogleAuthToken { get; set; }
    }
}