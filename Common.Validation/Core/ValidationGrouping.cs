using System;
using System.Collections.Generic;

namespace Common.Validation.Core
{
    internal class ValidationGrouping<T>
    {
        public Type Key { get; set; }
        public List<ValidationRule<T>> Rules { get; set; }

        public ValidationGrouping()
        {
            Rules = new List<ValidationRule<T>>();
        }
    }
}
