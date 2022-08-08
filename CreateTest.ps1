$Name = Read-Host "Name for Test"

if (Test-Path -Path ".\TestProjects\$Name")
{
	Write-Output "Already Exists"
	# #Clean Up
	# Remove-Item ".\TestProjects\$Name\ProjectWithCodeGen\TestFiles\*" -Recurse -Force
	
	# #Glue File
	# Copy-Item ".\ProjectWithCodeGen\ProjectWithCodeGen\ProjectWithCodegen.gluj" -Destination ".\TestProjects\$Name\ProjectWithCodeGen\TestFiles\"
	
	# #Entities
	# New-Item -Path ".\TestProjects\$Name\ProjectWithCodeGen\TestFiles\Entities" -ItemType Directory
	# Get-ChildItem ".\ProjectWithCodeGen\ProjectWithCodeGen\Entities\" -Filter *.glej | Copy-Item -Destination ".\TestProjects\$Name\ProjectWithCodeGen\TestFiles\Entities\"

	# #Screens
	# New-Item -Path ".\TestProjects\$Name\ProjectWithCodeGen\TestFiles\Screens" -ItemType Directory
	# Get-ChildItem ".\ProjectWithCodeGen\ProjectWithCodeGen\Screens\" -Filter *.glej | Copy-Item -Destination ".\TestProjects\$Name\ProjectWithCodeGen\TestFiles\Screens\"
}else{
	#Copy-Item -Path ".\ProjectWithCodeGen\*" -Destination ".\TestProjects\$Name\" -Recurse
	robocopy ".\ProjectWithCodeGen" ".\TestProjects\$Name" /E
	(Get-Content -path ".\TestProjects\$Name\ProjectWithCodegen.sln" -Raw) -Replace "\.\.\\GlueDynamicManager", "..\..\GlueDynamicManager" | Set-Content ".\TestProjects\$Name\ProjectWithCodegen.sln"
	(Get-Content -path ".\TestProjects\$Name\ProjectWithCodeGen\ProjectWithCodegen.csproj" -Raw) -Replace "\.\.\\GlueDynamicManager", "..\..\GlueDynamicManager" | Set-Content ".\TestProjects\$Name\ProjectWithCodeGen\ProjectWithCodegen.csproj"
	Write-Output "Created Test"
}

