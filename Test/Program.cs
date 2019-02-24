using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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


    /// <summary>
    /// GRA: Goal-Rule-Action.
    ///   Execute the Goal, according to the Rule, take the Action.
    ///   Goal, Rule and Action are given by functions
    /// </summary>
    public class GRA
    {
        // goal, rule and action functions
        public GoalFunc GoalFunc { get; set; }
        public RuleFunc RuleFunc { get; set; }
        public ActionFunc ActionFunc { get; set; }

        // goal, rule and action descriptions
        public string GoalDesc { get; set; }
        public string RuleDesc { get; set; }
        public string ActionDesc { get; set; }

        // Span is the desired execution time span of the GRA if goal is not accomplished.
        // WaitedTime is the time GRA has waited.
        // If WaitedTime>=Span, then the GRA should be executed.
        public TimeSpan Span { get; set; }
        public TimeSpan WaitedTime { get; set; }

        // TryCount is the GRA has been executed before goal is accomplished.
        // MaxTries is the maximum tries
        // If TryCount >= MaxTries, the GRA will be aborted.
        public int TryCount { get; set; }
        public int MaxTries { get; set; }

        // Indicated if the goal is accomplished, and if the goal is aborted.
        public bool IsGoalAccomplished { get; set; }
        public bool Abort { get; set; }

        public GRA(GoalFunc gf, RuleFunc rf, ActionFunc af, TimeSpan span)
        {
            GoalFunc = gf;
            RuleFunc = rf;
            ActionFunc = af;
            Span = span;
            MaxTries = int.MaxValue;

            Init();
        }

        protected void Init()
        {
            WaitedTime = Span;
            TryCount = 0;
            IsGoalAccomplished = false;
            Abort = false;
        }

        public virtual void Execute(object area, TimeSpan span)
        {
            if (!IsGoalAccomplished && !Abort)
            {
                WaitedTime += span;
                if (WaitedTime < Span)
                    return;

                WaitedTime = TimeSpan.Zero;

                if (GoalFunc != null)
                {
                    IsGoalAccomplished = GoalFunc(area);
                    if (IsGoalAccomplished)
                        return;
                }

                object result = null;
                if (RuleFunc != null)
                    result = RuleFunc(area);
                if (ActionFunc != null)
                    ActionFunc(area, result);

                TryCount++;
                if (TryCount >= MaxTries)
                    Abort = true;
            }
        }
    }

    /// <summary>
    /// GRA_R: Repetitive GRA.
    ///   Reset the GRA if the repeat time span is reached,
    ///     no matter the status of the GRA (Abort, or not accomplished).
    /// </summary>
    public class GRA_R : GRA
    {
        // RepeatSpan is the desired repeat execution time span of the GRA.
        // RepeatWaitedTime is the time GRA has waited.
        // If RepeatWaitedTime>=RepeatSpan, then the GRA will be reset.
        public TimeSpan RepeatSpan { get; set; }
        public TimeSpan RepeatWaitedTime { get; set; }

        // RepeatCount is the GRA has been repeated.
        // MaxRepeatCount is the maximum repeated number.
        // If RepeatCount >= MaxRepeatCount, the GRA will be aborted.
        public int RepeatCount { get; set; }
        public int MaxRepeatCount { get; set; }

        public GRA_R(GoalFunc gf, RuleFunc rf, ActionFunc af, 
            TimeSpan span, TimeSpan repeatSpan)
            :base(gf, rf, af, span)
        {
            RepeatSpan = repeatSpan;
            MaxRepeatCount = int.MaxValue;

            RepeatWaitedTime = repeatSpan;
            RepeatCount = 0;
        }

        public override void Execute(object area, TimeSpan span)
        {
            if (RepeatCount >= MaxRepeatCount)
            {
                Abort = true;
                return;
            }

            RepeatCount++;
            RepeatWaitedTime += span;
            if (RepeatWaitedTime >= RepeatSpan)
            {
                base.Init();
                RepeatWaitedTime = TimeSpan.Zero;
            }

            base.Execute(area, span);
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
        public Thread ThreadDT { get; set; }

        public DigitalTwin(object area)
        {
            GRAs = new List<GRA>();
            Area = area;
        }

        public void Run()
        {
            ThreadDT = new Thread(ProcessGRAs);
            ThreadDT.Start();
        }

        public void Terminate()
        {
            Exit = true;
        }

        void ProcessGRAs()
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
                    gra.Execute(Area, span);
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

            GRA gra = new GRA_R(DomainGoals.GF_FindLxj, null,
                DomainActions.AF_InputStr, new TimeSpan(0,0,10), new TimeSpan(0,1,0));
            dt.GRAs.Add(gra);
            dt.Run();

            Thread.Sleep(1000*60);
            dt.Terminate();
            Console.WriteLine("Exit");
            Console.ReadLine();
        }
    }
}
