[CmdletBinding()]
param (
    
)

$projects = @{
    "AspNetCore" = "./AspNetCore/AspNetCore.csproj";
    "Core" = "./Core/Core.csproj";
    "EntityFrameworkCore" = "./EntityFrameworkCore/EntityFrameworkCore.csproj";
    "EntityFrameworkCore.Sqlite" = "./EntityFrameworkCore.Sqlite/EntityFrameworkCore.Sqlite.csproj";
}

dotnet build -c Release 

foreach ($name in $projects) {
    <# $name is the current item #>

    dotnet pack "$($projects[$name])" --no-build -o "./publish/packages/"
}
