# Known Limitations

This file tracks accepted MVP limitations, not bugs.

- Full manual UI acceptance has not been completed yet.
- The map background requires internet access and a Stadia Maps API key.
- There are no offline maps, region downloads, preload, or bulk tile downloads.
- ChoKuda data is local only; there is no sync, cloud backup, sharing, collaboration, automatic backup service, or migration runner.
- Backup is manual: close ChoKuda and copy the whole selected `ChoKudaLibrary` folder.
- There are no routes, trip calendars, budgets, day plans, or itinerary planning.
- Address/region is entered manually; there is no geocoding or reverse geocoding.
- Point coordinates cannot be edited or dragged after creation.
- Photo preview supports basic zoom and scrollbar panning; there is no crop, edit, or generated thumbnail cache.
- Tags are edited as plain text and are not managed by a separate tag manager.
- Search does not cover files, photos, coordinates, or collection names.
- Search has no fuzzy matching, transliteration, morphology, or relevance ranking.
- Error recovery is conservative and local-file based; there is no database transaction layer.
