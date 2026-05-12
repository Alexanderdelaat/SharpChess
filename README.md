# SharpChess

## Documentation board sync

The documentation site now uses a live Notion embed:

`Published Notion page -> embedded in DocFX -> GitHub Pages`

DocFX does not render a native board from API data. It hosts a page that embeds the published Notion board in an iframe, so the visitor's browser loads the real Notion UI from Notion's servers.

### Required GitHub configuration

Publish the Notion board page, then configure one of these in `Settings -> Secrets and variables -> Actions`:

- Repository variable `NOTION_PUBLIC_BOARD_URL` with the published Notion page URL.
- Repository secret `NOTION_EMBED_URL` with either the published page URL or the `src` URL from Notion's embed code.

After the required configuration is in place, trigger the `Deploy DocFX to GitHub Pages` workflow on `main` or via manual dispatch. The workflow regenerates `docs/kanban.md`, builds DocFX, and publishes `_site`.

The embed URL will be visible in the public HTML output, so it should not be treated as sensitive.
