using ChoKuda.App.ViewModels;
using ChoKuda.Core.Collections;
using ChoKuda.Core.FileLibrary;

namespace ChoKuda.App.Tests.ViewModels;

public sealed class CollectionEditorViewModelTests
{
    [Fact]
    public void OpenNewCreatesDirtyNewForm()
    {
        var editor = new CollectionEditorViewModel();

        editor.OpenNew(CreateCollection("Draft"));

        Assert.True(editor.IsNew);
        Assert.True(editor.HasUnsavedChanges);
        Assert.Equal("Draft", editor.Form?.Name);
        Assert.Equal(string.Empty, editor.IconSearch);
    }

    [Fact]
    public void OpenNewWithServiceCreatesDefaultDraftAndClearsErrors()
    {
        var editor = new CollectionEditorViewModel();
        editor.OpenNew(CreateCollection(""));
        editor.Save(_ => CollectionSaveResult.Failure(
            [new CollectionSaveError(CollectionService.NameFieldName, "Name is required.")]));
        editor.IconSearch = "water";

        editor.OpenNew(new CollectionService());

        Assert.True(editor.IsNew);
        Assert.True(editor.HasUnsavedChanges);
        Assert.Equal("New collection", editor.Form?.Name);
        Assert.Equal(CollectionService.DefaultIconId, editor.Form?.IconId);
        Assert.Null(editor.NameError);
        Assert.Equal(string.Empty, editor.IconSearch);
    }

    [Fact]
    public void OpenExistingIsCleanUntilFormChanges()
    {
        var editor = new CollectionEditorViewModel();
        editor.OpenExisting(CreateCollection("Saved"));

        Assert.False(editor.IsNew);
        Assert.False(editor.HasUnsavedChanges);

        editor.Form!.Name = "Changed";

        Assert.True(editor.HasUnsavedChanges);
    }

    [Fact]
    public void OpenExistingByIdUsesCollectionListAndClearsErrors()
    {
        var editor = new CollectionEditorViewModel();
        var saved = CreateCollection("Saved");
        editor.OpenNew(CreateCollection(""));
        editor.Save(_ => CollectionSaveResult.Failure(
            [new CollectionSaveError(CollectionService.NameFieldName, "Name is required.")]));

        var opened = editor.OpenExisting(saved.Id, [saved]);

        Assert.True(opened);
        Assert.False(editor.IsNew);
        Assert.False(editor.HasUnsavedChanges);
        Assert.Equal(saved.Id, editor.Form?.Id);
        Assert.Equal(string.Empty, editor.IconSearch);
        Assert.Null(editor.NameError);
    }

    [Fact]
    public void OpenExistingByIdReturnsFalseAndKeepsCurrentFormWhenCollectionIsMissing()
    {
        var editor = new CollectionEditorViewModel();
        editor.OpenExisting(CreateCollection("Current"));

        var opened = editor.OpenExisting(Guid.NewGuid(), []);

        Assert.False(opened);
        Assert.Equal("Current", editor.Form?.Name);
        Assert.False(editor.IsNew);
    }

    [Fact]
    public void SelectIconUpdatesFormAndKeepsSearchText()
    {
        var editor = new CollectionEditorViewModel();
        editor.OpenExisting(CreateCollection("Saved"));
        editor.IconSearch = "sun";

        editor.SelectIcon("sun-fill");

        Assert.True(editor.IsIconSelected("sun-fill"));
        Assert.Equal("sun-fill", editor.Form?.IconId);
        Assert.Equal("sun", editor.IconSearch);
    }

