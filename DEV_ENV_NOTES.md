# Dev Environment Notes

## Environment Context
- This environment is a sandboxed Fedora userland running under Termux on Android.
- Resource limits can be tighter than a typical desktop Linux setup.

## Assumptions
- You are in `/root/dev/CLImate`.
- The project targets `net10.0`.

## Build
This environment has limited memory, so use the GC hard limit to avoid CoreCLR OOM:

```
DOTNET_GCServer=0 DOTNET_GCHeapHardLimitPercent=30 dotnet build CLImate.App/CLImate.App.csproj
```

## Run
Use the same GC settings when running:

```
DOTNET_GCServer=0 DOTNET_GCHeapHardLimitPercent=30 dotnet run --project CLImate.App -- "London, UK"
```

## Color Output
ANSI colors are enabled by default when output is a TTY.
- Disable: `--no-color`
- Force enable: `--color`
- Honor `NO_COLOR` if set.

## Tests
Due to ARM64 architecture limitations in this environment, the test runner (`dotnet test`) may fail with architecture detection issues. However, tests can be verified to compile successfully:

```
DOTNET_GCServer=0 DOTNET_GCHeapHardLimitPercent=30 dotnet build CLImate.Tests/CLImate.Tests.csproj
```

## Troubleshooting
- If you see CoreCLR memory errors, keep the GC hard limit settings above.
- If output is missing colors, ensure output is not redirected and `NO_COLOR` is unset.
- Test execution requires a properly configured .NET SDK for ARM64 which may not be available in all environments.
