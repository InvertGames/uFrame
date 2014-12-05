using UniRx;

public interface ICommandDispatcher : IObservable<CommandInfo>
{
    void ExecuteCommand(IuFrameCommand command, object argument, bool isChained = false);
}
public class CommandInfo
{
    public CommandInfo(IuFrameCommand command, object argument, bool isChained)
    {
        Command = command;
        Argument = argument;
        IsChained = isChained;
    }

    public IuFrameCommand Command { get; set; }
    public object Argument { get; set; }
    public bool IsChained { get; set; }
}