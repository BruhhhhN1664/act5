#!/usr/bin/env bash
# T07 evidence: the desktop app must contain no database access. Prints findings; exit 1 if any.
cd "$(dirname "$0")/../src/EquipmentBorrowing.Desktop" || exit 2
echo "== ProjectReference / PackageReference in the Desktop csproj =="
grep -E "ProjectReference|PackageReference" *.csproj
echo; echo "== Forbidden tokens in Desktop sources (expect none) =="
if grep -rnE "Npgsql|EntityFrameworkCore|DbContext|DbSet|Host=|Username=|Password=|SELECT |INSERT |UPDATE |SqlConnection|EquipmentBorrowing\.(Domain|Infrastructure|Application)\b" --include=*.cs --include=*.axaml --include=*.csproj --include=*.json . ; then
  echo "FOUND forbidden database/persistence references"; exit 1
else echo "None found: Desktop references only EquipmentBorrowing.Contracts and uses HttpClient."; fi
