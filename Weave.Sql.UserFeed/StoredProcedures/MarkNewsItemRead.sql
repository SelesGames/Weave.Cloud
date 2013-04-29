CREATE PROCEDURE [dbo].[MarkNewsItemRead]
	@p1 UNIQUEIDENTIFIER,       --NewsItemId
	@p2 UNIQUEIDENTIFIER,       --UserId
	@p3 DATETIME,               --PublishDateTime
	@p4 VARCHAR(MAX),           --SourceName
	@p5 VARCHAR(MAX),           --Title
	@p6 VARCHAR(MAX),           --Link
	@p7 VARBINARY(MAX)          --NewsItemBlob
AS
IF NOT EXISTS(SELECT [Id] FROM [dbo].[ReadNewsItem] WHERE [NewsItemId]=@p1 AND [UserId]=@p2)
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [dbo].[ReadNewsItem] (
		[NewsItemId],
		[UserId],
		[PublishDateTime],
		[ReadOn],
		[SourceName],
		[Title],
		[Link],
		[NewsItemBlob])
 
	VALUES (@p1,@p2,@p3,GETUTCDATE(),@p4,@p5,@p6,@p7)

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