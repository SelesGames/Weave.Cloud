﻿CREATE PROCEDURE [dbo].[InsertNewsItemIfNotExists]
	@p1 UNIQUEIDENTIFIER,  --Id
	@p2 UNIQUEIDENTIFIER,  --FeedId
	@p3 DATETIME,          --PublishDateTime
	@p4 NVARCHAR(MAX),     --Title
	@p5 NVARCHAR(MAX),     --Link
	@p6 NVARCHAR(MAX),     --Description
	@p7 NVARCHAR(MAX),     --PublishDateTimeString
	@p8 NVARCHAR(MAX),     --ImageUrl 
	@p9 NVARCHAR(MAX),     --VideoUri
	@p10 NVARCHAR(MAX),    --YoutubeId
	@p11 NVARCHAR(MAX),    --PodcastUri
	@p12 NVARCHAR(MAX),    --ZuneAppId
	@p13 NVARCHAR(MAX),    --OriginalRssXml
	@p14 VARBINARY(MAX)    --NewsItemBlob
AS
IF NOT EXISTS(SELECT [Id] FROM [dbo].[NewsItem] WHERE [Id]=@p1)
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [dbo].[NewsItem] (
		[Id],
		[FeedId],
		[PublishDateTime],
		[Title],
		[Link],
		[Description],
		[PublishDateTimeString],
		[ImageUrl],
		[VideoUri],
		[YoutubeId],
		[PodcastUri],
		[ZuneAppId],
		[OriginalRssXml],
		[NewsItemBlob])
 
	VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14)

	SELECT CAST(1 AS bit) AS Result
END

ELSE
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SELECT CAST(0 AS bit) AS Result
END

GO