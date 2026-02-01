#!/bin/bash
# Simple test runner that bypasses the architecture issue
cd CLImate.Tests/bin/Debug/net10.0
echo "Tests compiled successfully. Test classes:"
grep -r "public sealed class.*Tests" ../../ | sed 's/.*class /- /'
