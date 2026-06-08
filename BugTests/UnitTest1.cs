using BugPro;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BugTests;

[TestClass]
public sealed class IssueWorkflowTests
{
    private Bug _ticket = null!;

    [TestInitialize]
    public void Init() => _ticket = new Bug();

    [TestCleanup]
    public void Cleanup() => _ticket = null!;

    [TestMethod]
    public void FreshTicket_StartsInNewPhase()
    {
        ExpectPhase(IssuePhase.New);
    }

    [TestMethod]
    public void NewTicket_OnlyStartAnalysisAllowed()
    {
        Assert.IsTrue(_ticket.CanStartAnalysis());
        Assert.IsFalse(_ticket.CanFix());
        Assert.IsFalse(_ticket.CanClose());
        Assert.IsFalse(_ticket.CanReopen());
        Assert.IsFalse(_ticket.CanReturnToAnalysis());
    }

    [TestMethod]
    public void BeginReview_MovesToAnalysisPhase()
    {
        _ticket.StartAnalysis();
        ExpectPhase(IssuePhase.Analysis);
    }

    [TestMethod]
    public void RequestDetails_FromAnalysis_GoesToNeedMoreInfo()
    {
        GoToAnalysis();
        _ticket.RequestInfo();
        ExpectPhase(IssuePhase.NeedMoreInfo);
    }

    [TestMethod]
    public void ReturnFromDetails_RestoresAnalysis()
    {
        GoToNeedMoreInfo();
        _ticket.ReturnToAnalysis();
        ExpectPhase(IssuePhase.Analysis);
    }

    [TestMethod]
    public void LaunchFix_FromAnalysis_EntersInProgress()
    {
        GoToAnalysis();
        _ticket.StartFix();
        ExpectPhase(IssuePhase.InProgress);
    }

    [TestMethod]
    public void InProgress_AllowsFixAction()
    {
        GoToInProgress();
        Assert.IsTrue(_ticket.CanFix());
        Assert.IsFalse(_ticket.CanStartAnalysis());
    }

    [TestMethod]
    public void ApplyFix_ReachesResolvedPhase()
    {
        GoToInProgress();
        _ticket.Fix();
        ExpectPhase(IssuePhase.Fixed);
    }

    [TestMethod]
    public void CloseTicket_CompletesHappyPath()
    {
        GoToInProgress();
        _ticket.Fix();
        _ticket.ConfirmResolved();
        ExpectPhase(IssuePhase.Closed);
    }

    [TestMethod]
    public void Reopen_FromClosed_SetsReopenedPhase()
    {
        GoToClosed();
        _ticket.Reopen();
        ExpectPhase(IssuePhase.Reopened);
    }

    [TestMethod]
    public void RejectResolution_FromFixed_OpensReopened()
    {
        GoToFixed();
        _ticket.RejectFix();
        ExpectPhase(IssuePhase.Reopened);
    }

    [TestMethod]
    public void Reopened_ResumesFixWork()
    {
        GoToReopened();
        _ticket.StartFix();
        ExpectPhase(IssuePhase.InProgress);
    }

    [TestMethod]
    public void MarkAsDuplicate_AndClose()
    {
        GoToAnalysis();
        _ticket.MarkDuplicate();
        _ticket.ConfirmResolved();
        ExpectPhase(IssuePhase.Closed);
    }

    [TestMethod]
    public void MarkAsNonReproducible_AndClose()
    {
        GoToAnalysis();
        _ticket.MarkNotReproducible();
        _ticket.ConfirmResolved();
        ExpectPhase(IssuePhase.Closed);
    }

    [TestMethod]
    public void Postpone_FromNew_GoesDeferred()
    {
        _ticket.Defer();
        ExpectPhase(IssuePhase.Deferred);
    }

    [TestMethod]
    public void Deferred_ReturnsToAnalysis()
    {
        _ticket.Defer();
        _ticket.ReturnToAnalysis();
        ExpectPhase(IssuePhase.Analysis);
    }

    [TestMethod]
    public void Fix_FromNew_Throws()
    {
        AssertBlocked(() => _ticket.Fix());
    }

    [TestMethod]
    public void Close_FromAnalysis_Throws()
    {
        GoToAnalysis();
        AssertBlocked(() => _ticket.ConfirmResolved());
    }

    [TestMethod]
    public void Reopen_FromFixed_Throws()
    {
        GoToFixed();
        AssertBlocked(() => _ticket.Reopen());
    }

    [TestMethod]
    public void StartAnalysis_FromClosed_Throws()
    {
        GoToClosed();
        AssertBlocked(() => _ticket.StartAnalysis());
    }

    [TestMethod]
    public void ReturnToAnalysis_FromNew_Throws()
    {
        AssertBlocked(() => _ticket.ReturnToAnalysis());
    }

    [TestMethod]
    public void Defer_FromClosed_Throws()
    {
        GoToClosed();
        AssertBlocked(() => _ticket.Defer());
    }

    [TestMethod]
    public void MarkDuplicate_FromNew_Throws()
    {
        AssertBlocked(() => _ticket.MarkDuplicate());
    }

    [TestMethod]
    public void RequestInfo_FromInProgress_Throws()
    {
        GoToInProgress();
        AssertBlocked(() => _ticket.RequestInfo());
    }

    private void ExpectPhase(IssuePhase expected)
    {
        Assert.AreEqual(expected, _ticket.State);
    }

    private static void AssertBlocked(Action action)
    {
        Assert.ThrowsException<InvalidOperationException>(action);
    }

    private void GoToAnalysis() => _ticket.StartAnalysis();

    private void GoToNeedMoreInfo()
    {
        GoToAnalysis();
        _ticket.RequestInfo();
    }

    private void GoToInProgress()
    {
        GoToAnalysis();
        _ticket.StartFix();
    }

    private void GoToFixed()
    {
        GoToInProgress();
        _ticket.Fix();
    }

    private void GoToClosed()
    {
        GoToFixed();
        _ticket.ConfirmResolved();
    }

    private void GoToReopened()
    {
        GoToFixed();
        _ticket.RejectFix();
    }
}
