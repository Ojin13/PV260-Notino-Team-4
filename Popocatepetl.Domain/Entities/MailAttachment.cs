namespace Popocatepetl.Domain.Entities;

/// <summary>An outbound email attachment with raw byte content.</summary>
public sealed record MailAttachment(string FileName, byte[] Content, string ContentType);