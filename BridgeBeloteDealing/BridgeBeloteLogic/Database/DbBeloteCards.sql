USE [BridgeBelot]
GO

/****** Object:  UserDefinedFunction [dbo].[DealingCards]    Script Date: 11/06/2019 15:34:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[DealingCards]
(	
	@dealingId int
)
RETURNS TABLE 
AS
RETURN 
(
	-- Add the SELECT statement with parameter references here
	SELECT DbBeloteCards.[Name] BridgeBelotCard
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
where DbCards.dealingid = @dealingId

)
GO

