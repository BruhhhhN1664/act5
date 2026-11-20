namespace EquipmentBorrowing.Application;

/// <summary>Local calendar date. A fixed date can be configured for repeatable acceptance tests (Clock:FixedDate).</summary>
public sealed class LocalClock(DateOnly? fixedDate = null) : IClock
{
    public DateOnly Today => fixedDate ?? DateOnly.FromDateTime(DateTime.Now);
}

/// <summary>One-shot fault: after the first borrowing row is written inside a checkout transaction, throw once.</summary>
public sealed class ToggleFaultInjector : IFaultInjector
{
    private int _armed;
    public bool IsArmed => Volatile.Read(ref _armed) == 1;
    public void Arm(bool armed) => Interlocked.Exchange(ref _armed, armed ? 1 : 0);

    public void AfterBorrowingWritten(int writtenCount)
    {
        if (writtenCount >= 1 && Interlocked.CompareExchange(ref _armed, 0, 1) == 1)
            throw new InjectedFaultException();
    }
}
