﻿CREATE TABLE [dbo].[ReadNewsItem]
(
	[Id] INT NOT NULL PRIMARY KEY NONCLUSTERED IDENTITY,
	[NewsItemId] UNIQUEIDENTIFIER NOT NULL,
	[UserId] UNIQUEIDENTIFIER NOT NULL,
	[PublishDateTime] DATETIME NOT NULL,
	[ReadOn] DATETIME NOT NULL,
	[SourceName] VARCHAR(MAX) NOT NULL,
	[Title] VARCHAR(MAX) NOT NULL, 
	[Link] VARCHAR(MAX) NOT NULL, 
	[NewsItemBlob] VARBINARY(MAX) NOT NULL, 
)

GO
CREATE CLUSTERED INDEX [ReadNewsItemIndex] on [dbo].[ReadNewsItem] ([UserId],[Id])