# SharpChess

SharpChess is the backend and documentation repository for my SharpChess project. At the moment this repo mainly contains the ASP.NET Core API base, authentication flow, database setup, tests, Kubernetes files, and the DocFX documentation site.

## Purpose

I use this project to build the backend side of SharpChess in a structured way. Right now the focus is mostly on:

- user authentication
- JWT access tokens and refresh tokens
- PostgreSQL persistence with Entity Framework Core
- deployment and validation setup
- project documentation with DocFX

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- JWT authentication
- MSTest and Coverlet
- Docker
- Kubernetes
- DocFX
- GitHub Pages

## Project Structure

- `src/SharpChess.Api` - API entry point, controllers, middleware, health checks
- `src/SharpChess.Application` - application layer and auth-related logic
- `src/SharpChess.Domain` - domain models
- `src/SharpChess.Infrastructure` - database, Identity, JWT, and persistence code
- `tests` - API and domain test projects
- `SharpChess.Tests` - separate MSTest project in the repository root
- `docs` - DocFX content pages
- `k8s` - Kubernetes manifests
- `tools/SharpChess.NotionKanbanSync` - tooling for the generated Project Board documentation page
- `.github/workflows` - CI, Docker, and GitHub Pages workflows

## Running Locally

### Requirements

- .NET 10 SDK
- PostgreSQL

The development connection string is in [appsettings.Development.json](/Users/alexander/CodeProjects/SharpChess/src/SharpChess.Api/appsettings.Development.json:1). By default it expects PostgreSQL on `localhost:5432` with a database called `sharpchess`.

### Start the API

```bash
dotnet restore
dotnet run --project src/SharpChess.Api/SharpChess.Api.csproj
```

In development, the launch settings enable `Database__RunMigrationsOnStartup=true`, so migrations are applied automatically on startup. The default local URL is `http://localhost:5253`.

### Run the tests

```bash
dotnet test
```

### Build the documentation site

```bash
dotnet tool update -g docfx
docfx docfx.json
```

This generates the static site in `_site`.

## Documentation

This repository uses DocFX for the documentation site and GitHub Pages for publishing it. The documentation includes:

- architecture and project explanation pages
- generated API reference from the C# code
- a Project Board page
- a Kubernetes Pod Health page
- code coverage output

The GitHub Pages build runs through [docfx-pages.yml](/Users/alexander/CodeProjects/SharpChess/.github/workflows/docfx-pages.yml:1).

Some documentation pages depend on GitHub Actions configuration:

- `NOTION_EMBED_URL` or `NOTION_PUBLIC_BOARD_URL` for the Project Board page

The Kubernetes Pod Health page is a committed snapshot artifact. The GitHub Pages workflow uploads `docs/kubernetes-pod-health.md` as a workflow artifact and publishes that same committed file through DocFX. The Pages build does not need a self-hosted runner, live cluster access, `~/.kube/config`, or `KUBECONFIG`.

The defaults are:

- `K8S_NAMESPACE=default`
- `APP_LABEL=sharpchess-api`

You can refresh the snapshot locally, review it, and commit the updated Markdown with:

```bash
K8S_NAMESPACE=default APP_LABEL=sharpchess-api bash scripts/generate-kubernetes-pod-health.sh
```

## Current Status

This project is still in development. The base structure is there, and the authentication, database, CI, Docker, Kubernetes validation, and documentation parts are already set up. The chess-specific domain side is still limited in the current codebase.

## School Context

This repository is part of my HBO-ICT Software Engineering work. I use it to practice building a layered backend project, documenting the architecture, and working with deployment and testing setup in a way that is close to a real project.
