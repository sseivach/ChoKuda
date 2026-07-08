# ChoKuda Manual Testing

Use this checklist for the first full manual UI acceptance pass on Windows.

## Test Setup

- Prepare a Stadia Maps API key.
- Prepare an empty library folder, or use `%USERPROFILE%\Documents\ChoKudaLibrary`.
- Prepare test files:
  - one valid `.jpg`;
  - one valid `.png` or `.webp`;
  - one regular `.pdf`;
  - one PDF renamed to `.jpg`;
  - one regular text file.

## First Launch

- Start the app.
- If no library is configured, verify that setup is shown.
- Click `Create default library` and verify the folder structure is created.
- Restart the app and verify the same library is loaded.
- Repeat with `Choose folder` if testing a custom library path.

## Map

- With no Stadia key, verify that the API key screen is shown.
- Enter a Stadia key and save it.
- Verify the map loads.
- Click the map and verify the temporary marker appears.
- Verify coordinates appear in the bottom bar.
- Press `Esc` and verify the marker and coordinates disappear.
- Verify `Add point` is enabled only after a map click.

## Points

- Click the map and create a point.
- Fill title, address, description, and tags.
- Verify invalid tags block save.
- Save the point and verify the pin appears.
- Restart the app and verify the point remains.
- Click the pin and verify the point opens.
- Edit fields and save.
- Delete the point and verify confirmation is required.

## Collections

- Create a collection in the left panel.
- Choose a Bootstrap Icon and color.
- Verify duplicate names are rejected.
- Add a point to multiple collections.
- Verify the last added collection becomes primary.
- Manually change primary collection.
- Save and verify the pin style follows primary collection.
- Delete a primary collection and verify the point primary collection is recalculated.

## Filters

- Test collection filter `OR`.
- Test collection filter `AND`.
- Drag selected collection filters and verify pin style priority changes.
- Test tag filter `OR`.
- Test tag filter `AND`.
- Verify collection and tag filters combine with `AND`.
- Verify `Reset collections` does not reset tags or search.
- Verify `Reset tags` does not reset collections or search.

## Search

- Verify `Search` is disabled for empty input and whitespace.
- Search by point title.
- Search by address/region.
- Search by description.
- Search by tag.
- Verify `rent` and `#rent` both find `#rentcar`.
- Verify search and filters combine with `AND`.
- Click a result and verify the point opens.
- Verify the map centers on the result without changing zoom.
- Verify `Reset search` does not reset collection or tag filters.

## Photos And Files

- Click `Add files`.
- Select valid images, PDF, text file, and renamed invalid `.jpg` together.
- Before `Save`, verify files are not copied into the library.
- Verify valid images appear under Photos.
- Verify invalid `.jpg` appears under Files.
- Save the point.
- Verify photos are copied to `library/photos/`.
- Verify files are copied to `library/files/`.
- Verify point JSON stores file names only, not full paths.
- Verify stored names use `original__guid.ext`.
- Click a photo and verify the viewer opens.
- Click a file and verify Windows opens it with the system app.
- Delete a saved attachment and verify confirmation is required.
- Add a draft attachment and remove it before save; verify no confirmation is shown.

## Error Cases

- Try saving a point with empty title.
- Try saving invalid tags such as `#rent car`.
- Try importing a file that was deleted after selection.
- Try opening a missing file listed in JSON.
- Try deleting a locked attachment file.
- Verify errors are visible and understandable.

## Final Checks

Run:

```powershell
dotnet build ChoKuda.slnx
dotnet test ChoKuda.slnx
dotnet test tests\ChoKuda.Core.Tests\ChoKuda.Core.Tests.csproj --collect:"XPlat Code Coverage"
```

Expected:

- build passes;
- tests pass;
- Core coverage remains 100% line and 100% branch.
