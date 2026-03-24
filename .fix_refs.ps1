$files = @(
    "c:\Users\moham\source\repos\Bookify\Bookify.Domain\Contracts\Service\IServiceApprovalRepository.cs",
    "c:\Users\moham\source\repos\Bookify\Bookify.Application\Interfaces\Service\IServiceApprovalService.cs",
    "c:\Users\moham\source\repos\Bookify\Bookify.Application\DTO\Service\ServiceApprovalRequestResponse.cs",
    "c:\Users\moham\source\repos\Bookify\Bookify.Application\Services\ServiceApprovalService.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        $content = Get-Content $file
        $newContent = $content | Where-Object { $_ -notmatch "^\s*Ref:" }
        $newContent | Set-Content $file
        Write-Output "Fixed $file"
    }
}
