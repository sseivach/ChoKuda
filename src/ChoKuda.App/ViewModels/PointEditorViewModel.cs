using ChoKuda.Core.Domain;
using ChoKuda.Core.Attachments;
using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Map;
using ChoKuda.Core.Points;

namespace ChoKuda.App.ViewModels;

public sealed class PointEditorViewModel
{
    private PointDocument? _savedSnapshot;

    public PointDocument? Form { get; private set; }

    public bool IsNew { get; private set; }

    public string? TitleError { get; private set; }

    public string? TagsError { get; private set; }

    public string? GeneralError { get; private set; }

    public IReadOnlyList<string> DeleteErrors { get; private set; } = Array.Empty<string>();

    public MapCoordinate? SelectedCoordinate { get; private set; }

    public string SelectedCoordinateStatus =>
        SelectedCoordinate is null
            ? "No map point selected"
            : $"{SelectedCoordinate.Latitude:F5}, {SelectedCoordinate.Longitude:F5}";

    public bool HasUnsavedChanges =>
        Form is not null &&
        (IsNew || !PointDocumentsEqual(Form, _savedSnapshot));

    public bool RequiresDiscardConfirmation =>
        HasUnsavedChanges;

    public bool CanDiscardChanges(bool userConfirmed) =>
        !RequiresDiscardConfirmation || userConfirmed;

    public bool CanOpenNewPoint(
        bool isLibraryReady,
        bool isRightPanelOpen) =>
        isLibraryReady &&
        SelectedCoordinate is not null &&
        !isRightPanelOpen;

    public void SelectCoordinate(
        MapCoordinate coordinate,
        bool isRightPanelOpen)
    {
        if (isRightPanelOpen)
        {
            return;
        }

        SelectedCoordinate = coordinate;
    }

    public void ClearSelectedCoordinate(bool isRightPanelOpen)
    {
        if (isRightPanelOpen)
        {
            return;
        }

        ClearSelectedCoordinate();
    }

    public void ClearSelectedCoordinate()
    {
        SelectedCoordinate = null;
    }

    public void OpenNew(PointDocument draft)
    {
        Form = ClonePoint(draft);
        _savedSnapshot = null;
        IsNew = true;
    }

    public bool OpenNewFromSelectedCoordinate(Func<double, double, PointDocument> createDraft)
    {
        if (SelectedCoordinate is null)
        {
            return false;
        }

        OpenNew(createDraft(SelectedCoordinate.Latitude, SelectedCoordinate.Longitude));
        return true;
    }

    public void OpenExisting(PointDocument point)
    {
        ClearSelectedCoordinate();
        Form = ClonePoint(point);
        _savedSnapshot = ClonePoint(point);
        IsNew = false;
    }

    public void MarkSaved(PointDocument point)
    {
        Form = ClonePoint(point);
        _savedSnapshot = ClonePoint(point);
        IsNew = false;
        ClearSelectedCoordinate();
    }

    public void KeepUnsavedAfterPartialSave(
        PointDocument form,
        PointDocument savedSnapshot)
    {
        Form = ClonePoint(form);
        _savedSnapshot = ClonePoint(savedSnapshot);
        IsNew = false;
    }

    public PointDocument? Save(Func<PointDocument, PointSaveResult> save)
    {
        if (Form is null)
        {
            return null;
        }

        ClearErrors();
        var result = save(Form);

        if (!result.IsSuccess || result.Point is null)
        {
            ApplySaveErrors(result.Errors);
            return null;
        }

        MarkSaved(result.Point);
        return result.Point;
    }

    public PointEditorSaveResult SaveWithAttachments(
        Func<PointDocument, PointSaveResult> savePoint,
        AttachmentDraftState attachments,
        Func<PointDocument, IReadOnlyList<PendingAttachment>, AttachmentImportResult> importPendingAttachments,
        Func<PointDocument, PointSaveResult> saveImportedPoint)
    {
        var savedPoint = Save(savePoint);

        if (savedPoint is null)
        {
            return PointEditorSaveResult.Failure();
        }

        if (!attachments.HasPending)
        {
            return PointEditorSaveResult.Success(savedPoint);
        }

        var importResult = importPendingAttachments(savedPoint, attachments.PendingAttachments);
        attachments.SetImportErrors(importResult.Errors);
        attachments.ClearPending();

        var attachmentSaveResult = saveImportedPoint(importResult.Point);

        if (!attachmentSaveResult.IsSuccess || attachmentSaveResult.Point is null)
        {
            ApplySaveErrors(attachmentSaveResult.Errors);
            attachments.AddError("Imported files were copied, but point JSON could not be updated. Try saving again.");
            KeepUnsavedAfterPartialSave(importResult.Point, savedPoint);
            return PointEditorSaveResult.Failure(shouldReloadLibrary: true);
        }

        MarkSaved(attachmentSaveResult.Point);
        return PointEditorSaveResult.Success(attachmentSaveResult.Point);
    }

