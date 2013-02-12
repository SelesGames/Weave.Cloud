using Common.Validation.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Validation
{
    public class ValidationEngine
    {
        Dictionary<Type, object> dict = new Dictionary<Type, object>();

        public void AddRule<T>(Func<T, bool> rule, string onInvalidMessage)
        {
            var key = typeof(T);

            ValidationGrouping<T> group;

            if (dict.ContainsKey(key))
                group = (ValidationGrouping<T>)dict[key];
            else
            {
                group = new ValidationGrouping<T>{ Key = key };
                dict.Add(key, group);
            }

            group.Rules.Add(ValidationRule.Create(rule, onInvalidMessage));
        }

        public void Validate<T>(T obj)
        {
            List<string> ruleViolations = new List<string>();

            var key = typeof(T);
            if (!dict.ContainsKey(key))
                return;

            var group = (ValidationGrouping<T>)dict[key];
            foreach (var rule in group.Rules)
            {
                bool doesRulePass = false;
                bool doesRuleFaultWithException = false;
                try
                {
                    doesRulePass = rule.Evaluate(obj);
                }
                catch
                {
                    doesRuleFaultWithException = true;
                }

                if (doesRuleFaultWithException)
                    ruleViolations.Add(string.Format("RULE ERROR: {0}", rule.CreateMessage()));

                else if (!doesRulePass)
                    ruleViolations.Add(rule.CreateMessage());
            }

            if (ruleViolations.Any())
                throw new ValidationException { RuleViolations = ruleViolations };
        }
    }
}
