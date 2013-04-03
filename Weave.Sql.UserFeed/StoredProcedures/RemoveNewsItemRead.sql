CREATE PROCEDURE [dbo].[RemoveNewsItemRead]
	@p1 UNIQUEIDENTIFIER,       --UserId
	@p2 UNIQUEIDENTIFIER        --NewsItemId
AS

BEGIN

DELETE FROM [dbo].[ReadNewsItem] WHERE [UserId]=@p1 AND [NewsItemId]=@p2

END
GO