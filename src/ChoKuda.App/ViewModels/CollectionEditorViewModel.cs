using ChoKuda.Core.Collections;
using ChoKuda.Core.FileLibrary;
using ChoKuda.App.Services;

namespace ChoKuda.App.ViewModels;

public sealed class CollectionEditorViewModel
{
    private IReadOnlyList<string> _bootstrapIconIds = BootstrapIconCatalog.PreferredIconIds;
    private CollectionDocument? _savedSnapshot;

    public CollectionDocument? Form { get; private set; }

    public bool IsNew { get; private set; }

    public string IconSearch { get; set; } = string.Empty;

    public string? NameError { get; private set; }

    public string? ColorError { get; private set; }

    public string? GeneralError { get; private set; }

    public IReadOnlyList<string> DeleteErrors { get; private set; } = Array.Empty<string>();

    public bool HasUnsavedChanges =>
        Form is not null &&
        (IsNew || !CollectionDocumentsEqual(Form, _savedSnapshot));

    public IEnumerable<string> FilteredIconIds =>
        _bootstrapIconIds
            .Where(iconId => string.IsNullOrWhiteSpace(IconSearch) || iconId.Contains(IconSearch.Trim(), StringComparison.OrdinalIgnoreCase))
            .Take(48);

    public bool RequiresDiscardConfirmation =>
        HasUnsavedChanges;

    public bool CanDiscardChanges(bool userConfirmed) =>
        !RequiresDiscardConfirmation || userConfirmed;

    public void SetIconIds(IEnumerable<string> iconIds)
    {
        var normalizedIconIds = iconIds
            .Where(iconId => !string.IsNullOrWhiteSpace(iconId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        _bootstrapIconIds = normalizedIconIds.Length == 0
            ? BootstrapIconCatalog.PreferredIconIds
            : normalizedIconIds;
    }

    public void OpenNew(CollectionDocument draft)
    {
        Form = CloneCollection(draft);
        _savedSnapshot = null;
        IsNew = true;
        IconSearch = string.Empty;
        ClearErrors();
    }

    public void OpenExisting(CollectionDocument collection)
    {
        Form = CloneCollection(collection);
        _savedSnapshot = CloneCollection(collection);
        IsNew = false;
        IconSearch = collection.IconId;
        ClearErrors();
    }

    public CollectionDocument? Save(Func<CollectionDocument, CollectionSaveResult> save)
    {
        if (Form is null)
        {
            return null;
        }

        ClearErrors();
        var result = save(Form);

        if (!result.IsSuccess || result.Collection is null)
        {
            ApplySaveErrors(result.Errors);
            return null;
        }

        MarkSaved(result.Collection);
        return result.Collection;
    }

    public bool Delete(Func<Guid, CollectionDeleteResult> delete)
    {
        if (Form is null || IsNew)
        {
            return false;
        }

        ClearErrors();
        var result = delete(Form.Id);

        if (!result.IsSuccess)
        {
            DeleteErrors = result.Errors;
            return false;
        }

        Clear();
        return true;
    }

    public void Clear()
    {
        Form = null;
        _savedSnapshot = null;
        IsNew = false;
        IconSearch = string.Empty;
        ClearErrors();
    }

    public void SelectIcon(string iconId)
    {
        if (Form is null)
        {
            return;
        }

        Form.IconId = iconId;
        IconSearch = iconId;
    }

    public bool IsIconSelected(string iconId) =>
        Form?.IconId == iconId;

    public string IconChoiceClass(string iconId) =>
        IsIconSelected(iconId)
            ? "icon-choice selected"
            : "icon-choice";

    public void ClearErrors()
    {
        NameError = null;
        ColorError = null;
        GeneralError = null;
        DeleteErrors = Array.Empty<string>();
    }

    private void MarkSaved(CollectionDocument collection)
    {
        Form = CloneCollection(collection);
        _savedSnapshot = CloneCollection(collection);
        IsNew = false;
    }

    private void ApplySaveErrors(IReadOnlyList<CollectionSaveError> errors)
    {
        NameError = errors.FirstOrDefault(error => error.FieldName == CollectionService.NameFieldName)?.Message;
        ColorError = errors.FirstOrDefault(error => error.FieldName == CollectionService.ColorFieldName)?.Message;
        GeneralError = string.Join(
            " ",
            errors
                .Where(error => error.FieldName == CollectionService.GeneralFieldName)
                .Select(error => error.Message));
    }

    private static CollectionDocument CloneCollection(CollectionDocument collection) =>
        new()
        {
            Id = collection.Id,
            Name = collection.Name,
            IconId = collection.IconId,
            Color = collection.Color,
            DescriptionText = collection.DescriptionText,
        };

    private static bool CollectionDocumentsEqual(CollectionDocument collection, CollectionDocument? other) =>
        other is not null &&
        collection.Id == other.Id &&
        collection.Name == other.Name &&
        collection.IconId == other.IconId &&
        collection.Color == other.Color &&
        collection.DescriptionText == other.DescriptionText;
}
