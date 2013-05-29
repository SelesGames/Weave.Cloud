CREATE TABLE [dbo].[AuthInfo]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[UserId] UNIQUEIDENTIFIER NOT NULL, 
	[UserName] VARCHAR(MAX) NULL,
	[PasswordHash] VARCHAR(MAX) NULL,
	[FacebookAuthString] VARCHAR(MAX) NULL,
	[TwitterAuthString] VARCHAR(MAX) NULL,
	[MicrosoftAuthString] VARCHAR(MAX) NULL,
	[GoogleAuthString] VARCHAR(MAX) NULL,
)

GO
CREATE UNIQUE INDEX [AuthInfoIndex] on [dbo].[AuthInfo] ([UserId])