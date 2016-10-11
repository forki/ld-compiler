#!/bin/sh
# Script to populate the app.config with environment variables
sed -i "s/LOGGING_ENVIRONMENT/$LOGGING_ENVIRONMENT/" /compiler/bin/compiler.api.exe.config

