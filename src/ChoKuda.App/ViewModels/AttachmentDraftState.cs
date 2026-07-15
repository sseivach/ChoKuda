using System.IO;
using ChoKuda.Core.Attachments;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.ViewModels;

public sealed class AttachmentDraftState
{
    public const double MinimumPhotoViewerZoom = 0.5;
    public const double MaximumPhotoViewerZoom = 3;

    private const double PhotoViewerZoomStep = 0.25;
    private readonly List<PendingAttachment> _pendingAttachments = [];

    public IReadOnlyList<PendingAttachment> PendingAttachments =>
        _pendingAttachments;

    public IReadOnlyList<string> Errors { get; private set; } = Array.Empty<string>();

    public string? PhotoViewerPath { get; private set; }

    public double PhotoViewerZoom { get; private set; } = 1;

    public bool HasPending =>
        _pendingAttachments.Count > 0;

    public void AddFiles(
        IEnumerable<string> filePaths,
        Func<string, AttachmentKind> classify)
    {
        foreach (var filePath in filePaths)
        {
            if (_pendingAttachments.Any(attachment => string.Equals(attachment.SourcePath, filePath, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            _pendingAttachments.Add(new PendingAttachment(filePath, classify(filePath)));
        }
    }

    public void ClearPending()
    {
        _pendingAttachments.Clear();
    }

    public void ClearAll()
    {
        ClearPending();
        ClearErrors();
        ClosePhotoViewer();
    }

    public void ClearErrors()
    {
        Errors = Array.Empty<string>();
    }

    public void SetErrors(IEnumerable<string> errors)
    {
        Errors = errors.ToArray();
    }

    public void SetImportErrors(IEnumerable<AttachmentImportError> errors)
    {
        Errors = errors
            .Select(error => $"{Path.GetFileName(error.SourcePath)}: {error.Message}")
            .ToArray();
    }

    public void AddError(string error)
    {
        Errors = Errors.Concat([error]).ToArray();
    }

    public bool RemovePending(AttachmentDisplayItem item)
    {
        if (!item.IsPending)
        {
            return false;
        }

        var removedCount = _pendingAttachments.RemoveAll(attachment =>
            attachment.SourcePath == item.Path &&
            attachment.Kind == item.Kind);

        return removedCount > 0;
    }

    public void OpenFile(
        AttachmentDisplayItem item,
        Func<string, string?> open)
    {
        ClearErrors();
        var error = open(item.Path);

        if (!string.IsNullOrWhiteSpace(error))
        {
            SetErrors([error]);
        }
    }

    public IReadOnlyList<AttachmentDisplayItem> GetDisplayItems(
        AttachmentKind kind,
        PointDocument? point,
        string? savedDirectory)
    {
        if (point is null)
        {
            return Array.Empty<AttachmentDisplayItem>();
        }

        var savedNames = kind == AttachmentKind.Photo
            ? point.Photos
            : point.Files;
        var savedItems = savedNames.Select(name => new AttachmentDisplayItem(
            Kind: kind,
            DisplayName: RestoreOriginalFileName(name),
            Path: savedDirectory is null ? name : Path.Combine(savedDirectory, name),
            StoredName: name,
            IsPending: false));
        var pendingItems = _pendingAttachments
            .Where(attachment => attachment.Kind == kind)
            .Select(attachment => new AttachmentDisplayItem(
                Kind: kind,
                DisplayName: Path.GetFileName(attachment.SourcePath),
                Path: attachment.SourcePath,
                StoredName: string.Empty,
                IsPending: true));

        return savedItems.Concat(pendingItems).ToArray();
    }

    public void OpenPhotoViewer(string photoPath)
    {
        PhotoViewerPath = photoPath;
        ResetPhotoViewerZoom();
    }

    public void ClosePhotoViewer()
    {
        PhotoViewerPath = null;
        ResetPhotoViewerZoom();
    }

    public void ZoomPhotoViewerIn()
    {
        SetPhotoViewerZoom(PhotoViewerZoom + PhotoViewerZoomStep);
    }

    public void ZoomPhotoViewerOut()
    {
        SetPhotoViewerZoom(PhotoViewerZoom - PhotoViewerZoomStep);
    }

    public void ResetPhotoViewerZoom()
    {
        SetPhotoViewerZoom(1);
    }

    private void SetPhotoViewerZoom(double zoom)
    {
        PhotoViewerZoom = Math.Round(
            Math.Clamp(zoom, MinimumPhotoViewerZoom, MaximumPhotoViewerZoom),
            2);
    }

    public static string RestoreOriginalFileName(string storedName)
    {
        var extension = Path.GetExtension(storedName);
        var baseName = Path.GetFileNameWithoutExtension(storedName);
        var markerIndex = baseName.LastIndexOf("__", StringComparison.Ordinal);

        return markerIndex < 0
            ? storedName
            : $"{baseName[..markerIndex]}{extension}";
    }

    public static string ToImageSource(string filePath) =>
        new Uri(filePath).AbsoluteUri;
}
