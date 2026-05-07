# GenMail Studio

GenMail Studio is a .NET 8 desktop application (WPF) for generating internal username and email candidates from local text input files.

## Purpose

GenMail Studio is intended for lawful internal data processing scenarios such as:

- Account migration planning.
- Identity normalization.
- Internal directory cleanup.
- Username policy dry-runs and testing.

## Safety and legal boundary

GenMail Studio **does not** provide or support:

- SMTP sending.
- Email verification.
- Web crawling or scraping.
- Proxy usage.
- CAPTCHA bypass.
- Bulk messaging.
- Phishing workflows.

This tool operates on local inputs and generates local output files.

## Build

Run from repository root:

```bash
dotnet restore
dotnet build GenMailStudio.sln -c Release
```

## Test

Run from repository root:

```bash
dotnet test GenMailStudio.sln -c Release
```

## Publish (Windows x64 single-file)

```bash
dotnet publish src/GenMail.Wpf/GenMail.Wpf.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Using the WPF UI

1. Start the app.
2. Click **Browse .txt** and choose a local input file with one name or username per line.
3. Set **Domain** (for example, `example.com`).
4. Select username generation **Rules**.
5. Configure **Number Settings**:
   - Number Mode
   - Placement mode
   - Number range text (for example `01-03,99`)
6. Select **Dedupe Mode** and **Alias Filter** mode.
7. Optionally adjust **Safety** values shown in UI.
8. Click **Estimate** to get a conservative output estimate.
9. Click **Start** to run generation.
10. Use **Cancel** to stop a running process.
11. On completion, review **Status**, **Progress Counters**, and use **Open Folder** for output.

## Output files

Each run creates a timestamped output folder (for example `output/yyyyMMdd_HHmmss`) containing:

- `usernames.txt`
- `emails.txt`
- `duplicate_skipped.csv`
- `quality_rejected.csv`
- `rejected_inputs.csv`
- `summary.txt`

## Safety limits

Default safety guard settings:

- `MaxOutputEmails = 1_000_000`
- `MaxNumbersPerBase = 1_000`
- `MaxInputLinesBeforeWarning = 500_000`

Runs that estimate output above limits are rejected.

## Dedupe modes

- **None**: no deduplication.
- **InMemory**: dedupe in process memory for current run.
- **Sqlite**: persistent dedupe via SQLite database in the run output folder.

## Number expansion

Supported range patterns include:

- Simple ranges: `0-9`, `00-99`, `000-999`
- Year ranges: `1900-1999`, `2000-2026`
- Padded ranges: `001-050`
- Lists: `1,2,3,10`
- Mixed: `01-03,99`

Placement modes include suffix, prefix, infix-before-last-token, and combinations.

Examples:

- `john` + `00-02` (suffix) -> `john00`, `john01`, `john02`
- `john.smith` + `99` (infix-before-last-token) -> `john99.smith`

## Troubleshooting

### Build errors on Linux for WPF

The WPF project sets `EnableWindowsTargeting=true` so restore/build can be evaluated from non-Windows environments, while runtime publish target should remain Windows.

### Invalid domain errors

Use a domain-like value (for example `example.com`), without `@`.

### No outputs generated

Check:

- Input file content and extension (`.txt`).
- Selected rules.
- Number mode/settings.
- Quality rejections in `quality_rejected.csv`.
- Duplicate skips in `duplicate_skipped.csv`.
- Safety limits in `summary.txt` and status text.
