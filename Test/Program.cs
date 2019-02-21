using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    /*
     * Spatial related goals
     * Goal: CheckGRPDataAt(K)
     * Rule: K - Kn <= 20m
     * Action: 
     *   if Rule OK, return GRPDataAt(Kn);
     *   else request new GRP data
     *   
     * Goal: CheckExcvationFaceLocation()
     * Rule: CurrentDate - LastRecordDate <= 1
     * Action:
     *   if Rule is OK, return last record location
     *   else request new locaton data
     * 
     * Goal: CheckGeologicalSketchData(K)
     * Rule: if (RockClass==3)  K - Kn <= 10m,  if (RockClass==4) ....
     * Action:
     *   if Rule is OK, return last record location
     *   else request new data
     * 
     */




    // Goal function:
    //   in an area, give a param, return if the goal is accomplished
    public delegate bool GoalFunc<TArea, T>(TArea area, T param);

    // Rule function:
    //   define a rule in an area, 
    //   the rule output is a result
    public delegate TResult RuleFunc<TArea, TResult>(TArea area);

    // Action function: 
    //   In an area, take an action according to the given goal and rule
    //   return true means action succeeded (or goal accomplished)
    public delegate bool ActionFunc<TArea, T, TResult>(
        TArea area, 
        Goal<TArea, T> goal, 
        Rule<TArea, TResult> rule);

    public class Goal<TArea, T>
    {
        public string Name { get; set; }
        public GoalFunc<TArea, T> Content { get; set; }

        public Goal(string name, GoalFunc<TArea, T> gf)
        {
            Name = name;
            Content = gf;
        }

        public bool IsAccomplished(TArea area, T param)
        {
            return Content(area, param);
        }
    }

    public class Rule<TArea, TResult>
    {
        public string Name { get; set; }
        public RuleFunc<TArea, TResult> Content { get; set; }

        public Rule(string name, RuleFunc<TArea, TResult> rf)
        {
            Name = name;
            Content = rf;
        }

        public TResult Apply(TArea area)
        {
            return Content(area);
        }
    }

    public class Action<TArea, T, TResult>
    {
        public string Name { get; set; }
        public ActionFunc<TArea, T, TResult> Content { get; set; }

        public Action(string name, ActionFunc<TArea, T, TResult> af)
        {
            Name = name;
            Content = af;
        }

        public bool Execute(TArea area, Goal<TArea, T> goal, Rule<TArea, TResult> rule)
        {
            return Content(area, goal, rule);
        }
    }

    public class Domain
    {
        public ICollection<string> Records { get; }

        public Domain()
        {
            Records = new List<string>();
        }
    }

    public class DomainGoals
    {
         // A goal: if domain.Records contain lxj, goal accomplished.
        public static bool GF_FindLxj(Domain domain, string param)
        {
            return domain.Records.Any(r => r == "lxj");
        }
    }

    public class DomainRules
    {
        // A rule: input a string from console
        public static string RF_InputString(Domain domain)
        {
            string s = Console.ReadLine();
            domain.Records.Add(s);
            return s;
        }

        public static double RF_Distance()
        {
            return 1.0;
        }
    }

    public class DomainActions
    {
        // Execute the action until the goal is accomplished.
        public static bool AF_Excute(Domain domain,
            Goal<Domain, string> goal)
        {
            string param = "";
            while (!goal.IsAccomplished(domain, param))
            {
                Console.WriteLine("Input string:");
                param = Console.ReadLine();
            }
            Console.WriteLine("Goal accomplished.");
            return true;
        }

        // Execute the action with the rule until the goal is accomplished.
        public static bool AF_ExecuteRule(Domain domain, 
            Goal<Domain, string> goal,
            Rule<Domain, string> rule)
        {
            string param = "";
            while (!goal.IsAccomplished(domain, param))
            {
                Console.WriteLine("Input string:");
                param = rule.Apply(domain);
            }
            Console.WriteLine("Goal accomplished.");
            return true;
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
            Goal<Domain, string> goal = 
                new Goal<Domain, string>("Find lxj", DomainGoals.GF_FindLxj);
            Rule<Domain, string> rule = 
                new Rule<Domain, string>("Input string", DomainRules.RF_InputString);
            Action<Domain, string, string> action = 
                new Action<Domain, string, string>("test action", DomainActions.AF_Excute);

            action.Execute(domain, goal, rule);

            Console.ReadLine();
        }
    }
}
