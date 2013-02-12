using System;
using System.Collections.Generic;

namespace Common.Validation
{
    public class ValidationException : Exception
    {
        public List<string> RuleViolations { get; set; }
    }
}
