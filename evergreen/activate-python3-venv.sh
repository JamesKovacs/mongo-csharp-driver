#!/usr/bin/env bash

# Find the version of python on the system.
# If the directory "venv" exists, start the virtual environment.
# Otherwise, install a new virtual environment.
#
# Environment variables used as input:
#   OS                                               The current operating system
#
# Environment variables produced as output:
#   PYTHON                                           The venv python path

echo "Initializing Python3 virtual environment..."

if [ -e "/cygdrive/c/python/Python311/python" ]; then
    SYSTEM_PYTHON="/cygdrive/c/python/Python311/python"
elif [ -e "/opt/mongodbtoolchain/v3/bin/python3" ]; then
    SYSTEM_PYTHON="/opt/mongodbtoolchain/v3/bin/python3"
elif python3 --version >/dev/null 2>&1; then
    SYSTEM_PYTHON=python3
else
    SYSTEM_PYTHON=python
fi

if [ ! -e venv ]; then
    $SYSTEM_PYTHON -m venv ./venv
fi

export VIRTUAL_ENV="$(pwd)/venv"

if [ "Windows_NT" = "$OS" ]; then
    export PATH="$(pwd)/venv/Scripts:${PATH}"
else
    export PYTHON="$(pwd)/venv/bin:${PATH}"
fi

unset PYTHONHOME

echo "Python3 virtual environment has been initialized"
