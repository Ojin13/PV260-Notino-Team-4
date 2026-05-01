#!/bin/bash
# Double-click launcher for macOS — opens in a new Terminal.app window.
cd "$(dirname "$0")" || exit 1
exec ./Popocatepetl.CLI "$@"
