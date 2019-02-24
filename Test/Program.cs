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
    public delegate bool GoalFunc(object area);

    // Rule function:
    //   Define a rule in an area,  input a param, output a result.
    public delegate object RuleFunc(object area);

    // Action function: 
    //   In an area, given a param (usually a output from rule), take an action.
    public delegate void ActionFunc(object area, object param);


    // GRA: Goal-Rule-Action
    //   TArea: Area type
    //   T: Input param type
    //   TResult: rule ouput type
    public class GRA
    {
        public GoalFunc GoalFunc { get; set; }
        public RuleFunc RuleFunc { get; set; }
        public ActionFunc ActionFunc { get; set; }
        public TimeSpan Span { get; set; }
        public TimeSpan WaitedTime { get; set; }
        public string RuleDesc { get; set; }

        public bool IsGoalAccomplished { get; set; }

        public GRA(GoalFunc gf, RuleFunc rf, ActionFunc af, TimeSpan span)
        {
            GoalFunc = gf;
            RuleFunc = rf;
            ActionFunc = af;

            Span = span;
            WaitedTime = span;
        }
        public virtual void Execute(object area)
        {
            if (GoalFunc != null)
                IsGoalAccomplished = GoalFunc(area);

            if (!IsGoalAccomplished)
            {
                object result = null;
                if (RuleFunc != null)
                    result = RuleFunc(area);
                if (ActionFunc != null)
                    ActionFunc(area, result);
            }
        }
    }

    // digital twin
    //   1: Dynamic management
    //   2: Specialized management
    //   3: Emergency management
    //   
    public class DigitalTwin
    {
        public ICollection<GRA> GRAs { get; set; }
        public bool Exit { get; set; }
        public object Area { get; set; }

        public DigitalTwin(object area)
        {
            GRAs = new List<GRA>();
            Area = area;
        }

        public void Run()
        {
            DateTime lastTime = new DateTime();
            DateTime curTime = new DateTime();
            lastTime = DateTime.Now;

            while (Exit == false)
            {
                curTime = DateTime.Now;
                TimeSpan span = curTime - lastTime;

                foreach (GRA gra in GRAs)
                {
                    if (!gra.IsGoalAccomplished)
                    {
                        gra.WaitedTime += span;
                        if (gra.WaitedTime >= gra.Span)
                        {
                            gra.Execute(Area);
                            gra.WaitedTime = TimeSpan.Zero;
                        }
                    }
                }

                lastTime = curTime;
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
        public static bool GF_FindLxj(object area)
        {
            Domain domain = area as Domain;
            if (domain == null)
                return false;

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
        public static void AF_InputStr(object area, object param)
        {
            Domain domain = area as Domain;
            if (domain == null)
                return;

            Console.WriteLine("Input string:");
            string str = Console.ReadLine();
            domain.Records.Add(str);
        }
    }
    


    class Program
    {
        static void Main(string[] args)
        {
            Domain domain = new Domain();
            DigitalTwin dt = new DigitalTwin(domain);

            GRA gra = new GRA(DomainGoals.GF_FindLxj, null,
                DomainActions.AF_InputStr, new TimeSpan(0,0,10));
            dt.GRAs.Add(gra);
            dt.Run();

            Console.WriteLine("OK");
            Console.ReadLine();
        }
    }
}
