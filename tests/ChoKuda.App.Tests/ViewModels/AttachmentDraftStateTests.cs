using ChoKuda.App.ViewModels;
using ChoKuda.Core.Attachments;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.Tests.ViewModels;

public sealed class AttachmentDraftStateTests
{
    [Fact]
    public void AddFilesClassifiesFilesAndSkipsDuplicateSourcePaths()
    {
        var attachments = new AttachmentDraftState();
        var photoPath = Path.Combine("c:", "temp", "photo.jpg");
        var duplicatePhotoPath = Path.Combine("C:", "TEMP", "photo.jpg");
        var filePath = Path.Combine("c:", "temp", "guide.pdf");

        attachments.AddFiles(
            [photoPath, duplicatePhotoPath, filePath],
            path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                ? AttachmentKind.Photo
                : AttachmentKind.File);

        Assert.Equal(2, attachments.PendingAttachments.Count);
        Assert.Contains(attachments.PendingAttachments, attachment => attachment.SourcePath == photoPath && attachment.Kind == AttachmentKind.Photo);
        Assert.Contains(attachments.PendingAttachments, attachment => attachment.SourcePath == filePath && attachment.Kind == AttachmentKind.File);
    }

    [Fact]
    public void GetDisplayItemsCombinesSavedAndPendingItems()
    {
        var attachments = new AttachmentDraftState();
        var point = new PointDocument
        {
            Photos = ["waterfall__11111111111111111111111111111111.jpg"],
            Files = ["guide__22222222222222222222222222222222.pdf"],
        };
        var pendingPhotoPath = Path.Combine("c:", "temp", "draft.png");
        attachments.AddFiles([pendingPhotoPath], _ => AttachmentKind.Photo);

        var photoItems = attachments.GetDisplayItems(AttachmentKind.Photo, point, Path.Combine("library", "photos"));
        var fileItems = attachments.GetDisplayItems(AttachmentKind.File, point, Path.Combine("library", "files"));

        Assert.Equal(["waterfall.jpg", "draft.png"], photoItems.Select(item => item.DisplayName));
        Assert.Equal([false, true], photoItems.Select(item => item.IsPending));
        Assert.Equal(["guide.pdf"], fileItems.Select(item => item.DisplayName));
        Assert.Equal(Path.Combine("library", "photos", "waterfall__11111111111111111111111111111111.jpg"), photoItems[0].Path);
    }

    [Fact]
    public void RemovePendingRemovesOnlyMatchingPendingItem()
    {
        var attachments = new AttachmentDraftState();
        var photoPath = Path.Combine("c:", "temp", "draft.png");
        attachments.AddFiles([photoPath], _ => AttachmentKind.Photo);
        var item = attachments.GetDisplayItems(AttachmentKind.Photo, new PointDocument(), null).Single();

        var removed = attachments.RemovePending(item);

        Assert.True(removed);
        Assert.Empty(attachments.PendingAttachments);
    }

    [Fact]
    public void OpenFileClearsExistingErrorsWhenOpenSucceeds()
    {
        var attachments = new AttachmentDraftState();
        attachments.SetErrors(["Old error."]);
        var item = new AttachmentDisplayItem(
            AttachmentKind.File,
            "guide.pdf",
            "library/files/guide.pdf",
            "guide.pdf",
            IsPending: false);

        attachments.OpenFile(item, path =>
        {
            Assert.Equal("library/files/guide.pdf", path);
            return null;
        });

        Assert.Empty(attachments.Errors);
    }

    [Fact]
    public void OpenFileStoresErrorWhenOpenFails()
    {
        var attachments = new AttachmentDraftState();
        var item = new AttachmentDisplayItem(
            AttachmentKind.File,
            "missing.pdf",
            "library/files/missing.pdf",
            "missing.pdf",
            IsPending: false);

        attachments.OpenFile(item, _ => "File was not found.");

        Assert.Equal(["File was not found."], attachments.Errors);
    }

    [Fact]
    public void ErrorAndViewerStateCanBeClearedTogether()
    {
        var attachments = new AttachmentDraftState();
        attachments.SetErrors(["Could not open file."]);
        attachments.OpenPhotoViewer(Path.Combine("c:", "temp", "photo.jpg"));
        attachments.ZoomPhotoViewerIn();

        attachments.ClearAll();

        Assert.Empty(attachments.Errors);
        Assert.Null(attachments.PhotoViewerPath);
        Assert.Equal(1, attachments.PhotoViewerZoom);
        Assert.Empty(attachments.PendingAttachments);
    }

    [Fact]
    public void PhotoViewerZoomIsClampedAndResetWithViewerState()
    {
        var attachments = new AttachmentDraftState();
        attachments.OpenPhotoViewer(Path.Combine("c:", "temp", "photo.jpg"));

        for (var index = 0; index < 20; index++)
        {
            attachments.ZoomPhotoViewerIn();
        }

        Assert.Equal(AttachmentDraftState.MaximumPhotoViewerZoom, attachments.PhotoViewerZoom);

        for (var index = 0; index < 20; index++)
        {
            attachments.ZoomPhotoViewerOut();
        }

        Assert.Equal(AttachmentDraftState.MinimumPhotoViewerZoom, attachments.PhotoViewerZoom);

        attachments.ResetPhotoViewerZoom();

        Assert.Equal(1, attachments.PhotoViewerZoom);

        attachments.ZoomPhotoViewerIn();
        attachments.OpenPhotoViewer(Path.Combine("c:", "temp", "other.jpg"));

        Assert.Equal(1, attachments.PhotoViewerZoom);

        attachments.ZoomPhotoViewerIn();
        attachments.ClosePhotoViewer();

        Assert.Null(attachments.PhotoViewerPath);
        Assert.Equal(1, attachments.PhotoViewerZoom);
    }

    [Fact]
    public void RestoreOriginalFileNameRemovesGuidMarkerWhenPresent()
    {
        Assert.Equal(
            "my photo.jpg",
            AttachmentDraftState.RestoreOriginalFileName("my photo__550e8400e29b41d4a716446655440000.jpg"));
        Assert.Equal("plain.pdf", AttachmentDraftState.RestoreOriginalFileName("plain.pdf"));
    }
}
