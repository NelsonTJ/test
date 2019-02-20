using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    // goal function: return true means goal accomplished
    public delegate bool GoalFunc<T>(Domain domain, T param);
    // rule function
    public delegate TResult RuleFunc<TResult>(Domain domain);
    // action function: return true means action succeeded
    public delegate bool ActionFunc<T, TResult>(Domain domain, Goal<T> goal, Rule<TResult> rule, T param);

    public class Domain
    {
        public ICollection<string> Records { get; }

        public Domain()
        {
            Records = new List<string>();
        }
    }

    public class Goal<T>
    {
        public string Name { get; set; }
        public GoalFunc<T> Content { get; set; }

        // A goal: if domain.Records contain lxj, goal accomplished.
        public static GoalFunc<T> GF_FindLxj = (domain, param) =>
        {
            return domain.Records.Any(r => r == "lxj");
        };

        public Goal(string name, GoalFunc<T> gf)
        {
            Name = name;
            Content = gf;
        }

        public bool IsAccomplished(Domain domain, T param)
        {
            bool result = Content(domain, param);
            return result;
        }
    }

    public class Rule<TResult>
    {
        public string Name { get; set; }
        public RuleFunc<TResult> Content { get; set; }

        // A rule: input a string from console
        public static RuleFunc<string> RF_InputString = (domain) =>
        {
            string s = Console.ReadLine();
            domain.Records.Add(s);
            return s;
        };

        public Rule(string name, RuleFunc<TResult> rf)
        {
            Name = name;
            Content = rf;
        }

        public TResult Apply(Domain domain)
        {
            return Content(domain);
        }
    }

    public class Action<T, TResult>
    {
        public string Name { get; set; }
        public ActionFunc<T, TResult> Content { get; set; }
        public Rule<TResult> Rule { get; set; }
        public Goal<T> Goal { get; set; }

        // An action: execute rule action
        public static ActionFunc<string, string> AF_ExecuteRule = (domain, goal, rule, param) =>
        {
            while (!goal.IsAccomplished(domain, param))
            {
                Console.WriteLine("Input string:");
                string s = rule.Apply(domain);
                param = s;
            }
            Console.WriteLine("Goal accomplished.");
            return true;
        };

        public Action(string name, ActionFunc<T, TResult> af)
        {
            Name = name;
            Content = af;
        }

        public bool Execute(Domain domain, Goal<T> goal, Rule<TResult> rule, T param)
        {
            return Content(domain, goal, rule, param);
        }
    }

    public class DigitalTwin<TRuleResult>
    {
        public Action<TRuleResult> Action { get; set; }

        public DigitalTwin()
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Domain domain = new Domain();
            Goal<string> goal = new Goal<string>("Find lxj", Goal<string>.GF_FindLxj);
            Rule<string> rule = new Rule<string>("Input string", Rule<string>.RF_InputString);
            Action<string, string> action = new Action<string, string>("test action", Action<string, string>.AF_ExecuteRule);

            action.Execute(domain, goal, rule, "");

            Console.ReadLine();
        }
    }
}
