# T07 evidence (Windows): the desktop app must contain no database access.
$root = Join-Path $PSScriptRoot "..\src\EquipmentBorrowing.Desktop"
Write-Host "== References in the Desktop csproj =="
Select-String -Path (Join-Path $root "*.csproj") -Pattern "ProjectReference|PackageReference" | ForEach-Object { $_.Line.Trim() }
Write-Host "`n== Forbidden tokens in Desktop sources (expect none) =="
$pattern = 'Npgsql|EntityFrameworkCore|DbContext|DbSet|Host=|Username=|Password=|\b(SELECT|INSERT|UPDATE)\b\s+.*(FROM|INTO|SET)\b|SqlConnection|EquipmentBorrowing\.(Domain|Infrastructure|Application)\b'
$hits = Get-ChildItem $root -Recurse -Include *.cs,*.axaml,*.csproj,*.json | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } | Select-String -Pattern $pattern
if ($hits) { $hits | ForEach-Object { "$($_.Path):$($_.LineNumber): $($_.Line.Trim())" }; Write-Host "FOUND forbidden references"; exit 1 }
else { Write-Host "None found: Desktop references only EquipmentBorrowing.Contracts and uses HttpClient." }
