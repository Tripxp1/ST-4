using Stateless;

namespace BugPro;

public enum IssuePhase
{
    New,
    Analysis,
    InProgress,
    NeedMoreInfo,
    NotReproducible,
    Duplicate,
    Fixed,
    Closed,
    Reopened,
    Deferred
}

public enum TransitionSignal
{
    StartAnalysis,
    RequestInfo,
    MarkDuplicate,
    MarkNotReproducible,
    StartFix,
    Fix,
    ConfirmResolved,
    RejectFix,
    Reopen,
    Defer,
    ReturnToAnalysis
}

public sealed class Bug
{
    private IssuePhase phase;
    private readonly StateMachine<IssuePhase, TransitionSignal> engine;

    public Bug()
    {
        phase = IssuePhase.New;
        engine = new StateMachine<IssuePhase, TransitionSignal>(
            () => phase,
            s => phase = s);

        BuildWorkflow();
    }

    public IssuePhase State => phase;

    public void StartAnalysis() => Apply(TransitionSignal.StartAnalysis);

    public void RequestInfo() => Apply(TransitionSignal.RequestInfo);

    public void MarkDuplicate() => Apply(TransitionSignal.MarkDuplicate);

    public void MarkNotReproducible() => Apply(TransitionSignal.MarkNotReproducible);

    public void StartFix() => Apply(TransitionSignal.StartFix);

    public void Fix() => Apply(TransitionSignal.Fix);

    public void ConfirmResolved() => Apply(TransitionSignal.ConfirmResolved);

    public void RejectFix() => Apply(TransitionSignal.RejectFix);

    public void Reopen() => Apply(TransitionSignal.Reopen);

    public void Defer() => Apply(TransitionSignal.Defer);

    public void ReturnToAnalysis() => Apply(TransitionSignal.ReturnToAnalysis);

    public bool CanStartAnalysis() => engine.CanFire(TransitionSignal.StartAnalysis);

    public bool CanFix() => engine.CanFire(TransitionSignal.Fix);

    public bool CanClose() => engine.CanFire(TransitionSignal.ConfirmResolved);

    public bool CanReopen() => engine.CanFire(TransitionSignal.Reopen);

    public bool CanReturnToAnalysis() => engine.CanFire(TransitionSignal.ReturnToAnalysis);

    private void Apply(TransitionSignal signal) => engine.Fire(signal);

    private void BuildWorkflow()
    {
        engine.Configure(IssuePhase.New)
            .Permit(TransitionSignal.StartAnalysis, IssuePhase.Analysis)
            .Permit(TransitionSignal.Defer, IssuePhase.Deferred);

        engine.Configure(IssuePhase.Analysis)
            .Permit(TransitionSignal.RequestInfo, IssuePhase.NeedMoreInfo)
            .Permit(TransitionSignal.MarkDuplicate, IssuePhase.Duplicate)
            .Permit(TransitionSignal.MarkNotReproducible, IssuePhase.NotReproducible)
            .Permit(TransitionSignal.StartFix, IssuePhase.InProgress)
            .Permit(TransitionSignal.Defer, IssuePhase.Deferred);

        engine.Configure(IssuePhase.NeedMoreInfo)
            .Permit(TransitionSignal.ReturnToAnalysis, IssuePhase.Analysis)
            .Permit(TransitionSignal.Defer, IssuePhase.Deferred);

        engine.Configure(IssuePhase.InProgress)
            .Permit(TransitionSignal.Fix, IssuePhase.Fixed)
            .Permit(TransitionSignal.ReturnToAnalysis, IssuePhase.Analysis)
            .Permit(TransitionSignal.Defer, IssuePhase.Deferred);

        engine.Configure(IssuePhase.Fixed)
            .Permit(TransitionSignal.ConfirmResolved, IssuePhase.Closed)
            .Permit(TransitionSignal.RejectFix, IssuePhase.Reopened);

        engine.Configure(IssuePhase.Closed)
            .Permit(TransitionSignal.Reopen, IssuePhase.Reopened);

        engine.Configure(IssuePhase.Reopened)
            .Permit(TransitionSignal.ReturnToAnalysis, IssuePhase.Analysis)
            .Permit(TransitionSignal.StartFix, IssuePhase.InProgress)
            .Permit(TransitionSignal.Defer, IssuePhase.Deferred);

        engine.Configure(IssuePhase.Deferred)
            .Permit(TransitionSignal.ReturnToAnalysis, IssuePhase.Analysis);

        engine.Configure(IssuePhase.Duplicate)
            .Permit(TransitionSignal.ConfirmResolved, IssuePhase.Closed);

        engine.Configure(IssuePhase.NotReproducible)
            .Permit(TransitionSignal.ConfirmResolved, IssuePhase.Closed);
    }
}

internal static class Program
{
    private static void Main()
    {
        var ticket = new Bug();

        PrintPhase(ticket, "регистрация");
        ticket.Defer();
        PrintPhase(ticket, "отложен");
        ticket.ReturnToAnalysis();
        ticket.StartAnalysis();
        PrintPhase(ticket, "разбор");
        ticket.StartFix();
        ticket.Fix();
        ticket.ConfirmResolved();
        PrintPhase(ticket, "завершён");
    }

    private static void PrintPhase(Bug ticket, string step)
    {
        Console.WriteLine($"[{step}] фаза: {ticket.State}");
    }
}
