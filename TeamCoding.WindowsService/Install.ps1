If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))

{   
$arguments = "& '" + $myinvocation.mycommand.definition + "'"
Start-Process powershell -Verb runAs -ArgumentList $arguments
Break
}

function is64bit() {
  return ([IntPtr]::Size -eq 8)
}
function get-programfilesdir() {
  if (is64bit -eq $true) {
    (Get-Item "Env:ProgramFiles(x86)").Value
  }
  else {
    (Get-Item "Env:ProgramFiles").Value
  }
}
Try
{
$dir = "$(get-programfilesdir)\TeamCoding Sync\"
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
New-Item $dir -type directory -Force
Copy-Item "$scriptPath\*" $dir -Force
Start-Process "$($dir)TeamCoding.WindowsService.exe" -ArgumentList "\i" -Verb runAs -wait
}
Catch
{
echo $_.Exception|format-list -force
Write-Host -NoNewLine 'An error occured (press a key to exit)';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
}