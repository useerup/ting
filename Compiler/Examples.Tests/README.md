# Example Notebook Harness

This directory contains infrastructure for managing "notebook"-style example files that combine source code with compiler output.

## Structure

- **`Examples.Tests/`**: NUnit test project that verifies all examples compile correctly and their outputs are current
- **`Examples.Tools/`**: Command-line tool for updating example outputs after compiler changes

## File Format

Example files use `.ting.md` extension (Markdown with Ting code blocks):

```markdown
```ting
{ true, false }
```
```ting-output
Compilation succeeded.

Set [Literal (Boolean true); Literal (Boolean false)]

Scopes:
No scopes collected.
```
```

Each file contains:
- A `ting` code block with source code
- A `ting-output` block with compiler diagnostics (input, compilation status, core expression, scopes, etc.)

## Workflow

### 1. Running Tests

Tests verify that all examples are current with the compiler:

```bash
dotnet test Examples.Tests
```

Each example file gets a separate test case. If outputs are outdated, tests fail.

### 2. Updating Examples

After making compiler changes, update all examples:

```bash
dotnet run --project Examples.Tools
```

This tool:
- Discovers all `.ting.md` files in `Compiler/Compiler/examples/`
- Compiles each source block
- Updates the `ting-output` sections with fresh compiler output
- Writes files back only if changed

### 3. CI/CD Integration

- Tests run automatically on every commit to verify examples haven't regressed
- If compiler output changes, the tool must be run and results committed
- Use `--check` mode in CI to verify outputs are current without modifying files

## Example Files Location

`Compiler/Compiler/examples/*.ting.md`

## Design Rationale

**Why a "notebook" format?**
- Source and output stay together for clarity
- Examples serve as living documentation
- Easy to spot regressions in compiler behavior
- Can be included in markdown docs without duplication

**Why separate test and tool projects?**
- Tests can fail without preventing the tool from running
- Tool can be used standalone or in build pipelines
- Clear separation of concerns (verification vs. mutation)

**Why NUnit?**
- Better F# support than xUnit
- TestCaseSource works well with dynamic test discovery
