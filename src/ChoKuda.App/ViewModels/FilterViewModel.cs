using ChoKuda.Core.FileLibrary;
using ChoKuda.Core.Filters;

namespace ChoKuda.App.ViewModels;

public sealed class FilterViewModel
{
    private Guid? _draggedCollectionId;

    public List<Guid> SelectedCollectionIds { get; } = [];

    public List<string> SelectedTags { get; } = [];

    public FilterMode CollectionMode { get; private set; } = FilterMode.Any;

    public FilterMode TagMode { get; private set; } = FilterMode.Any;

    public IReadOnlyList<PointDocument> Apply(IEnumerable<PointDocument> points) =>
        PointFilterService.Apply(
            points,
            new PointFilterOptions(
                SelectedCollectionIds,
                CollectionMode,
                SelectedTags,
                TagMode));

    public void RemoveMissingValues(
        IEnumerable<Guid> availableCollectionIds,
        IEnumerable<string> availableTags)
    {
        var collectionIds = availableCollectionIds.ToHashSet();
        var tags = availableTags.ToHashSet(StringComparer.Ordinal);

        SelectedCollectionIds.RemoveAll(collectionId => !collectionIds.Contains(collectionId));
        SelectedTags.RemoveAll(tag => !tags.Contains(tag));
    }

    public void ToggleCollection(Guid collectionId, bool isSelected)
    {
        if (isSelected && !SelectedCollectionIds.Contains(collectionId))
        {
            SelectedCollectionIds.Add(collectionId);
        }
        else if (!isSelected)
        {
            SelectedCollectionIds.Remove(collectionId);
        }
    }

    public void ToggleTag(string tag, bool isSelected)
    {
        if (isSelected && !SelectedTags.Contains(tag, StringComparer.Ordinal))
        {
            SelectedTags.Add(tag);
        }
        else if (!isSelected)
        {
            SelectedTags.Remove(tag);
        }
    }

    public void SetCollectionMode(FilterMode mode)
    {
        CollectionMode = mode;
    }

    public void SetTagMode(FilterMode mode)
    {
        TagMode = mode;
    }

    public void ResetCollections()
    {
        SelectedCollectionIds.Clear();
    }

    public void ResetTags()
    {
        SelectedTags.Clear();
    }

    public void StartCollectionDrag(Guid collectionId)
    {
        _draggedCollectionId = collectionId;
    }

    public bool DropCollection(Guid targetCollectionId)
    {
        if (!_draggedCollectionId.HasValue ||
            _draggedCollectionId.Value == targetCollectionId)
        {
            _draggedCollectionId = null;
            return false;
        }

        SelectedCollectionIds.Remove(_draggedCollectionId.Value);
        var targetIndex = SelectedCollectionIds.IndexOf(targetCollectionId);
        SelectedCollectionIds.Insert(targetIndex, _draggedCollectionId.Value);
        _draggedCollectionId = null;
        return true;
    }
}
