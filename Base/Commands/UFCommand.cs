using System;
using System.Collections;
using UniRx;

public abstract class UFCommand<T> : ICommandWith<T>
{
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
    public event CommandEvent OnCommandExecuted;
    public event CommandEvent OnCommandExecuting;

    public object Sender { get; set; }
    public object Parameter { get; set; }

    protected abstract void Perform(T arg);

    public void Execute()
    {
        OnOnCommandExecuting();
        Execute((T)Parameter);
        OnOnCommandComplete();
    }

    public void Execute(object arg)
    {
        Parameter = arg;
        Execute();
    }

    public virtual bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged = delegate { };
}