﻿CREATE TABLE [dbo].[User]
(
	[Id]					BIGINT		IDENTITY(1,1)		NOT NULL,
	[Username]				NVARCHAR(4000)				NULL,
	[UserGuid]				NVARCHAR(128)				NOT NULL,
	[Email]					NVARCHAR(4000)				NULL,
	[Password]				NVARCHAR(MAX)						NOT NULL,
	[PasswordSalt]			NVARCHAR(MAX)					NULL,
	[PasswordFormat]		INT							NOT NULL,	
	[CreatedOnUtc]			DATETIME2						NOT NULL,	
	[UpdatedOnUtc]			DATETIME2						NOT NULL,	
	[DeletedOnUtc]			DATETIME2						NOT NULL,	
	[AdminComment]			NVARCHAR(MAX)					NULL,
	[Active]				BIT							NOT NULL DEFAULT 0,
	[Deleted]				BIT							NOT NULL DEFAULT 0,
	[LastIpAddress]			NVARCHAR(128)							NULL,
	[LastLoginDateUtc]		DATETIME2						NULL,	
	[LastActivityDateUtc]		DATETIME2						NULL,	
	[RowVersion]			ROWVERSION						NOT NULL, 
	PRIMARY KEY ([Id]),
)