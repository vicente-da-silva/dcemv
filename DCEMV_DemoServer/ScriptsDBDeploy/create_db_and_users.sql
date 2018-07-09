/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
Use master;
GO
CREATE DATABASE DCEMVDemoServer;
GO
Use DCEMVDemoServer;
GO
CREATE LOGIN DCEMVDemoServer_User WITH PASSWORD = 'a@#dcwerRD';
GO
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'DCEMVDemoServer_User')
BEGIN
    CREATE USER [DCEMVDemoServer_User] FOR LOGIN [DCEMVDemoServer_User];
	EXEC sp_addrolemember N'db_owner', N'DCEMVDemoServer_User'
END;
GO

