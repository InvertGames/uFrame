using System;
using System.Collections;

/// <summary>
/// A command with an argument of type T.
/// Not usually bound to directly but used to forward a command to a parent viewmodel
/// </summary>
/// <typeparam name="T">The argument parameter.</typeparam>
public class CommandWith<T> : ICommandWith<T>
{
    public event CommandEvent OnCommandExecuted;

    public event CommandEvent OnCommandExecuting;

    public object Sender { get; set; }

    public object Parameter { get; set; }

    protected Action<T> Delegate { get; set; }

    public CommandWith(Action<T> @delegate)
    {
        Delegate = @delegate;
    }

    public CommandWith(T parameter, Action<T> @delegate)
    {
        Parameter = parameter;
        Delegate = @delegate;
    }

    //public static implicit operator CommandWith<T>(Action<T> e)
    //{
    //    return new CommandWith<T>(e);
    //}

    public virtual IEnumerator Execute()
    {
        OnOnCommandExecuting();
        if (Delegate != null)
            Delegate((T)Parameter);

        OnOnCommandComplete();
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

public class CommandWithSender<TSender> : ICommandWith<TSender>
{
    public event CommandEvent OnCommandExecuted;

    public event CommandEvent OnCommandExecuting;

    public object Sender { get; set; }
    public object Parameter { get; set; }


    protected Action<TSender> Delegate { get; set; }

    public CommandWithSender(Action<TSender> @delegate)
    {
        Delegate = @delegate;
    }

    public CommandWithSender(TSender sender, Action<TSender> @delegate)
    {
        Sender = sender;
        Delegate = @delegate;
    }

    public virtual IEnumerator Execute()
    {
        OnOnCommandExecuting();
        if (Delegate != null)
            Delegate((TSender)Sender);

        OnOnCommandComplete();
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

public class CommandWithSenderAndArgument<TSender,TArgument> : ICommandWith<TArgument>
{
    public event CommandEvent OnCommandExecuted;

    public event CommandEvent OnCommandExecuting;

    public object Sender { get; set; }
    public object Parameter { get; set; }


    protected Action<TSender,TArgument> Delegate { get; set; }

    public CommandWithSenderAndArgument(Action<TSender,TArgument> @delegate)
    {
        Delegate = @delegate;
    }

    public CommandWithSenderAndArgument(TSender sender, Action<TSender, TArgument> @delegate)
    {
        Sender = sender;
        Delegate = @delegate;
    }

    public virtual IEnumerator Execute()
    {
        OnOnCommandExecuting();
        if (Delegate != null)
            Delegate((TSender)Sender,(TArgument)Parameter);

        OnOnCommandComplete();
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