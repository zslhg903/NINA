nuget pack ../NINA.Core/NINA.Core.csproj								%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINA.Profile/NINA.Profile.csproj							%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINA.Astrometry/NINA.Astrometry.csproj					%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINA.Image/NINA.Image.csproj								%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINA.MGEN/NINA.MGEN.csproj								%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINA.Equipment/NINA.Equipment.csproj						%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINA.Platesolving/NINA.Platesolving.csproj				%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINACustomControlLibrary/NINACustomControlLibrary.csproj	%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINA.Core.WPF/NINA.WPF.Base.csproj						%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINA.Sequencer/NINA.Sequencer.csproj						%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"
nuget pack ../NINA.Plugin/NINA.Plugin.csproj							%1 %2 -Properties Configuration=SignedRelease -IncludeReferencedProjects -exclude "**\*.tt" -exclude "**\Accord.dll.config"