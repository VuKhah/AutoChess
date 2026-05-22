# ============================================================
# train_ai.ps1 — Chạy training AI mà không cần mở Unity Editor
#
# Cách dùng:
#   .\train_ai.ps1              → Quick mode (30 pop x 40 gen, ~2 min)
#   .\train_ai.ps1 -Production  → Production mode (100 pop x 150 gen, ~20 min)
#
# Kết quả: Assets/Resources/AI_Library.json
# ============================================================

param(
    [switch]$Production
)

$ProjectPath = $PSScriptRoot
$LogFile     = Join-Path $ProjectPath "training_log.txt"
$Method      = if ($Production) { "AITrainingBatch.RunProduction" } else { "AITrainingBatch.RunQuick" }

# ── Tìm Unity.exe từ Unity Hub ──────────────────────────────────────────────
$HubEditorRoot = "C:\Program Files\Unity\Hub\Editor"
$UnityExe = $null

if (Test-Path $HubEditorRoot)
{
    # Lấy phiên bản mới nhất
    $Latest = Get-ChildItem $HubEditorRoot -Directory |
              Sort-Object Name -Descending |
              Select-Object -First 1

    if ($Latest)
    {
        $Candidate = Join-Path $Latest.FullName "Editor\Unity.exe"
        if (Test-Path $Candidate) { $UnityExe = $Candidate }
    }
}

if (-not $UnityExe)
{
    Write-Error "Không tìm thấy Unity.exe trong $HubEditorRoot"
    Write-Host  "Hãy sửa biến `$HubEditorRoot trong script này cho đúng với máy của bạn."
    exit 1
}

Write-Host "Unity  : $UnityExe"
Write-Host "Project: $ProjectPath"
Write-Host "Method : $Method"
Write-Host "Log    : $LogFile"
Write-Host ""
Write-Host "Bắt đầu training... (có thể mất vài phút)"
Write-Host "Theo dõi log: Get-Content '$LogFile' -Wait"
Write-Host ""

$StartTime = Get-Date

& $UnityExe `
    -batchmode `
    -quit `
    -projectPath $ProjectPath `
    -executeMethod $Method `
    -logFile $LogFile

$Duration = (Get-Date) - $StartTime
$ExitCode = $LASTEXITCODE

Write-Host ""
if ($ExitCode -eq 0)
{
    Write-Host "Training hoàn tất sau $([int]$Duration.TotalMinutes) phút $($Duration.Seconds) giây"
    $JsonPath = Join-Path $ProjectPath "Assets\Resources\AI_Library.json"
    if (Test-Path $JsonPath)
    {
        Write-Host "Output : $JsonPath"
        $Size = (Get-Item $JsonPath).Length
        Write-Host "Size   : $Size bytes"
    }
}
else
{
    Write-Error "Training thất bại (exit code $ExitCode). Xem log: $LogFile"
}
