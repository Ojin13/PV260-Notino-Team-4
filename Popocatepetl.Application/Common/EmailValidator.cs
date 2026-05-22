using System.Net.Mail;

namespace Popocatepetl.Application.Common;

public static class EmailValidator
{
    public static bool IsValid(string? input) =>
        !string.IsNullOrWhiteSpace(input) && MailAddress.TryCreate(input, out _);
}
