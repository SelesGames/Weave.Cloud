CREATE TABLE [dbo].[UserInfo]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[UserId] UNIQUEIDENTIFIER NOT NULL, 
	[FacebookAuthString] VARCHAR(MAX) NULL,
	[TwitterAuthString] VARCHAR(MAX) NULL,
	[MicrosoftAuthString] VARCHAR(MAX) NULL,
	[GoogleAuthString] VARCHAR(MAX) NULL
)