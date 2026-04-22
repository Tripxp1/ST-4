using Stateless;

namespace BugPro;

public class Bug
{
    public enum State { NewDefect, Triage, Fix, Closed, Reopened }
    public enum Trigger { StartTriage, NoTime, NeedSeparateSolution, OtherProduct, NeedMoreInfo, FixDone, NotDefect, DoNotFix, Duplicate, NotReproducible, VerifyOk, VerifyFailed, Reopen }

    private readonly StateMachine<State, Trigger> _machine;
    private State _currentState;

    public Bug()
    {
        _machine = new StateMachine<State, Trigger>(State.NewDefect);
        _currentState = State.NewDefect;

        _machine.Configure(State.NewDefect)
            .Permit(Trigger.StartTriage, State.Triage);

        _machine.Configure(State.Triage)
            .Permit(Trigger.NoTime, State.Fix)
            .Permit(Trigger.NeedSeparateSolution, State.Fix)
            .Permit(Trigger.OtherProduct, State.Closed)
            .Ignore(Trigger.NeedMoreInfo)
            .Ignore(Trigger.StartTriage) 
            .Ignore(Trigger.Reopen);

        _machine.Configure(State.Fix)
            .Permit(Trigger.FixDone, State.Closed)
            .Permit(Trigger.NotDefect, State.Closed)
            .Permit(Trigger.DoNotFix, State.Closed)
            .Permit(Trigger.Duplicate, State.Closed)
            .Permit(Trigger.NotReproducible, State.Closed)
            .Permit(Trigger.Reopen, State.Reopened);

        _machine.Configure(State.Closed)
            .Permit(Trigger.Reopen, State.Reopened)
            .Ignore(Trigger.FixDone)
            .Ignore(Trigger.NotDefect)
            .Ignore(Trigger.DoNotFix)
            .Ignore(Trigger.Duplicate)
            .Ignore(Trigger.NotReproducible);

        _machine.Configure(State.Reopened)
            .Permit(Trigger.StartTriage, State.Triage)
            .Ignore(Trigger.Reopen) 
            .Permit(Trigger.FixDone, State.Closed);

        _machine.OnTransitioned(t => _currentState = t.Destination);
    }

    public State CurrentState => _currentState;

    public void StartTriage() => _machine.Fire(Trigger.StartTriage);
    public void NoTime() => _machine.Fire(Trigger.NoTime);
    public void NeedSeparateSolution() => _machine.Fire(Trigger.NeedSeparateSolution);
    public void OtherProduct() => _machine.Fire(Trigger.OtherProduct);
    public void NeedMoreInfo() => _machine.Fire(Trigger.NeedMoreInfo);
    public void FixDone() => _machine.Fire(Trigger.FixDone);
    public void NotDefect() => _machine.Fire(Trigger.NotDefect);
    public void DoNotFix() => _machine.Fire(Trigger.DoNotFix);
    public void Duplicate() => _machine.Fire(Trigger.Duplicate);
    public void NotReproducible() => _machine.Fire(Trigger.NotReproducible);
    public void VerifyOk() => _machine.Fire(Trigger.VerifyOk);
    public void VerifyFailed() => _machine.Fire(Trigger.VerifyFailed);
    public void Reopen() => _machine.Fire(Trigger.Reopen);
}

public static class Program
{
    public static void Main()
    {
        var bug = new Bug();
        Console.WriteLine($"Initial state: {bug.CurrentState}");

        bug.StartTriage();
        Console.WriteLine($"After StartTriage: {bug.CurrentState}");

        bug.NeedSeparateSolution();
        Console.WriteLine($"After NeedSeparateSolution: {bug.CurrentState}");

        bug.FixDone();
        Console.WriteLine($"After FixDone: {bug.CurrentState}");

        bug.Reopen();
        Console.WriteLine($"After Reopen: {bug.CurrentState}");

        bug.StartTriage();
        Console.WriteLine($"After StartTriage from Reopened: {bug.CurrentState}");

        bug.NeedSeparateSolution();
        Console.WriteLine($"After NeedSeparateSolution: {bug.CurrentState}");

        bug.FixDone();
        Console.WriteLine($"After FixDone: {bug.CurrentState}");

        var bug2 = new Bug();
        bug2.StartTriage();
        bug2.NeedSeparateSolution();
        bug2.NotDefect();
        Console.WriteLine($"Bug2 after NotDefect: {bug2.CurrentState}");
    }
}