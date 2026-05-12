using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Popocatepetl.CLI.Prompts.Native;

public sealed class NativeFileSaveDialog : IFileSaveDialog
{
    private static readonly Lazy<bool> _isAvailable = new(() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
        (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && HasZenity()));

    public bool IsAvailable => _isAvailable.Value;

    public async Task<string?> ShowAsync(string title, string defaultName, CancellationToken ct = default)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return await RunMacOsAsync(title, defaultName, ct);
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return await RunWindowsAsync(title, defaultName, ct);
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && HasZenity())
        {
            return await RunLinuxAsync(title, defaultName, ct);
        }
        return null;
    }

    private static async Task<string?> RunMacOsAsync(string title, string defaultName, CancellationToken ct)
    {
        var script =
            $"POSIX path of (choose file name with prompt \"{EscapeAppleScript(title)}\" default name \"{EscapeAppleScript(defaultName)}\")";

        var psi = new ProcessStartInfo("osascript")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-e");
        psi.ArgumentList.Add(script);

        return await CaptureSingleLineAsync(psi, ct);
    }

    private static async Task<string?> RunWindowsAsync(string title, string defaultName, CancellationToken ct)
    {
        var script = $@"
Add-Type -AssemblyName System.Windows.Forms
$d = New-Object System.Windows.Forms.SaveFileDialog
$d.Title = '{EscapeSingleQuote(title)}'
$d.FileName = '{EscapeSingleQuote(defaultName)}'
$d.OverwritePrompt = $true
if ($d.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {{ Write-Output $d.FileName }}
else {{ exit 1 }}";

        var psi = new ProcessStartInfo("powershell")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-NoProfile");
        psi.ArgumentList.Add("-NonInteractive");
        psi.ArgumentList.Add("-Command");
        psi.ArgumentList.Add(script);

        return await CaptureSingleLineAsync(psi, ct);
    }

    private static async Task<string?> RunLinuxAsync(string title, string defaultName, CancellationToken ct)
    {
        var psi = new ProcessStartInfo("zenity")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("--file-selection");
        psi.ArgumentList.Add("--save");
        psi.ArgumentList.Add("--confirm-overwrite");
        psi.ArgumentList.Add($"--title={title}");
        psi.ArgumentList.Add($"--filename={defaultName}");

        return await CaptureSingleLineAsync(psi, ct);
    }

    private static async Task<string?> CaptureSingleLineAsync(ProcessStartInfo psi, CancellationToken ct)
    {
        try
        {
            using var process = Process.Start(psi);
            if (process is null)
            {
                return null;
            }
            var stdout = await process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);
            if (process.ExitCode != 0)
            {
                return null;
            }
            var trimmed = stdout.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }
        catch
        {
            return null;
        }
    }

    private static bool HasZenity()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("zenity")
            {
                ArgumentList = { "--version" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            });
            process?.WaitForExit(500);
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static string EscapeAppleScript(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string EscapeSingleQuote(string s) =>
        s.Replace("'", "''");
}
