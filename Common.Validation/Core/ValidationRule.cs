using System;

namespace Common.Validation.Core
{
    internal class ValidationRule
    {
        internal static ValidationRule<T> Create<T>(Func<T, bool> rule, string onInvalidMessage)
        {
            return new ValidationRule<T>
            {
                Rule = rule,
                OnInvalidMessage = onInvalidMessage
            };
        }
    }

    internal class ValidationRule<T>
    {
        public Func<T, bool> Rule { get; set; }
        public string OnInvalidMessage { get; set; }

        public bool Evaluate(T obj)
        {
            return Rule(obj);
        }

        public string CreateMessage()
        {
            return string.Format("{0}: {1}", typeof(T).Name, OnInvalidMessage);
        }
    }
}
