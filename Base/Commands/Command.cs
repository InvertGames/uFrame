using System;
using System.Collections;
using UniRx;

/// <summary>
/// A ViewModel command that can be executed.
/// IEnumerator is always used so that any command can be a coroutine.
/// </summary>
public class Command : IuFrameCommand
{
    public event CommandEvent OnCommandExecuted;

    public event CommandEvent OnCommandExecuting;
    public object Sender { get; set; }

    public object Parameter { get; set; }

    protected Action Delegate { get; set; }



    public Command(Action @delegate)
    {
        Delegate = @delegate;
    }

    public void Execute()
    {
        OnOnCommandExecuting();
        if (Delegate != null)
        {
            Delegate();
        }
        OnOnCommandComplete();
    }

    public void Execute(object parameter)
    {
        Parameter = parameter;
        Execute();
    }

    public bool CanExecute(object parameter)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnOnCommandComplete()
    {
        CommandEvent handler = OnCommandExecuted;
        if (handler != null) handler();
    }

    protected virtual void OnOnCommandExecuting()
    {
        CommandEvent handler = OnCommandExecuting;
        if (handler != null) handler();
    }

    public void OnCompleted()
    {
        
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(Unit value)
    {
        Execute();
    }

    public IDisposable Subscribe(IObserver<Unit> observer)
    {
        CommandEvent handler = () => observer.OnNext(Unit.Default);
        this.OnCommandExecuted += handler;

        return Disposable.Create(() => OnCommandExecuted -= handler);
    }

    public event EventHandler CanExecuteChanged = delegate { };
}
[Obsolete("Yield commands are no longer used.")]
public class YieldCommand : IuFrameCommand
{
    public event CommandEvent OnCommandExecuted;

    public event CommandEvent OnCommandExecuting;


    public object Sender { get; set; }

    public object Parameter { get; set; }

    protected Func<IEnumerator> EnumeratorDelegate { get; set; }

    public YieldCommand(Func<IEnumerator> enumeratorDelegate)
    {
        EnumeratorDelegate = enumeratorDelegate;
    }

    public void Execute()
    {
        OnOnCommandExecuting();
        if (EnumeratorDelegate != null)
        {
            OnOnCommandComplete();
        }
        else
        {
            OnOnCommandComplete();
        }
    }

    public void Execute(object parameter)
    {
        Parameter = parameter;
        Execute();
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    protected virtual void OnOnCommandComplete()
    {
        CommandEvent handler = OnCommandExecuted;
        if (handler != null) handler();
    }

    protected virtual void OnOnCommandExecuting()
    {
        CommandEvent handler = OnCommandExecuting;
        if (handler != null) handler();
    }

    public void OnCompleted()
    {
        
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(Unit value)
    {
        Execute();
    }

    public IDisposable Subscribe(IObserver<Unit> observer)
    {
        CommandEvent handler = () => observer.OnNext(Unit.Default);
        this.OnCommandExecuted += handler;

        return Disposable.Create(() => OnCommandExecuted -= handler);
    }

    public event EventHandler CanExecuteChanged = delegate { };
}