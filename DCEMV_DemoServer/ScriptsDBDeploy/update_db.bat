REM *************************************************************************
REM DC EMV
REM Open Source EMV
REM Copyright (C) 2018  Vicente Da Silva
REM 
REM This program is free software: you can redistribute it and/or modify
REM it under the terms of the GNU Affero General Public License as published
REM by the Free Software Foundation, either version 3 of the License, or
REM any later version.
REM 
REM This program is distributed in the hope that it will be useful,
REM but WITHOUT ANY WARRANTY; without even the implied warranty of
REM MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
REM GNU Affero General Public License for more details.
REM 
REM You should have received a copy of the GNU Affero General Public License
REM along with this program.  If not, see http://www.gnu.org/licenses/
REM *************************************************************************
rem Add this to each backend project file to enable migration tools
rem <ItemGroup>
rem     <DotNetCliToolReference Include="Microsoft.EntityFrameworkCore.Tools.DotNet">
rem       <Version>1.0.0-*</Version>
rem     </DotNetCliToolReference>
rem </ItemGroup>

rem create api identity tables migration scripts
set DB_SERVER_NAME=localhost
rem set DB_SERVER_NAME=paylooladbserver.database.windows.net
echo %DB_SERVER_NAME%

rem update the database
dotnet ef database update -c PersistedGrantDbContext
dotnet ef database update -c ConfigurationDbContext
dotnet ef database update -c IdentityUserDbContext
dotnet ef database update -c ApiDbContext
