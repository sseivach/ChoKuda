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
    public void SelectIconUpdatesFormAndSearchText()
    {
        var editor = new CollectionEditorViewModel();
        editor.OpenExisting(CreateCollection("Saved"));

        editor.SelectIcon("sun-fill");

        Assert.True(editor.IsIconSelected("sun-fill"));
        Assert.Equal("sun-fill", editor.Form?.IconId);
        Assert.Equal("sun-fill", editor.IconSearch);
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
}
