using ChoKuda.Core.Attachments;

namespace ChoKuda.App.ViewModels;

public sealed record AttachmentDisplayItem(
    AttachmentKind Kind,
    string DisplayName,
    string Path,
    string StoredName,
    bool IsPending);
