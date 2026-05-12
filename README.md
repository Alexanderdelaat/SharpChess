# SharpChess

## Documentation board sync

The documentation site reads the project board from Notion in one direction only:

`Notion -> generated Markdown -> DocFX -> GitHub Pages`

The sync is read-only. The integration only calls Notion read endpoints and never creates, updates, deletes, moves, archives, or reorders cards.

### Required GitHub Actions secrets

Configure these in `Settings -> Secrets and variables -> Actions` for the repository:

- `NOTION_TOKEN`
- `NOTION_DATABASE_ID`

If the Notion database contains more than one data source, also add:

- `NOTION_DATA_SOURCE_ID`

The Notion integration must have read access to the board database, and the database must be shared with that integration inside Notion.

After the secrets are configured, trigger the `Deploy DocFX to GitHub Pages` workflow on `main` or via manual dispatch. The workflow reads the Notion board, regenerates `docs/kanban.md`, builds DocFX, and publishes `_site`.

The repository does not store the Notion token in source control. The workflow consumes it from GitHub Actions secrets only.
