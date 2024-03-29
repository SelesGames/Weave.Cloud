﻿CREATE TABLE [dbo].[NewsItem]
(
-- proposed change
	[Id] INT NOT NULL PRIMARY KEY NONCLUSTERED IDENTITY,
	[NewsItemId] UNIQUEIDENTIFIER NOT NULL,
-- end proposed change
	--[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED, 
	[FeedId] UNIQUEIDENTIFIER NOT NULL,
	[PublishDateTime] DATETIME NOT NULL,
	[Title] NVARCHAR(MAX) NOT NULL, 
	[OriginalPublishDateTimeString] NVARCHAR(MAX) NOT NULL, 
	[UtcPublishDateTimeString] NVARCHAR(MAX) NOT NULL, 
	[Link] NVARCHAR(MAX) NOT NULL, 
	[ImageUrl] NVARCHAR(MAX) NULL, 
	[Description] NVARCHAR(MAX) NULL, 
	[YoutubeId] NVARCHAR(MAX) NULL, 
	[VideoUri] NVARCHAR(MAX) NULL, 
	[PodcastUri] NVARCHAR(MAX) NULL, 
	[ZuneAppId] NVARCHAR(MAX) NULL, 
	[OriginalRssXml] NVARCHAR(MAX) NULL, 
	[NewsItemBlob] VARBINARY(MAX) NOT NULL, 
	[ImageWidth] INT NULL, 
	[ImageHeight] INT NULL, 
	[BaseResizedImageUrl] VARCHAR(MAX) NULL, 
	[SupportedFormats] VARCHAR(MAX) NULL 
)

--GO
--CREATE CLUSTERED INDEX [PublishDateTimeIndex] ON [dbo].[NewsItem] ([PublishDateTime])

--Go
--CREATE INDEX [FeedIdIndex] ON [dbo].[NewsItem] ([FeedId])
GO
CREATE CLUSTERED INDEX [FeedIdIndex] on [dbo].[NewsItem] ([FeedId],[Id])
