using ChoKuda.Core.Collections;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.ViewModels;

public sealed class CollectionEditorViewModel
{
    private static readonly IReadOnlyList<string> BootstrapIconIds =
    [
        "geo-alt-fill", "pin-map-fill", "map-fill", "compass-fill", "camera-fill", "image-fill",
        "sun-fill", "moon-stars-fill", "cloud-sun-fill", "water", "tree-fill", "flower1",
        "fire", "umbrella-fill", "snow", "star-fill", "heart-fill", "flag-fill", "bookmark-fill",
        "signpost-fill", "binoculars-fill", "backpack-fill", "car-front-fill", "taxi-front-fill",
        "bus-front-fill", "train-front-fill", "airplane-fill", "bicycle", "scooter", "fuel-pump-fill",
        "house-door-fill", "building-fill", "buildings-fill", "hospital-fill", "shop", "basket-fill",
        "cart-fill", "bag-fill", "cup-hot-fill", "cup-straw", "egg-fried", "cake2-fill",
        "fork-knife", "geo-fill", "crosshair", "bullseye", "diamond-fill", "circle-fill",
        "square-fill", "triangle-fill", "hexagon-fill", "lightning-fill", "gem", "key-fill",
        "shield-fill", "calendar-event-fill", "clock-fill", "wifi", "telephone-fill", "link-45deg",
        "file-earmark-pdf-fill", "journal-text", "book-fill", "palette-fill", "tools", "gear-fill",
        "person-walking", "cash-coin", "gift-fill", "trophy-fill", "rocket-takeoff-fill", "magic",
        "brightness-high-fill", "thermometer-sun", "droplet-fill", "wind", "layers-fill",
        "collection-fill", "tags-fill", "filter-circle-fill", "funnel-fill", "search", "grip-vertical",
        "trash-fill", "pencil-fill", "save-fill",
    ];

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
        BootstrapIconIds
            .Where(iconId => string.IsNullOrWhiteSpace(IconSearch) || iconId.Contains(IconSearch.Trim(), StringComparison.OrdinalIgnoreCase))
            .Take(48);

    public bool RequiresDiscardConfirmation =>
        HasUnsavedChanges;

    public bool CanDiscardChanges(bool userConfirmed) =>
        !RequiresDiscardConfirmation || userConfirmed;

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
