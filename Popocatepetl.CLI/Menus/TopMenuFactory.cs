using Popocatepetl.CLI.Auth;
using Popocatepetl.CLI.Menus.Admin;
using Popocatepetl.CLI.Menus.PowerUser;
using Popocatepetl.CLI.Menus.Settings;
using Popocatepetl.CLI.Menus.User;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Services;
using Popocatepetl.Domain.Enums;

namespace Popocatepetl.CLI.Menus;

public sealed class TopMenuFactory
{
    private readonly ViewLatestDiffAction _viewLatestDiff;
    private readonly ChangeThemeAction _changeTheme;
    private readonly SendReportAction _sendReport;
    private readonly DownloadReportAction _downloadReport;
    private readonly ExportDiffAction _exportDiff;
    private readonly ViewAuditLogsAction _viewAuditLogs;
    private readonly LanguageMenuAction _languageMenu;
    private readonly AdminPasswordGate _adminGate;
    private readonly CurrentUserContext _session;

    public TopMenuFactory(
        ViewLatestDiffAction viewLatestDiff,
        ChangeThemeAction changeTheme,
        SendReportAction sendReport,
        DownloadReportAction downloadReport,
        ExportDiffAction exportDiff,
        ViewAuditLogsAction viewAuditLogs,
        LanguageMenuAction languageMenu,
        AdminPasswordGate adminGate,
        CurrentUserContext session)
    {
        _viewLatestDiff = viewLatestDiff;
        _changeTheme = changeTheme;
        _sendReport = sendReport;
        _downloadReport = downloadReport;
        _exportDiff = exportDiff;
        _viewAuditLogs = viewAuditLogs;
        _languageMenu = languageMenu;
        _adminGate = adminGate;
        _session = session;
    }

    public MenuNode Build()
    {
        var userBranch = MenuNode.GuardedBranch(
            "menu.user.title",
            UserRole.None,
            SetRoleEntry(UserRole.User),
            MenuNode.Leaf(_viewLatestDiff.LabelKey, UserRole.User, _viewLatestDiff));

        var powerBranch = MenuNode.GuardedBranch(
            "menu.poweruser.title",
            UserRole.None,
            SetRoleEntry(UserRole.PowerUser),
            MenuNode.Leaf(_changeTheme.LabelKey, UserRole.PowerUser, _changeTheme),
            MenuNode.Leaf(_sendReport.LabelKey, UserRole.PowerUser, _sendReport));

        var adminBranch = MenuNode.GuardedBranch(
            "menu.admin.title",
            UserRole.None,
            AdminEntry(),
            MenuNode.Leaf(_downloadReport.LabelKey, UserRole.Admin, _downloadReport),
            MenuNode.Leaf(_exportDiff.LabelKey, UserRole.Admin, _exportDiff),
            MenuNode.Leaf(_viewAuditLogs.LabelKey, UserRole.Admin, _viewAuditLogs));

        return MenuNode.Branch(
            "menu.top.title",
            UserRole.None,
            userBranch,
            powerBranch,
            adminBranch,
            MenuNode.Leaf(_languageMenu.LabelKey, UserRole.None, _languageMenu));
    }

    private Func<CancellationToken, Task<bool>> SetRoleEntry(UserRole role) => _ =>
    {
        _session.Role = role;
        return Task.FromResult(true);
    };

    private Func<CancellationToken, Task<bool>> AdminEntry() => async ct =>
    {
        if (!await _adminGate.TryUnlockAsync(ct))
        {
            return false;
        }
        _session.Role = UserRole.Admin;
        return true;
    };
}
