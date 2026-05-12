using Microsoft.Extensions.Localization;

namespace Popocatepetl.CLI.Tests.TestUtilities;

internal sealed class FakeStringLocalizer<T> : IStringLocalizer<T> where T : class
{
    public LocalizedString this[string name] => new(name, name, resourceNotFound: false);

    public LocalizedString this[string name, params object[] arguments] =>
        new(name, string.Format(name, arguments), resourceNotFound: false);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        Enumerable.Empty<LocalizedString>();
}
