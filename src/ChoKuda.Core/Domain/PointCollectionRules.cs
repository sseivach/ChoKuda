namespace ChoKuda.Core.Domain;

public static class PointCollectionRules
{
    public static Guid? AddCollection(
        IReadOnlyCollection<Guid> collectionIds,
        Guid? currentPrimaryCollectionId,
        Guid addedCollectionId)
    {
        return addedCollectionId;
    }

    public static Guid? RemoveCollection(
        IReadOnlyCollection<Guid> remainingCollectionIds,
        Guid removedCollectionId,
        Guid? currentPrimaryCollectionId,
        IReadOnlyCollection<CollectionSummary> availableCollections)
    {
        if (remainingCollectionIds.Count == 0)
        {
            return null;
        }

        if (!currentPrimaryCollectionId.HasValue)
        {
            return ChooseFirstCollectionByName(remainingCollectionIds, availableCollections);
        }

        if (currentPrimaryCollectionId.Value == removedCollectionId)
        {
            return ChooseFirstCollectionByName(remainingCollectionIds, availableCollections);
        }

        if (!remainingCollectionIds.Contains(currentPrimaryCollectionId.Value))
        {
            return ChooseFirstCollectionByName(remainingCollectionIds, availableCollections);
        }

        return currentPrimaryCollectionId;
    }

    private static Guid? ChooseFirstCollectionByName(
        IReadOnlyCollection<Guid> remainingCollectionIds,
        IReadOnlyCollection<CollectionSummary> availableCollections)
    {
        return availableCollections
            .Where(collection => remainingCollectionIds.Contains(collection.Id))
            .Order(CollectionNameComparer.Instance)
            .FirstOrDefault()
            ?.Id;
    }
}
