rm *.nupkg
nuget pack .\GameDonkey.nuspec -IncludeReferencedProjects -Prop Configuration=Release
cp *.nupkg C:\Projects\Nugets\