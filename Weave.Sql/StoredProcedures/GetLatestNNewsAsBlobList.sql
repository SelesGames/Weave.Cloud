CREATE PROCEDURE [dbo].[GetLatestNNewsAsBlobList]
	@p1 UNIQUEIDENTIFIER,
	@p2 INT = 25
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP(@p2) [NewsItemBlob] FROM [dbo].[NewsItem] WHERE [FeedId]=@p1 order by [PublishDateTime] desc
END
GO