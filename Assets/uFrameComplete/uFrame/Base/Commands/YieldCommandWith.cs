using System;
using System.Collections;

/// <summary>
/// A coroutine command with a parameter.
/// </summary>
/// <typeparam name="T"></typeparam>
public class YieldCommandWith<T> : ICommandWith<T>
{
    public event CommandEvent OnCommandExecuted;

    public event CommandEvent OnCommandExecuting;
    public object Sender { get; set; }

    public object Parameter { get; set; }

    protected Func<T, IEnumerator> EnumeratorDelegate
    {
        get;
        set;
    }

    public YieldCommandWith(Func<T, IEnumerator> enumeratorDelegate)
    {
        EnumeratorDelegate = enumeratorDelegate;
    }

    public YieldCommandWith(T sender, Func<T, IEnumerator> enumeratorDelegate)
    {
        Parameter = sender;
        EnumeratorDelegate = enumeratorDelegate;
    }

    public IEnumerator Execute()
    {
        OnOnCommandExecuting();
        if (EnumeratorDelegate != null)
        {
            var result = EnumeratorDelegate((T)Parameter);
            OnOnCommandComplete();
            return result;
        }
        else
        {
            OnOnCommandComplete();
        }

        return null;
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
}
/// <summary>
/// A coroutine command with a parameter.
/// </summary>
/// <typeparam name="T"></typeparam>
public class YieldCommandWithSender<T> : ICommandWith<T>
{
    public event CommandEvent OnCommandExecuted;

    public event CommandEvent OnCommandExecuting;
    
    public object Sender { get; set; }

    public object Parameter { get; set; }

    protected Func<T, IEnumerator> EnumeratorDelegate
    {
        get;
        set;
    }

    public YieldCommandWithSender(Func<T, IEnumerator> enumeratorDelegate)
    {
        EnumeratorDelegate = enumeratorDelegate;
    }

    public YieldCommandWithSender(T sender, Func<T, IEnumerator> enumeratorDelegate)
    {
        Sender = sender;
        EnumeratorDelegate = enumeratorDelegate;
    }

    public IEnumerator Execute()
    {
        OnOnCommandExecuting();
        if (EnumeratorDelegate != null)
        {
            var result = EnumeratorDelegate((T)Sender);
            OnOnCommandComplete();
            return result;
        }
        else
        {
            OnOnCommandComplete();
        }

        return null;
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
}

/// <summary>
/// A coroutine command with a parameter.
/// </summary>
/// <typeparam name="TSender"></typeparam>
/// <typeparam name="TArgument"></typeparam>
public class YieldCommandWithSenderAndArgument<TSender, TArgument> : ICommandWith<TArgument>
{
    public event CommandEvent OnCommandExecuted;

    public event CommandEvent OnCommandExecuting;
    public object Sender { get; set; }

    public object Parameter { get; set; }

    protected Func<TSender, TArgument, IEnumerator> EnumeratorDelegate
    {
        get;
        set;
    }

    public YieldCommandWithSenderAndArgument(Func<TSender,TArgument, IEnumerator> enumeratorDelegate)
    {
        EnumeratorDelegate = enumeratorDelegate;
    }

    public YieldCommandWithSenderAndArgument(TSender sender, Func<TSender, TArgument, IEnumerator> enumeratorDelegate)
    {
        Sender = sender;
        EnumeratorDelegate = enumeratorDelegate;
    }

    public IEnumerator Execute()
    {
        OnOnCommandExecuting();
        if (EnumeratorDelegate != null)
        {
            var result = EnumeratorDelegate((TSender)Sender,(TArgument)Parameter);
            OnOnCommandComplete();
            return result;
        }
        else
        {
            OnOnCommandComplete();
        }

        return null;
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
}