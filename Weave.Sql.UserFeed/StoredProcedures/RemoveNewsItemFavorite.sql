CREATE PROCEDURE [dbo].[RemoveNewsItemFavorite]
	@p1 UNIQUEIDENTIFIER,       --UserId
	@p2 UNIQUEIDENTIFIER        --NewsItemId
AS

BEGIN

DELETE TOP(1) FROM [dbo].[FavoriteNewsItem] WHERE [UserId]=@p1 AND [NewsItemId]=@p2

END
GO