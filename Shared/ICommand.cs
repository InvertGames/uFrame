using System;
using System.Collections;
#if DLL
namespace Invert.MVVM
{
#endif
using System.Collections.Generic;
using UniRx;

public delegate void CommandEvent();

/// <summary>
/// The base command interface for implementing a command in a ViewModel
/// </summary>
public interface IuFrameCommand : ISubject<Unit>, System.Windows.Input.ICommand
{
   
   
    //event EventHandler CanExecuteChanged;
    event CommandEvent OnCommandExecuted;
    event CommandEvent OnCommandExecuting; 

    //object Sender { get; set; }
    //object Parameter { get; set; }
    
    //void Execute();
}

public interface IParameterCommand : IuFrameCommand
{
    object Parameter { get; set; }
}
/// <summary>
/// A base command interface for implementing a command with a parameter in a ViewModel
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICommandWith<T> : IParameterCommand
{
    //IEnumerator Execute(T parameter);
}
#if DLL
}
#endif

public class SimpleSubject<T> : ISubject<T>
{
    private List<IObserver<T>> _observers;

    public List<IObserver<T>> Observers
    {
        get { return _observers ?? (_observers = new List<IObserver<T>>()); }
        set { _observers = value; }
    }

    public void OnCompleted()
    {
        foreach (var observer in Observers.ToArray())
        {
            if (observer == null) continue;
            observer.OnCompleted();
        }
        Observers.Clear();
    }

    public void OnError(Exception error)
    {
        foreach (var observer in Observers.ToArray())
        {
            if (observer == null) continue;
            observer.OnError(error);
        }
    }

    public void OnNext(T value)
    {
        foreach (var observer in Observers)
        {
            if (observer == null) continue;
            observer.OnNext(value);
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        Observers.Add(observer);
        return Disposable.Create(() => Observers.Remove(observer));
    }
}