    public bool Delete(
        Func<Guid, PointDeleteResult> delete,
        AttachmentDraftState? attachments = null)
    {
        if (Form is null || IsNew)
        {
            return false;
        }

        ClearErrors();
        attachments?.ClearErrors();
        var result = delete(Form.Id);

        if (!result.IsSuccess)
        {
            SetDeleteErrors(result.Errors);
            return false;
        }

        Clear();
        attachments?.ClearAll();
        return true;
    }

    public bool DeleteSavedAttachment(
        AttachmentDisplayItem item,
        AttachmentDraftState attachments,
        Func<PointDocument, AttachmentKind, string, AttachmentDeleteResult> deleteSavedAttachment)
    {
        if (Form is null || item.IsPending)
        {
            return false;
        }

        attachments.ClearErrors();
        var result = deleteSavedAttachment(Form, item.Kind, item.StoredName);

        if (!result.IsSuccess)
        {
            attachments.SetErrors(result.Errors);
            return false;
        }

        MarkSaved(result.Point);
        return true;
    }

    public void Clear()
    {
        Form = null;
        _savedSnapshot = null;
        IsNew = false;
        ClearErrors();
    }

    public void ApplySaveErrors(IReadOnlyList<PointSaveError> errors)
    {
        TitleError = errors.FirstOrDefault(error => error.FieldName == PointService.TitleFieldName)?.Message;
        TagsError = errors.FirstOrDefault(error => error.FieldName == PointService.TagsFieldName)?.Message;
        GeneralError = string.Join(
            " ",
            errors
                .Where(error => error.FieldName == PointService.GeneralFieldName)
                .Select(error => error.Message));
    }

    public void SetDeleteErrors(IReadOnlyList<string> errors)
    {
        DeleteErrors = errors;
    }

    public void ClearErrors()
    {
        TitleError = null;
        TagsError = null;
        GeneralError = null;
        DeleteErrors = Array.Empty<string>();
    }

    public void ToggleCollection(
        Guid collectionId,
        bool isSelected,
        IReadOnlyCollection<CollectionSummary> availableCollections)
    {
        if (Form is null)
        {
            return;
        }

        if (isSelected)
        {
            if (!Form.CollectionIds.Contains(collectionId))
            {
                Form.CollectionIds.Add(collectionId);
            }

            Form.PrimaryCollectionId = PointCollectionRules.AddCollection(
                Form.CollectionIds,
                Form.PrimaryCollectionId,
                collectionId);
            return;
        }

        Form.CollectionIds.Remove(collectionId);
        Form.PrimaryCollectionId = PointCollectionRules.RemoveCollection(
            Form.CollectionIds,
            collectionId,
            Form.PrimaryCollectionId,
            availableCollections);
    }

    public void SetPrimaryCollection(Guid? collectionId)
    {
        if (Form is null)
        {
            return;
        }

        Form.PrimaryCollectionId = collectionId.HasValue && Form.CollectionIds.Contains(collectionId.Value)
            ? collectionId
            : null;
    }

    public bool IsInCollection(Guid collectionId) =>
        Form?.CollectionIds.Contains(collectionId) == true;

    private static PointDocument ClonePoint(PointDocument point) =>
        new()
        {
            Id = point.Id,
            Title = point.Title,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            AddressRegion = point.AddressRegion,
            DescriptionText = point.DescriptionText,
            CollectionIds = point.CollectionIds.ToList(),
            PrimaryCollectionId = point.PrimaryCollectionId,
            TagsText = point.TagsText,
            Photos = point.Photos.ToList(),
            Files = point.Files.ToList(),
        };

    private static bool PointDocumentsEqual(PointDocument point, PointDocument? other) =>
        other is not null &&
        point.Id == other.Id &&
        point.Title == other.Title &&
        point.Latitude.Equals(other.Latitude) &&
        point.Longitude.Equals(other.Longitude) &&
        point.AddressRegion == other.AddressRegion &&
        point.DescriptionText == other.DescriptionText &&
        point.CollectionIds.SequenceEqual(other.CollectionIds) &&
        point.PrimaryCollectionId == other.PrimaryCollectionId &&
        point.TagsText == other.TagsText &&
        point.Photos.SequenceEqual(other.Photos) &&
        point.Files.SequenceEqual(other.Files);
}
