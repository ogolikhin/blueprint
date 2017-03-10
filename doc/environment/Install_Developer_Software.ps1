Write-Progress -Activity 'Installing Software...'
Write-Progress -Activity 'Installing Software...' -Status 'Silverlight 5 SDK'
Start-Process -FilePath "\\svmfs01\SWLIB\Silverlight\Silverlight 5 SDK\silverlight_sdk.exe" -ArgumentList "/quiet /norestart" -Wait
Write-Host 'Silverlight 5 SDK is installed'
Write-Progress -Activity 'Installing Software...' -Status 'Silverlight 5 Toolkit December 2011' -PercentComplete 20
Start-Process -FilePath "\\svmfs01\SWLIB\Silverlight\Silverlight 5 Toolkit - December 2011\Silverlight_5_Toolkit_December_2011.msi" -ArgumentList "/quiet /norestart" -Wait
Write-Host 'Silverlight 5 Toolkit December 2011 is installed'
Write-Progress -Activity 'Installing Software...' -Status 'Silverlight Developer x64' -PercentComplete 40
Start-Process -FilePath "\\svmfs01\SWLIB\Silverlight\Silverlight_Developer_x64.exe" -ArgumentList "/q /doNotRequireDRMPrompt /ignorewarnings" -Wait
Write-Progress -Activity 'Installing Software...' -Status 'WCF RIA Services' -PercentComplete 60
Start-Process -FilePath "\\svmfs01\SWLIB\Silverlight\WCF RIA Services\RiaServices.msi" -ArgumentList "/quiet /norestart" -Wait
Write-Progress -Activity 'Installing Software...' -Status 'WCF RIA Services Toolkit' -PercentComplete 80
Start-Process -FilePath "\\svmfs01\SWLIB\Silverlight\WCF RIA Services Toolkit (September 2011)\RiaServicesToolkit.msi" -ArgumentList "/quiet /norestart" -Wait
Write-Progress -Activity 'Installing Software...' -Completed -Status "All done."