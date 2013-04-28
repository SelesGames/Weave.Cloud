CREATE PROCEDURE [dbo].[GetNewsItemRead]
	@p1 UNIQUEIDENTIFIER,	-- UserId
	@p2 int,				-- take
	@p3 int = 0				-- skip
AS
BEGIN

With o As 
(
	Select top(@p2 + @p3) ROW_NUMBER() Over (Order By [ReadOn] desc) As Row, [ReadOn], [NewsItemBlob] From [dbo].[ReadNewsItem] where [UserId]=@p1
)
Select * From o
Where Row Between @p3 + 1 AND @p3 + @p2
Order By Row Asc

END
GO