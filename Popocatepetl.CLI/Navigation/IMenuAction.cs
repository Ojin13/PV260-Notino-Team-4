namespace Popocatepetl.CLI.Navigation;

public interface IMenuAction
{
    string LabelKey { get; }

    Task ExecuteAsync(CancellationToken ct);
}