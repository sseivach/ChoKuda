# Known Limitations

This file tracks accepted MVP limitations, not bugs.

- Full manual UI acceptance has not been completed yet.
- The map background requires internet access and a Stadia Maps API key.
- There are no offline maps, region downloads, preload, or bulk tile downloads.
- ChoKuda data is local only; there is no sync, cloud backup, sharing, or collaboration.
- There are no routes, trip calendars, budgets, day plans, or itinerary planning.
- Address/region is entered manually; there is no geocoding or reverse geocoding.
- Point coordinates cannot be edited or dragged after creation.
- File import uses a button and system picker; drag-and-drop is not implemented.
- Photo preview is simple; there is no zoom, pan, crop, edit, or generated thumbnail cache.
- Tags are edited as plain text and are not managed by a separate tag manager.
- Search does not cover files, photos, coordinates, or collection names.
- Search has no fuzzy matching, transliteration, morphology, or relevance ranking.
- Bootstrap Icons are selected from the local list included in the app, not the full upstream catalog.
- Error recovery is conservative and local-file based; there is no database transaction layer.
