CREATE PROCEDURE [dbo].[InsertNewsItemIfNotExists]
	@p1 UNIQUEIDENTIFIER,       --Id
	@p2 UNIQUEIDENTIFIER,       --FeedId
	@p3 DATETIME,               --PublishDateTime
	@p4 NVARCHAR(MAX),          --Title
	@p5 NVARCHAR(MAX),          --Link
	@p6 NVARCHAR(MAX),          --Description
	@p7 NVARCHAR(MAX),          --OriginalPublishDateTimeString
	@p8 NVARCHAR(MAX),          --UtcPublishDateTimeString
	@p9 NVARCHAR(MAX),          --ImageUrl 
	@p10 NVARCHAR(MAX),         --VideoUri
	@p11 NVARCHAR(MAX),         --YoutubeId
	@p12 NVARCHAR(MAX),         --PodcastUri
	@p13 NVARCHAR(MAX),         --ZuneAppId
	@p14 NVARCHAR(MAX),         --OriginalRssXml
	@p15 VARBINARY(MAX),        --NewsItemBlob
	@p16 INT = NULL,            --ImageWidth
	@p17 INT = NULL,            --ImageHeight
	@p18 VARCHAR(MAX) = NULL,   --BaseResizedImageUrl
	@p19 VARCHAR(MAX) = NULL    --SupportedFormats
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
		[OriginalPublishDateTimeString],
		[UtcPublishDateTimeString],
		[ImageUrl],
		[VideoUri],
		[YoutubeId],
		[PodcastUri],
		[ZuneAppId],
		[OriginalRssXml],
		[NewsItemBlob],
		[ImageWidth],
		[ImageHeight],
		[BaseResizedImageUrl],
		[SupportedFormats])
 
	VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19)

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