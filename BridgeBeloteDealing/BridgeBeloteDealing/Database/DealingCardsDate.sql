USE [BridgeBelot]
GO

/****** Object:  UserDefinedFunction [dbo].[DealingCardsDate]    Script Date: 13/06/2019 08:12:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[DealingCardsDate]
(	
	@dealingDate datetime
)
RETURNS TABLE 
AS
RETURN 
(
	with DealingsForDate (Id, [TimeStamp])
	as
	(
	select top 1 Id, [TimeStamp] from [dbo].[DbDealings] where convert(date, [TimeStamp]) = convert(date, @dealingDate) order by [TimeStamp] desc
	)
	-- Add the SELECT statement with parameter references here
	SELECT DealingsForDate.Id DealingId 
	, DbBeloteCards.[Name] BridgeBelotCard
	, DbSuits.[Name] Suit
	, DbSides.[Name] Side
	, DbStages.[Name] Stage
	, DbBeloteCards.SuitSortOrder
	, DbBeloteCards.NoTrumpSortOrder
from  DbCards
inner join DbSuits on DbCards.Suit = DbSuits.[Value]
inner join DbStages on DbCards.Stage = DbStages.[Value]
inner join DbSides on DbCards.Side = DbSides.[Value]
inner join DbBeloteCards on DbCards.BeloteCard = DbBeloteCards.[Value]
inner join DealingsForDate on DbCards.dealingId = DealingsForDate.Id

)
GO

