$Name = Read-Host "Name for Test"

if (Test-Path -Path ".\EmptyProject\GlueDynamicManager\Test\$Name")
{
	Write-Output "Test already exists"
	return;
}

New-Item -Path ".\EmptyProject\GlueDynamicManager\Test\$Name" -ItemType Directory | Out-Null
New-Item -Path ".\EmptyProject\GlueDynamicManager\Test\$Name\Base" -ItemType Directory | Out-Null

Copy-Item ".\ProjectWithCodeGen\ProjectWithCodeGen\ProjectWithCodegen.gluj" -Destination ".\EmptyProject\GlueDynamicManager\Test\$Name\Base\Glue.gluj" | Out-Null

New-Item -Path ".\EmptyProject\GlueDynamicManager\Test\$Name\Base\Entities" -ItemType Directory | Out-Null
Get-ChildItem ".\ProjectWithCodeGen\ProjectWithCodeGen\Entities\" -Filter *.glej | Copy-Item -Destination ".\EmptyProject\GlueDynamicManager\Test\$Name\Base\Entities\" | Out-Null

New-Item -Path ".\EmptyProject\GlueDynamicManager\Test\$Name\Base\Screens" -ItemType Directory | Out-Null
Get-ChildItem ".\ProjectWithCodeGen\ProjectWithCodeGen\Screens\" -Filter *.glsj | Copy-Item -Destination ".\EmptyProject\GlueDynamicManager\Test\$Name\Base\Screens\" | Out-Null

$changeNum = 1

$command = Read-Host "Press enter after making changes (or type exit to stop)"

while($command -ne "exit")
{
	$changeName = "Change$changeNum"
	New-Item -Path ".\EmptyProject\GlueDynamicManager\Test\$Name\$changeName" -ItemType Directory | Out-Null

	Copy-Item ".\ProjectWithCodeGen\ProjectWithCodeGen\ProjectWithCodegen.gluj" -Destination ".\EmptyProject\GlueDynamicManager\Test\$Name\$changeName\Glue.gluj" | Out-Null

	New-Item -Path ".\EmptyProject\GlueDynamicManager\Test\$Name\$changeName\Entities" -ItemType Directory | Out-Null
	Get-ChildItem ".\ProjectWithCodeGen\ProjectWithCodeGen\Entities\" -Filter *.glej | Copy-Item -Destination ".\EmptyProject\GlueDynamicManager\Test\$Name\$changeName\Entities\" | Out-Null

	New-Item -Path ".\EmptyProject\GlueDynamicManager\Test\$Name\$changeName\Screens" -ItemType Directory | Out-Null
	Get-ChildItem ".\ProjectWithCodeGen\ProjectWithCodeGen\Screens\" -Filter *.glsj | Copy-Item -Destination ".\EmptyProject\GlueDynamicManager\Test\$Name\$changeName\Screens\" | Out-Null
	
	$changeNum = $changeNum + 1
	$command = Read-Host "Press enter after making changes (or type exit to stop)"
}

