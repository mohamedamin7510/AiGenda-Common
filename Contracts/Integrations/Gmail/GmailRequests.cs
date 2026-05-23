namespace AI_genda_API.Contracts.Integrations.Gmail;

public record GmailSendRequest(string To, string Subject, string BodyText);
public record GmailReplyRequest(string To, string Subject, string BodyText, string OriginalMessageId, string ThreadId);
public record GmailDraftRequest(string To, string Subject, string BodyText);
