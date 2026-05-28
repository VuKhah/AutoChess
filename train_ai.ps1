# ============================================================
# train_ai.ps1 - Chạy training AI không cần mở Unity Editor
#
# Cách dùng:
#   .\train_ai.ps1              → Quick mode  (30 pop × 40 gen, ~2 phút)
#   .\train_ai.ps1 -Production  → Production  (100 pop × 150 gen, ~20 phút)
#
# Output chính : Assets/Resources/AI_Library.json
# Output báo cáo: Assets/Document/02_Data/Train/training_TIMESTAMP.csv
# Log lỗi      : TrainingLogs/error_TIMESTAMP.log  (xóa tự động nếu thành công)
# ============================================================

param(
    [switch]$Production
)

$ProjectPath = $PSScriptRoot
$Method      = if ($Production) { "AITrainingBatch.RunProduction" } else { "AITrainingBatch.RunQuick" }

# ── Kiểm tra Unity Editor đang mở ────────────────────────────────────────────
$UnityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($UnityProcs)
{
    Write-Error "Unity Editor dang mo! Hay dong Unity truoc khi chay training."
    Write-Host  "Ly do: Unity batch mode khong the chay khi project dang duoc mo trong Editor."
    Write-Host  "Cac tien trinh Unity dang chay:"
    $UnityProcs | ForEach-Object { Write-Host "  PID $($_.Id)  $($_.MainWindowTitle)" }
    exit 1
}

# ── Xóa stale lockfile (còn sót khi Unity tắt không sạch) ────────────────────
$LockFile = Join-Path $ProjectPath "Temp\UnityLockfile"
if (Test-Path $LockFile)
{
    Write-Host "Xoa stale lockfile: $LockFile"
    Remove-Item $LockFile -Force
}

# ── Tìm Unity.exe ────────────────────────────────────────────────────────────
$HubEditorRoot = "C:\Program Files\Unity\Hub\Editor"
$UnityExe      = $null

if (Test-Path $HubEditorRoot)
{
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
    Write-Error "Khong tim thay Unity.exe trong $HubEditorRoot"
    Write-Host  "Hay sua bien HubEditorRoot trong script nay."
    exit 1
}

# ── Chuẩn bị log ─────────────────────────────────────────────────────────────
$LogDir  = Join-Path $ProjectPath "TrainingLogs"
$null    = New-Item -ItemType Directory -Force -Path $LogDir
$Stamp   = Get-Date -Format "yyyyMMdd_HHmmss"
$LogFile = Join-Path $LogDir "error_$Stamp.log"

Write-Host "Unity  : $UnityExe"
Write-Host "Project: $ProjectPath"
Write-Host "Method : $Method"
Write-Host ""
Write-Host "Bat dau training... CSV se xuat vao Assets/Document/02_Data/Train/"
Write-Host ""

$StartTime = Get-Date

$proc = Start-Process `
    -FilePath    $UnityExe `
    -ArgumentList "-batchmode", "-quit", "-projectPath", "`"$ProjectPath`"", "-executeMethod", $Method, "-logFile", "`"$LogFile`"" `
    -PassThru `
    -Wait

$ExitCode = if ($proc -and $null -ne $proc.ExitCode) { $proc.ExitCode } else { -1 }
$Duration = (Get-Date) - $StartTime
$JsonPath = Join-Path $ProjectPath "Assets\Resources\AI_Library.json"

Write-Host ""

# ── Kiểm tra kết quả ─────────────────────────────────────────────────────────
# Thành công khi: exit code 0 VÀ JSON tồn tại VÀ JSON được tạo/cập nhật trong lần chạy này
$JsonValid   = (Test-Path $JsonPath) -and ((Get-Item $JsonPath).Length -gt 100)
$JsonIsNew   = $JsonValid -and ((Get-Item $JsonPath).LastWriteTime -gt $StartTime)
$Success     = ($ExitCode -eq 0) -and $JsonIsNew

if ($Success)
{
    $JsonSize = [math]::Round((Get-Item $JsonPath).Length / 1KB, 1)
    Write-Host "Training hoan tat sau $([int]$Duration.TotalMinutes) phut $($Duration.Seconds) giay"
    Write-Host "AI_Library.json : $JsonPath ($JsonSize KB)"

    # Tìm CSV được tạo ra trong lần chạy này (LastWriteTime sau StartTime)
    $CsvDir    = Join-Path $ProjectPath "Assets\Document\02_Data\Train"
    $LatestCsv = Get-ChildItem $CsvDir -Filter "training_*.csv" -ErrorAction SilentlyContinue |
                 Where-Object { $_.LastWriteTime -gt $StartTime } |
                 Sort-Object LastWriteTime -Descending |
                 Select-Object -First 1

    if ($LatestCsv)
    {
        $CsvSize = [math]::Round($LatestCsv.Length / 1KB, 1)
        Write-Host "CSV bao cao      : $($LatestCsv.FullName) ($CsvSize KB)"
    }
    else
    {
        Write-Warning "Khong tim thay CSV moi - training co the da bi skip."
    }

    # Xóa log nếu nhỏ (thành công, không cần giữ)
    if (Test-Path $LogFile)
    {
        $LogSizeMB = [math]::Round((Get-Item $LogFile).Length / 1MB, 1)
        if ($LogSizeMB -gt 50)
        {
            Write-Warning "Log file lon bat thuong: ${LogSizeMB}MB"
            Write-Host    "Log giu lai de debug: $LogFile"
        }
        else
        {
            Remove-Item $LogFile -Force -ErrorAction SilentlyContinue
        }
    }

    # In tóm tắt CSV
    if ($LatestCsv)
    {
        Write-Host ""
        Write-Host "--- Ket qua training (gen cuoi) ---"
        $Lines = Get-Content $LatestCsv.FullName
        if ($Lines.Count -ge 2)
        {
            Write-Host $Lines[0]           # header
            Write-Host $Lines[-1]          # dòng cuối
        }
    }
}
else
{
    Write-Host "Training that bai  (exit code: $ExitCode, thoi gian: $([int]$Duration.TotalMinutes)p $($Duration.Seconds)s)"
    Write-Host ""

    # Kiểm tra lỗi phổ biến từ log
    if (Test-Path $LogFile)
    {
        $ErrorLines = Select-String -Path $LogFile -Pattern "Error|Exception|Aborting|fatal" |
            Where-Object { $_.Line -notmatch "Licensing|AI\.Tracing|Curl|entitlement|com\.unity\.ai" } |
            Select-Object -Last 15

        if ($ErrorLines)
        {
            Write-Host "--- Loi tu Unity log ---"
            $ErrorLines | ForEach-Object { Write-Host "  $($_.Line.Trim())" }
        }

        $LogSizeMB = [math]::Round((Get-Item $LogFile).Length / 1MB, 1)
        Write-Host ""
        Write-Host "Log day du: $LogFile (${LogSizeMB}MB)"
    }
    else
    {
        Write-Host "Log file khong duoc tao ra - Unity co the da thoat truoc khi khoi dong."
        Write-Host "Kiem tra: Unity Editor co dang mo khong? Project path co dung khong?"
    }

    exit 1
}
