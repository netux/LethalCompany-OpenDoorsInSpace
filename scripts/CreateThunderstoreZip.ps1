$selfDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function RelativeToSelf($p) { Join-Path -Path $selfDir -ChildPath $p }


$compress = @{
	Path = @(
		RelativeToSelf("..\manifest.json")
		RelativeToSelf("..\icon.png")
		RelativeToSelf("..\README.md")
		RelativeToSelf("..\CHANGELOG.md")
		RelativeToSelf("..\bin\Release\netstandard2.1\OpenDoorsInSpace.dll")
	)
	DestinationPath = RelativeToSelf("..\thunderstore.zip")
}

Compress-Archive -Force @compress