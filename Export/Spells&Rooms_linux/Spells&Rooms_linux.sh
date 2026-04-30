#!/bin/sh
printf '\033c\033]0;%s\a' Spells&Rooms
base_path="$(dirname "$(realpath "$0")")"
"$base_path/Spells&Rooms_linux" "$@"
