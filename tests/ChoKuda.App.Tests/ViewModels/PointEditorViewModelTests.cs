using ChoKuda.App.ViewModels;
using ChoKuda.Core.Attachments;
using ChoKuda.Core.Domain;
using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Map;
using ChoKuda.Core.Points;

namespace ChoKuda.App.Tests.ViewModels;

public sealed class PointEditorViewModelTests
{
    [Fact]
    public void SelectCoordinateStoresCoordinateOnlyWhenRightPanelIsClosed()
    {
        var editor = new PointEditorViewModel();
        var coordinate = new MapCoordinate { Latitude = 34.123456, Longitude = -112.654321 };

        editor.SelectCoordinate(coordinate, isRightPanelOpen: true);

        Assert.Null(editor.SelectedCoordinate);
        Assert.Equal("No map point selected", editor.SelectedCoordinateStatus);
        Assert.False(editor.CanOpenNewPoint(isLibraryReady: true, isRightPanelOpen: false));

        editor.SelectCoordinate(coordinate, isRightPanelOpen: false);

        Assert.Equal(coordinate, editor.SelectedCoordinate);
        Assert.Equal("34.12346, -112.65432", editor.SelectedCoordinateStatus);
        Assert.True(editor.CanOpenNewPoint(isLibraryReady: true, isRightPanelOpen: false));
    }

    [Fact]
    public void ClearSelectedCoordinateIgnoresEscapeWhenRightPanelIsOpen()
    {
        var editor = new PointEditorViewModel();
        editor.SelectCoordinate(new MapCoordinate { Latitude = 1, Longitude = 2 }, isRightPanelOpen: false);

        editor.ClearSelectedCoordinate(isRightPanelOpen: true);

        Assert.NotNull(editor.SelectedCoordinate);

        editor.ClearSelectedCoordinate(isRightPanelOpen: false);

        Assert.Null(editor.SelectedCoordinate);
    }

    [Fact]
    public void OpenNewFromSelectedCoordinateCreatesDraftAndKeepsCoordinateUntilSaveOrClose()
    {
        var editor = new PointEditorViewModel();
        editor.SelectCoordinate(new MapCoordinate { Latitude = 10.5, Longitude = 20.25 }, isRightPanelOpen: false);

        var opened = editor.OpenNewFromSelectedCoordinate((latitude, longitude) => new PointDocument
        {
            Title = "New point",
            Latitude = latitude,
            Longitude = longitude,
        });

        Assert.True(opened);
        Assert.True(editor.IsNew);
        Assert.Equal(10.5, editor.Form?.Latitude);
        Assert.Equal(20.25, editor.Form?.Longitude);
        Assert.NotNull(editor.SelectedCoordinate);
    }

    [Fact]
    public void OpenNewCreatesDirtyNewForm()
    {
        var editor = new PointEditorViewModel();

        editor.OpenNew(CreatePoint("Draft"));

        Assert.True(editor.IsNew);
        Assert.True(editor.HasUnsavedChanges);
        Assert.Equal("Draft", editor.Form?.Title);
    }

    [Fact]
    public void OpenExistingIsCleanUntilFormChanges()
    {
        var editor = new PointEditorViewModel();
        editor.OpenExisting(CreatePoint("Saved"));

        Assert.False(editor.IsNew);
        Assert.False(editor.HasUnsavedChanges);

        editor.Form!.Title = "Changed";

        Assert.True(editor.HasUnsavedChanges);
    }

    [Fact]
    public void CanDiscardChangesRequiresConfirmationOnlyWhenDirty()
    {
        var editor = new PointEditorViewModel();
        editor.OpenExisting(CreatePoint("Saved"));

        Assert.False(editor.RequiresDiscardConfirmation);
        Assert.True(editor.CanDiscardChanges(userConfirmed: false));

        editor.Form!.Title = "Changed";

        Assert.True(editor.RequiresDiscardConfirmation);
        Assert.False(editor.CanDiscardChanges(userConfirmed: false));
        Assert.True(editor.CanDiscardChanges(userConfirmed: true));
    }