    [Fact]
    public void FilteredIconIdsUsesIconSearchAndLimitsResults()
    {
        var editor = new CollectionEditorViewModel
        {
            IconSearch = "fill",
        };

        var icons = editor.FilteredIconIds.ToArray();

        Assert.NotEmpty(icons);
        Assert.True(icons.Length <= 48);
        Assert.All(icons, icon => Assert.Contains("fill", icon, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SetIconIdsUsesProvidedCatalogForSearch()
    {
        var editor = new CollectionEditorViewModel();
        editor.SetIconIds(["alpha", "waterfall", "zoom-in"]);
        editor.IconSearch = "zoom";

        var icons = editor.FilteredIconIds.ToArray();

        Assert.Equal(["zoom-in"], icons);
    }

    [Fact]
    public void IconChoiceClassMarksSelectedIcon()
    {
        var editor = new CollectionEditorViewModel();
        editor.OpenExisting(CreateCollection("Saved"));

        editor.SelectIcon("sun-fill");

        Assert.Equal("icon-choice selected", editor.IconChoiceClass("sun-fill"));
        Assert.Equal("icon-choice", editor.IconChoiceClass("moon-stars-fill"));
    }

    [Fact]
    public void SaveMarksReturnedCollectionSaved()
    {
        var editor = new CollectionEditorViewModel();
        var saved = CreateCollection("Saved");
        editor.OpenNew(CreateCollection("Draft"));

        var result = editor.Save(_ => CollectionSaveResult.Success(saved));

        Assert.Same(saved, result);
        Assert.False(editor.IsNew);
        Assert.False(editor.HasUnsavedChanges);
        Assert.Equal("Saved", editor.Form?.Name);
    }

    [Fact]
    public void SaveWithCollectionServiceCreatesNewCollection()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var editor = new CollectionEditorViewModel();
        editor.OpenNew(CreateCollection("  Arizona  "));

        var result = editor.Save(paths, new CollectionService(), []);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Arizona", result.Name);
        Assert.False(editor.IsNew);
        Assert.False(editor.HasUnsavedChanges);
        Assert.True(File.Exists(paths.GetCollectionFilePath(result.Id)));
    }

    [Fact]
    public void SaveWithCollectionServiceUpdatesExistingCollection()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var collectionService = new CollectionService();
        var savedCollection = collectionService.CreateCollection(paths, CreateCollection("Arizona"), []).Collection!;
        var editor = new CollectionEditorViewModel();
        editor.OpenExisting(savedCollection);
        editor.Form!.Name = "  Mexico  ";

        var result = editor.Save(paths, collectionService, [savedCollection]);

        Assert.NotNull(result);
        Assert.Equal(savedCollection.Id, result.Id);
        Assert.Equal("Mexico", result.Name);
        Assert.False(editor.IsNew);
        Assert.False(editor.HasUnsavedChanges);
    }

    [Fact]
    public void SaveAppliesFieldErrorsOnFailure()
    {
        var editor = new CollectionEditorViewModel();
        editor.OpenNew(CreateCollection(""));

        var result = editor.Save(_ => CollectionSaveResult.Failure(
            [
                new CollectionSaveError(CollectionService.NameFieldName, "Name is required."),
                new CollectionSaveError(CollectionService.ColorFieldName, "Color must use #rrggbb format."),
                new CollectionSaveError(CollectionService.GeneralFieldName, "Collection id is required."),
            ]));

        Assert.Null(result);
        Assert.Equal("Name is required.", editor.NameError);
        Assert.Equal("Color must use #rrggbb format.", editor.ColorError);
        Assert.Equal("Collection id is required.", editor.GeneralError);
    }

    [Fact]
    public void DeleteClearsEditorOnSuccess()
    {
        var editor = new CollectionEditorViewModel();
        var collection = CreateCollection("Saved");
        editor.OpenExisting(collection);

        var deleted = editor.Delete(collectionId =>
        {
            Assert.Equal(collection.Id, collectionId);
            return CollectionDeleteResult.Success();
        });

        Assert.True(deleted);
        Assert.Null(editor.Form);
        Assert.False(editor.IsNew);
    }

    [Fact]
    public void DeleteWithCollectionServiceDeletesSavedCollection()
    {
        using var temp = TempDirectory.Create();
        var paths = new FileLibraryService().EnsureLibrary(temp.Path);
        var collectionService = new CollectionService();
        var savedCollection = collectionService.CreateCollection(paths, CreateCollection("Arizona"), []).Collection!;
        var editor = new CollectionEditorViewModel();
        editor.OpenExisting(savedCollection);

        var deleted = editor.Delete(paths, collectionService);

        Assert.True(deleted);
        Assert.Null(editor.Form);
        Assert.False(File.Exists(paths.GetCollectionFilePath(savedCollection.Id)));
    }

    [Fact]
    public void DeleteKeepsFormAndAppliesErrorsOnFailure()
    {
        var editor = new CollectionEditorViewModel();
        editor.OpenExisting(CreateCollection("Saved"));

        var deleted = editor.Delete(_ => CollectionDeleteResult.Failure(["Collection JSON was not found."]));

        Assert.False(deleted);
        Assert.NotNull(editor.Form);
        Assert.Equal(["Collection JSON was not found."], editor.DeleteErrors);
    }

    [Fact]
    public void CanDiscardChangesRequiresConfirmationOnlyWhenDirty()
    {
        var editor = new CollectionEditorViewModel();
        editor.OpenExisting(CreateCollection("Saved"));

        Assert.False(editor.RequiresDiscardConfirmation);
        Assert.True(editor.CanDiscardChanges(userConfirmed: false));

        editor.Form!.Color = "#ff0000";

        Assert.True(editor.RequiresDiscardConfirmation);
        Assert.False(editor.CanDiscardChanges(userConfirmed: false));
        Assert.True(editor.CanDiscardChanges(userConfirmed: true));
    }

    private static CollectionDocument CreateCollection(string name) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            IconId = "geo-alt-fill",
            Color = "#3366ff",
            DescriptionText = "Description",
        };

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public static TempDirectory Create() =>
            new(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
