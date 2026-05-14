#!/usr/bin/env python3
"""Generate the DocFX SonarCloud metrics page without exposing secrets."""

from __future__ import annotations

import base64
import datetime as dt
import html
import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path


SONARCLOUD_API = "https://sonarcloud.io/api"
METRIC_KEYS = [
    "bugs",
    "vulnerabilities",
    "code_smells",
    "coverage",
    "duplicated_lines_density",
    "security_hotspots",
    "ncloc",
]
METRIC_LABELS = {
    "bugs": "Bugs",
    "vulnerabilities": "Vulnerabilities",
    "code_smells": "Code Smells",
    "coverage": "Coverage",
    "duplicated_lines_density": "Duplications",
    "security_hotspots": "Security Hotspots",
    "ncloc": "Lines of Code",
}
PERCENT_METRICS = {"coverage", "duplicated_lines_density"}


def env_required(name: str) -> str:
    value = os.environ.get(name, "").strip()
    if not value:
        raise RuntimeError(f"{name} is required")

    return value


def sonar_get(path: str, token: str, params: dict[str, str]) -> dict:
    query = urllib.parse.urlencode(params)
    url = f"{SONARCLOUD_API}/{path}?{query}"
    auth = base64.b64encode(f"{token}:".encode("utf-8")).decode("ascii")
    request = urllib.request.Request(
        url,
        headers={
            "Authorization": f"Basic {auth}",
            "Accept": "application/json",
            "User-Agent": "SharpChess-DocFX-SonarCloud-Metrics",
        },
    )

    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            return json.loads(response.read().decode("utf-8"))
    except urllib.error.HTTPError as error:
        detail = error.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"SonarCloud API request failed with HTTP {error.code}: {detail}") from error


def escape_md(value: object) -> str:
    return html.escape(str(value), quote=False).replace("|", "\\|")


def format_metric(metric_key: str, value: str | None) -> str:
    if value is None or value == "":
        return "n/a"

    if metric_key in PERCENT_METRICS:
        try:
            return f"{float(value):.1f}%"
        except ValueError:
            return value

    try:
        return f"{int(float(value)):,}"
    except ValueError:
        return value


def quality_gate_label(status: str) -> str:
    normalized = status.upper()
    if normalized == "OK":
        return "Passed"
    if normalized == "ERROR":
        return "Failed"
    if normalized == "WARN":
        return "Warning"

    return normalized or "Unknown"


def metric_rows(measures: dict[str, str]) -> str:
    rows = []
    for metric_key in METRIC_KEYS:
        rows.append(
            f"| {escape_md(METRIC_LABELS[metric_key])} | {escape_md(format_metric(metric_key, measures.get(metric_key)))} |"
        )

    return "\n".join(rows)


def condition_rows(conditions: list[dict]) -> str:
    if not conditions:
        return "| - | - | - | - |"

    rows = []
    for condition in conditions:
        rows.append(
            "| "
            + " | ".join(
                [
                    escape_md(condition.get("metricKey", "-")),
                    escape_md(condition.get("status", "-")),
                    escape_md(condition.get("actualValue", "-")),
                    escape_md(condition.get("errorThreshold", "-")),
                ]
            )
            + " |"
        )

    return "\n".join(rows)


def write_page(output_path: Path, branch: str, quality_gate: dict, measures: dict[str, str]) -> None:
    generated_at = dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%d %H:%M UTC")
    dashboard_url = "https://sonarcloud.io/"
    gate_status = quality_gate_label(str(quality_gate.get("status", "")))
    branch_line = f"- Branch: `{escape_md(branch)}`" if branch else "- Branch: default branch"

    output_path.write_text(
        f"""---
title: SonarQube
---

# SonarQube

This page is generated during the DocFX GitHub Pages workflow from the SonarCloud Web API. The GitHub Actions runner uses a secret at build time, and the published site contains only static metric values.

<p><a class=\"sonar-dashboard-link\" href=\"{html.escape(dashboard_url, quote=True)}\" target=\"_blank\" rel=\"noopener noreferrer\">Open SonarCloud dashboard</a></p>

_Generated at {generated_at}._

{branch_line}
- Quality Gate: **{escape_md(gate_status)}**

## Metrics

| Metric | Value |
| --- | --- |
{metric_rows(measures)}

## Quality Gate Conditions

| Metric | Status | Actual | Threshold |
| --- | --- | --- | --- |
{condition_rows(quality_gate.get("conditions", []))}

## Why This Page Is Static

SonarCloud blocks iframe embedding through browser security controls, so the documentation site does not try to embed the dashboard directly. Instead, GitHub Actions fetches the metrics before DocFX builds the site and publishes this static page.

No SonarCloud token, API key, cookie, or secret is written to this page.
""",
        encoding="utf-8",
    )


def main() -> int:
    repo_root = Path(__file__).resolve().parents[1]
    output_path = repo_root / "docs" / "sonarqube.md"
    token = env_required("SONAR_TOKEN")
    project_key = env_required("SONAR_PROJECT_KEY")
    branch = os.environ.get("SONAR_BRANCH", "main").strip()

    common_params = {"component": project_key, "metricKeys": ",".join(METRIC_KEYS)}
    gate_params = {"projectKey": project_key}
    if branch:
        common_params["branch"] = branch
        gate_params["branch"] = branch

    measures_payload = sonar_get("measures/component", token, common_params)
    gate_payload = sonar_get("qualitygates/project_status", token, gate_params)

    measures = {
        measure["metric"]: measure.get("value", "")
        for measure in measures_payload.get("component", {}).get("measures", [])
    }
    quality_gate = gate_payload.get("projectStatus", {})

    write_page(output_path, branch, quality_gate, measures)
    print(f"Generated {output_path.relative_to(repo_root)} from SonarCloud metrics.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as error:
        print(f"Failed to generate SonarCloud docs: {error}", file=sys.stderr)
        raise SystemExit(1)