    [Fact]
    public void MarkSavedReplacesFormAndClearsDirtyState()
    {
        var editor = new PointEditorViewModel();
        editor.OpenNew(CreatePoint("Draft"));

        editor.MarkSaved(CreatePoint("Saved"));

        Assert.False(editor.IsNew);
        Assert.False(editor.HasUnsavedChanges);
        Assert.Equal("Saved", editor.Form?.Title);
    }

    [Fact]
    public void ToggleCollectionMakesLastAddedCollectionPrimary()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var editor = new PointEditorViewModel();
        editor.OpenExisting(CreatePoint("Saved"));

        editor.ToggleCollection(arizona, true, []);
        editor.ToggleCollection(summer, true, []);

        Assert.Equal([arizona, summer], editor.Form!.CollectionIds);
        Assert.Equal(summer, editor.Form.PrimaryCollectionId);
    }

    [Fact]
    public void RemovingPrimaryCollectionChoosesFirstRemainingByName()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var editor = new PointEditorViewModel();
        editor.OpenExisting(CreatePoint("Saved", [arizona, summer], summer));

        editor.ToggleCollection(
            summer,
            false,
            [
                new CollectionSummary(summer, "Summer"),
                new CollectionSummary(arizona, "Arizona"),
            ]);

        Assert.Equal([arizona], editor.Form!.CollectionIds);
        Assert.Equal(arizona, editor.Form.PrimaryCollectionId);
    }

    [Fact]
    public void SetPrimaryCollectionRejectsCollectionOutsidePoint()
    {
        var arizona = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var summer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var editor = new PointEditorViewModel();
        editor.OpenExisting(CreatePoint("Saved", arizona));

        editor.SetPrimaryCollection(summer);

        Assert.Null(editor.Form!.PrimaryCollectionId);
    }

    [Fact]
    public void KeepUnsavedAfterPartialSavePreservesDirtyState()
    {
        var saved = CreatePoint("Saved");
        var form = CreatePoint("Saved");
        form.Files.Add("guide__11111111111111111111111111111111.pdf");
        var editor = new PointEditorViewModel();

        editor.KeepUnsavedAfterPartialSave(form, saved);

        Assert.False(editor.IsNew);
        Assert.True(editor.HasUnsavedChanges);
        Assert.Equal(form.Files, editor.Form!.Files);
    }

    [Fact]
    public void SaveUsesCreateOrUpdateDelegateAndMarksSavedOnSuccess()
    {
        var editor = new PointEditorViewModel();
        editor.OpenNew(CreatePoint("Draft"));
        var saved = CreatePoint("Saved");

        var result = editor.Save(_ => PointSaveResult.Success(saved));

        Assert.Same(saved, result);
        Assert.False(editor.IsNew);
        Assert.False(editor.HasUnsavedChanges);
        Assert.Equal("Saved", editor.Form?.Title);
    }

    [Fact]
    public void SaveKeepsFormAndAppliesErrorsOnFailure()
    {
        var editor = new PointEditorViewModel();
        editor.OpenNew(CreatePoint(""));

        var result = editor.Save(_ => PointSaveResult.Failure(
            [new PointSaveError(PointService.TitleFieldName, "Title is required.")]));

        Assert.Null(result);
        Assert.True(editor.IsNew);
        Assert.True(editor.HasUnsavedChanges);
        Assert.Equal("Title is required.", editor.TitleError);
    }

    [Fact]
    public void SaveWithAttachmentsReturnsSavedPointWhenThereAreNoPendingAttachments()
    {
        var editor = new PointEditorViewModel();
        var attachments = new AttachmentDraftState();
        var saved = CreatePoint("Saved");
        editor.OpenExisting(CreatePoint("Draft"));

        var result = editor.SaveWithAttachments(
            _ => PointSaveResult.Success(saved),
            attachments,
            (_, _) => throw new InvalidOperationException("Import should not run."),
            _ => throw new InvalidOperationException("Attachment save should not run."));

        Assert.True(result.IsSuccess);
        Assert.Same(saved, result.Point);
        Assert.False(result.ShouldReloadLibrary);
        Assert.False(editor.HasUnsavedChanges);
    }

    [Fact]
    public void SaveWithAttachmentsImportsPendingFilesAndMarksImportedPointSaved()
    {
        var editor = new PointEditorViewModel();
        var attachments = new AttachmentDraftState();
        var firstSavedPoint = CreatePoint("Saved");
        var importedPoint = CreatePoint("Saved");
        importedPoint.Files.Add("guide__11111111111111111111111111111111.pdf");
        editor.OpenExisting(CreatePoint("Draft"));
        attachments.AddFiles(["guide.pdf"], _ => AttachmentKind.File);

        var result = editor.SaveWithAttachments(
            _ => PointSaveResult.Success(firstSavedPoint),
            attachments,
            (_, pendingAttachments) =>
            {
                Assert.Single(pendingAttachments);
                return new AttachmentImportResult(importedPoint, []);
            },
            point => PointSaveResult.Success(point));

        Assert.True(result.IsSuccess);
        Assert.Equal(importedPoint.Files, editor.Form!.Files);
        Assert.Empty(attachments.PendingAttachments);
        Assert.Empty(attachments.Errors);
        Assert.False(editor.HasUnsavedChanges);
    }

    [Fact]
    public void SaveWithAttachmentsKeepsPartialImportStateWhenImportedPointCannotBeSaved()
    {
        var editor = new PointEditorViewModel();
        var attachments = new AttachmentDraftState();
        var firstSavedPoint = CreatePoint("Saved");
        var importedPoint = CreatePoint("Saved");
        importedPoint.Files.Add("guide__11111111111111111111111111111111.pdf");
        editor.OpenExisting(CreatePoint("Draft"));
        attachments.AddFiles(["guide.pdf"], _ => AttachmentKind.File);

        var result = editor.SaveWithAttachments(
            _ => PointSaveResult.Success(firstSavedPoint),
            attachments,
            (_, _) => new AttachmentImportResult(importedPoint, [new AttachmentImportError("missing.pdf", "Source file was not found.")]),
            _ => PointSaveResult.Failure([new PointSaveError(PointService.GeneralFieldName, "Point JSON could not be saved.")]));

        Assert.False(result.IsSuccess);
        Assert.True(result.ShouldReloadLibrary);
        Assert.True(editor.HasUnsavedChanges);
        Assert.Equal(importedPoint.Files, editor.Form!.Files);
        Assert.Empty(attachments.PendingAttachments);
        Assert.Equal(
            ["missing.pdf: Source file was not found.", "Imported files were copied, but point JSON could not be updated. Try saving again."],
            attachments.Errors);
        Assert.Equal("Point JSON could not be saved.", editor.GeneralError);
    }

    [Fact]
    public void DeleteClearsEditorOnSuccess()
    {
        var editor = new PointEditorViewModel();
        var point = CreatePoint("Saved");
        editor.OpenExisting(point);

        var deleted = editor.Delete(pointId =>
        {
            Assert.Equal(point.Id, pointId);
            return PointDeleteResult.Success();
        });

        Assert.True(deleted);
        Assert.Null(editor.Form);
        Assert.False(editor.IsNew);
    }

    [Fact]
    public void DeleteClearsAttachmentStateOnSuccess()
    {
        var editor = new PointEditorViewModel();
        var attachments = new AttachmentDraftState();
        editor.OpenExisting(CreatePoint("Saved"));
        attachments.AddFiles(["guide.pdf"], _ => AttachmentKind.File);
        attachments.SetErrors(["Old attachment error."]);
        attachments.OpenPhotoViewer("photo.jpg");

        var deleted = editor.Delete(_ => PointDeleteResult.Success(), attachments);

        Assert.True(deleted);
        Assert.Empty(attachments.PendingAttachments);
        Assert.Empty(attachments.Errors);
        Assert.Null(attachments.PhotoViewerPath);
    }

    [Fact]
    public void DeleteKeepsFormAndAppliesErrorsOnFailure()
    {
        var editor = new PointEditorViewModel();
        editor.OpenExisting(CreatePoint("Saved"));

        var deleted = editor.Delete(_ => PointDeleteResult.Failure(["Point JSON was not found."]));

        Assert.False(deleted);
        Assert.NotNull(editor.Form);
        Assert.Equal(["Point JSON was not found."], editor.DeleteErrors);
    }

    [Fact]
    public void DeleteSavedAttachmentMarksReturnedPointSaved()
    {
        var editor = new PointEditorViewModel();
        var attachments = new AttachmentDraftState();
        var point = CreatePoint("Saved");
        point.Files.Add("guide__11111111111111111111111111111111.pdf");
        var updatedPoint = CreatePoint("Saved");
        editor.OpenExisting(point);
        var item = new AttachmentDisplayItem(
            AttachmentKind.File,
            "guide.pdf",
            "library/files/guide__11111111111111111111111111111111.pdf",
            "guide__11111111111111111111111111111111.pdf",
            IsPending: false);

        var deleted = editor.DeleteSavedAttachment(
            item,
            attachments,
            (form, kind, storedName) =>
            {
                Assert.Equal(point.Id, form.Id);
                Assert.Equal(AttachmentKind.File, kind);
                Assert.Equal("guide__11111111111111111111111111111111.pdf", storedName);
                return AttachmentDeleteResult.Success(updatedPoint);
            });

        Assert.True(deleted);
        Assert.Empty(editor.Form!.Files);
        Assert.False(editor.HasUnsavedChanges);
        Assert.Empty(attachments.Errors);
    }

    [Fact]
    public void DeleteSavedAttachmentKeepsFormAndSetsAttachmentErrorsOnFailure()
    {
        var editor = new PointEditorViewModel();
        var attachments = new AttachmentDraftState();
        var point = CreatePoint("Saved");
        point.Photos.Add("photo__11111111111111111111111111111111.jpg");
        editor.OpenExisting(point);
        var item = new AttachmentDisplayItem(
            AttachmentKind.Photo,
            "photo.jpg",
            "library/photos/photo__11111111111111111111111111111111.jpg",
            "photo__11111111111111111111111111111111.jpg",
            IsPending: false);

        var deleted = editor.DeleteSavedAttachment(
            item,
            attachments,
            (form, _, _) => AttachmentDeleteResult.Failure(form, ["Attachment could not be deleted."]));

        Assert.False(deleted);
        Assert.Equal(point.Photos, editor.Form!.Photos);
        Assert.Equal(["Attachment could not be deleted."], attachments.Errors);
    }

    [Fact]
    public void ApplySaveErrorsSplitsFieldErrorsForUi()
    {
        var editor = new PointEditorViewModel();

        editor.ApplySaveErrors(
            [
                new PointSaveError(PointService.TitleFieldName, "Title is required."),
                new PointSaveError(PointService.TagsFieldName, "Every tag must start with #."),
                new PointSaveError(PointService.GeneralFieldName, "Latitude is required."),
                new PointSaveError(PointService.GeneralFieldName, "Longitude is required."),
            ]);

        Assert.Equal("Title is required.", editor.TitleError);
        Assert.Equal("Every tag must start with #.", editor.TagsError);
        Assert.Equal("Latitude is required. Longitude is required.", editor.GeneralError);
    }

    [Fact]
    public void ClearErrorsClearsSaveAndDeleteErrors()
    {
        var editor = new PointEditorViewModel();
        editor.ApplySaveErrors([new PointSaveError(PointService.TitleFieldName, "Title is required.")]);
        editor.SetDeleteErrors(["Point JSON was not found."]);

        editor.ClearErrors();

        Assert.Null(editor.TitleError);
        Assert.Null(editor.TagsError);
        Assert.Null(editor.GeneralError);
        Assert.Empty(editor.DeleteErrors);
    }

    private static PointDocument CreatePoint(
        string title,
        params Guid[] collectionIds) =>
        CreatePoint(title, collectionIds, collectionIds.LastOrDefault());

    private static PointDocument CreatePoint(
        string title,
        Guid[] collectionIds,
        Guid primaryCollectionId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Latitude = 1,
            Longitude = 2,
            CollectionIds = collectionIds.ToList(),
            PrimaryCollectionId = primaryCollectionId == Guid.Empty ? null : primaryCollectionId,
        };
}
