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

### Local generation

Run the sync locally with environment variables set:

```bash
export NOTION_TOKEN="secret_..."
export NOTION_DATABASE_ID="your-notion-database-id"
bash scripts/sync-notion-kanban.sh
```

The script overwrites `docs/kanban.md`, which is included in the DocFX site and published by the existing GitHub Pages workflow.
