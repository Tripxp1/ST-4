using BugPro;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BugTests;

[TestClass]
public class BugTests
{
    private Bug? _bug;

    [TestInitialize]
    public void SetUp()
    {
        _bug = new Bug();
    }

    [TestMethod]
    public void InitialState_ShouldBeNewDefect()
    {
        Assert.AreEqual(Bug.State.NewDefect, _bug!.CurrentState);
    }

    [TestMethod]
    public void StartTriage_FromNewDefect_TransitionsToTriage()
    {
        _bug!.StartTriage();
        Assert.AreEqual(Bug.State.Triage, _bug.CurrentState);
    }

    [TestMethod]
    public void NoTime_FromTriage_TransitionsToFix()
    {
        _bug!.StartTriage();
        _bug.NoTime();
        Assert.AreEqual(Bug.State.Fix, _bug.CurrentState);
    }

    [TestMethod]
    public void NeedSeparateSolution_FromTriage_TransitionsToFix()
    {
        _bug!.StartTriage();
        _bug.NeedSeparateSolution();
        Assert.AreEqual(Bug.State.Fix, _bug.CurrentState);
    }

    [TestMethod]
    public void OtherProduct_FromTriage_TransitionsToClosed()
    {
        _bug!.StartTriage();
        _bug.OtherProduct();
        Assert.AreEqual(Bug.State.Closed, _bug.CurrentState);
    }

    [TestMethod]
    public void NeedMoreInfo_FromTriage_StaysInTriage()
    {
        _bug!.StartTriage();
        _bug.NeedMoreInfo();
        Assert.AreEqual(Bug.State.Triage, _bug.CurrentState);
    }

    [TestMethod]
    public void FixDone_FromFix_TransitionsToClosed()
    {
        _bug!.StartTriage();
        _bug.NeedSeparateSolution();
        _bug.FixDone();
        Assert.AreEqual(Bug.State.Closed, _bug.CurrentState);
    }

    [TestMethod]
    public void NotDefect_FromFix_TransitionsToClosed()
    {
        _bug!.StartTriage();
        _bug.NeedSeparateSolution();
        _bug.NotDefect();
        Assert.AreEqual(Bug.State.Closed, _bug.CurrentState);
    }

    [TestMethod]
    public void DoNotFix_FromFix_TransitionsToClosed()
    {
        _bug!.StartTriage();
        _bug.NeedSeparateSolution();
        _bug.DoNotFix();
        Assert.AreEqual(Bug.State.Closed, _bug.CurrentState);
    }

    [TestMethod]
    public void Duplicate_FromFix_TransitionsToClosed()
    {
        _bug!.StartTriage();
        _bug.NeedSeparateSolution();
        _bug.Duplicate();
        Assert.AreEqual(Bug.State.Closed, _bug.CurrentState);
    }

    [TestMethod]
    public void NotReproducible_FromFix_TransitionsToClosed()
    {
        _bug!.StartTriage();
        _bug.NeedSeparateSolution();
        _bug.NotReproducible();
        Assert.AreEqual(Bug.State.Closed, _bug.CurrentState);
    }

    [TestMethod]
    public void Reopen_FromClosed_TransitionsToReopened()
    {
        _bug!.StartTriage();
        _bug.OtherProduct();
        _bug.Reopen();
        Assert.AreEqual(Bug.State.Reopened, _bug.CurrentState);
    }

    [TestMethod]
    public void Reopen_FromFix_TransitionsToReopened()
    {
        _bug!.StartTriage();
        _bug.NoTime();
        _bug.Reopen();
        Assert.AreEqual(Bug.State.Reopened, _bug.CurrentState);
    }

    [TestMethod]
    public void StartTriage_FromReopened_TransitionsToTriage()
    {
        _bug!.StartTriage();
        _bug.OtherProduct();
        _bug.Reopen();
        _bug.StartTriage();
        Assert.AreEqual(Bug.State.Triage, _bug.CurrentState);
    }

    [TestMethod]
    public void FixDone_FromReopened_TransitionsToClosed()
    {
        _bug!.StartTriage();
        _bug.NoTime();
        _bug.Reopen();
        _bug.FixDone();
        Assert.AreEqual(Bug.State.Closed, _bug.CurrentState);
    }

    [TestMethod]
    public void NoTime_FromNewDefect_ThrowsException()
    {
        Assert.ThrowsException<InvalidOperationException>(() => _bug!.NoTime());
    }

    [TestMethod]
    public void FixDone_FromTriage_ThrowsException()
    {
        _bug!.StartTriage();
        Assert.ThrowsException<InvalidOperationException>(() => _bug.FixDone());
    }

    [TestMethod]
    public void NotDefect_FromNewDefect_ThrowsException()
    {
        Assert.ThrowsException<InvalidOperationException>(() => _bug!.NotDefect());
    }

    [TestMethod]
    public void Reopen_FromNewDefect_ThrowsException()
    {
        Assert.ThrowsException<InvalidOperationException>(() => _bug!.Reopen());
    }

    [TestMethod]
    public void Reopen_WhenAlreadyReopened_DoesNotThrow()
    {
        _bug!.StartTriage();
        _bug.OtherProduct();
        _bug.Reopen();
        _bug.Reopen();
        Assert.AreEqual(Bug.State.Reopened, _bug.CurrentState);
    }

    [TestMethod]
    public void StartTriage_WhenAlreadyInTriage_DoesNotThrow()
    {
        _bug!.StartTriage();
        _bug.StartTriage();
        Assert.AreEqual(Bug.State.Triage, _bug.CurrentState);
    }
}