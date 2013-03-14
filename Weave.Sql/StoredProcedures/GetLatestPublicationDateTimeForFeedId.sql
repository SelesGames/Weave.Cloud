CREATE PROCEDURE [dbo].[GetLatestPublicationDateTimeForFeedId]
	@p1 UNIQUEIDENTIFIER
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP(1) [PublishDateTime] FROM [dbo].[NewsItem] WHERE [FeedId]=@p1 order by [PublishDateTime] desc
END
GO