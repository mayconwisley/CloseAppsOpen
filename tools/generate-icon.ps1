Add-Type -AssemblyName System.Drawing

$size = 32
$bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Background: dark navy
$bg = [System.Drawing.Color]::FromArgb(255, 15, 23, 42)
$g.FillRectangle((New-Object System.Drawing.SolidBrush($bg)), 0, 0, $size, $size)

# Window frame (thin white rect)
$framePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(180, 51, 65, 85), 1)
$g.DrawRectangle($framePen, 3, 3, 25, 25)

# Title bar fill
$titleBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 30, 41, 59))
$g.FillRectangle($titleBrush, 4, 4, 24, 7)

# Traffic-light dots
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 239, 68, 68))),   5, 5, 4, 4)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 245, 158, 11))), 11, 5, 4, 4)
$g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,  34,197, 94))), 17, 5, 4, 4)

# Red X in the body area
$xPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 239, 68, 68), 3)
$xPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$xPen.EndCap   = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawLine($xPen,  8, 14, 24, 26)
$g.DrawLine($xPen, 24, 14,  8, 26)

$g.Dispose(); $xPen.Dispose(); $framePen.Dispose()

# Read pixel data (BGRA, top-down)
$rect    = New-Object System.Drawing.Rectangle(0, 0, $size, $size)
$bmpData = $bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$pixels  = New-Object byte[] ($size * $size * 4)
[System.Runtime.InteropServices.Marshal]::Copy($bmpData.Scan0, $pixels, 0, $pixels.Length)
$bmp.UnlockBits($bmpData); $bmp.Dispose()

# Flip rows to bottom-up (ICO/BMP requirement)
$flipped = New-Object byte[] ($pixels.Length)
for ($row = 0; $row -lt $size; $row++) {
    [Array]::Copy($pixels, ($size - 1 - $row) * $size * 4, $flipped, $row * $size * 4, $size * 4)
}

# BITMAPINFOHEADER (40 bytes)
$bih = New-Object byte[] 40
[BitConverter]::GetBytes([int32]40        ).CopyTo($bih,  0)   # biSize
[BitConverter]::GetBytes([int32]$size     ).CopyTo($bih,  4)   # biWidth
[BitConverter]::GetBytes([int32]($size*2) ).CopyTo($bih,  8)   # biHeight (x2 for mask)
[BitConverter]::GetBytes([int16]1         ).CopyTo($bih, 12)   # biPlanes
[BitConverter]::GetBytes([int16]32        ).CopyTo($bih, 14)   # biBitCount

# AND mask: $size rows × ($size/8 rounded to DWORD) = 32 × 4 = 128 bytes (all 0 = opaque)
$andMask = New-Object byte[] ($size * 4)

$imgData = $bih + $flipped + $andMask
$imgSize = $imgData.Length  # 40 + 4096 + 128 = 4264

# ICO ICONDIR (6 bytes)
$icoHdr = [byte[]](0,0, 1,0, 1,0)   # reserved | type=1 | count=1

# ICONDIRENTRY (16 bytes), image offset = 6 + 16 = 22
$entry = New-Object byte[] 16
$entry[0] = [byte]$size; $entry[1] = [byte]$size   # width, height
[BitConverter]::GetBytes([int16]1        ).CopyTo($entry,  4)  # planes
[BitConverter]::GetBytes([int16]32       ).CopyTo($entry,  6)  # bit count
[BitConverter]::GetBytes([int32]$imgSize ).CopyTo($entry,  8)  # bytes in res
[BitConverter]::GetBytes([int32]22       ).CopyTo($entry, 12)  # image offset

$icoBytes = $icoHdr + $entry + $imgData
$out = Join-Path $PSScriptRoot "..\CloseAppsOpen\app.ico"
[System.IO.File]::WriteAllBytes($out, $icoBytes)
Write-Host "✓ Icon generated: $out ($($icoBytes.Length) bytes)"
