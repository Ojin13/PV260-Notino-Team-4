using Popocatepetl.Domain.Enums;

namespace Popocatepetl.CLI.Navigation;

public sealed record MenuNode(
    string LabelKey,
    UserRole RequiredRole,
    IMenuAction? Action,
    Func<CancellationToken, Task<bool>>? EntryGuard,
    IReadOnlyList<MenuNode> Children)
{
    public bool IsLeaf => Action is not null;

    public static MenuNode Branch(string labelKey, UserRole requiredRole, params MenuNode[] children)
        => new(labelKey, requiredRole, Action: null, EntryGuard: null, children);

    public static MenuNode GuardedBranch(
        string labelKey,
        UserRole requiredRole,
        Func<CancellationToken, Task<bool>> guard,
        params MenuNode[] children)
        => new(labelKey, requiredRole, Action: null, guard, children);

    public static MenuNode Leaf(string labelKey, UserRole requiredRole, IMenuAction action)
        => new(labelKey, requiredRole, action, EntryGuard: null, Array.Empty<MenuNode>());
}
