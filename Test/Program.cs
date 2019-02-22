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
    //   In an area, give a param, return if the goal is accomplished
    public delegate bool GoalFunc<TArea, T>(TArea area, T param);

    // Rule function:
    //   Define a rule in an area,  input a param, output a result.
    public delegate TResult RuleFunc<TArea, T, TResult>(TArea area, T param);

    // Action function: 
    //   In an area, given a param (usually a output from rule), take an action.
    public delegate void ActionFunc<TArea, T>(TArea area, T param);

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

    public class Rule<TArea, T, TResult>
    {
        public string Name { get; set; }
        public RuleFunc<TArea, T, TResult> Content { get; set; }

        public Rule(string name, RuleFunc<TArea, T, TResult> rf)
        {
            Name = name;
            Content = rf;
        }

        public TResult Apply(TArea area, T param)
        {
            return Content(area, param);
        }
    }

    public class Action<TArea, T>
    {
        public string Name { get; set; }
        public ActionFunc<TArea, T> Content { get; set; }

        public Action(string name, ActionFunc<TArea, T> af)
        {
            Name = name;
            Content = af;
        }

        public void Execute(TArea area, T param)
        {
            Content(area, param);
        }
    }

    // GRA: Goal-Rule-Action
    //   TArea: Area type
    //   T: Input param type
    public abstract class GRA<TArea, T>
    {
        public virtual void Run(TArea area, T param) { }
    }

    // GRA: Goal-Rule-Action
    //   TArea: Area type
    //   T: Input param type
    //   TResult: rule ouput type
    public class GRA<TArea, T, TResult> : GRA<TArea, T>
    {
        public Goal<TArea, T> Goal { get; set; }
        public Rule<TArea, T, TResult> Rule { get; set; }
        public Action<TArea, TResult> Action { get; set; }

        public GRA(Goal<TArea, T> goal,
            Rule<TArea, T, TResult> rule,
            Action<TArea, TResult> action)
        {
            Goal = goal;
            Rule = rule;
            Action = action;
        }

        public override void Run(TArea area, T param)
        {
            if (Goal == null)
                return;
            TResult result = default(TResult);

            while (!Goal.IsAccomplished(area, param))
            {
                if (Rule != null)
                    result = Rule.Apply(area, param);
                if (Action != null)
                    Action.Execute(area, result);
            }
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
        // no rules available
    }

    public class DomainActions
    {
        // Prompt to input a string.
        public static void AF_InputStr(Domain domain, string param)
        {
            Console.WriteLine("Input string:");
            string str = Console.ReadLine();
            domain.Records.Add(str);
        }
    }
    

    // digital twin
    //   1: Dynamic management
    //   2: Specialized management
    //   3: Emergency management
    //   
    public class DigitalTwin<TRuleResult>
    {

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
            Action<Domain, string> action = 
                new Action<Domain, string>("test action", DomainActions.AF_InputStr);

            GRA<Domain, string> gra = new GRA<Domain, string, string>(goal, null, action);

            gra.Run(domain, null);

            Console.WriteLine("OK");
            Console.ReadLine();
        }
    }
}
