USE [BridgeBelot]
GO

/****** Object:  Table [dbo].[DbCards]    Script Date: 07/07/2019 09:29:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DbCards](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DealingId] [int] NOT NULL,
	[Suit] [int] NOT NULL,
	[Side] [int] NOT NULL,
	[Stage] [int] NOT NULL,
	[BeloteCard] [int] NOT NULL,
	[SequenceNo] [int] NULL,
	[ShuffledSequenceNo] [int] NULL,
 CONSTRAINT [PK_Cards] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DbCards]  WITH CHECK ADD  CONSTRAINT [FK_Cards_Sides] FOREIGN KEY([Side])
REFERENCES [dbo].[DbSides] ([Value])
GO

ALTER TABLE [dbo].[DbCards] CHECK CONSTRAINT [FK_Cards_Sides]
GO

ALTER TABLE [dbo].[DbCards]  WITH CHECK ADD  CONSTRAINT [FK_Cards_Stages] FOREIGN KEY([Stage])
REFERENCES [dbo].[DbStages] ([Value])
GO

ALTER TABLE [dbo].[DbCards] CHECK CONSTRAINT [FK_Cards_Stages]
GO

ALTER TABLE [dbo].[DbCards]  WITH CHECK ADD  CONSTRAINT [FK_Cards_Suits] FOREIGN KEY([Suit])
REFERENCES [dbo].[DbSuits] ([Value])
GO

ALTER TABLE [dbo].[DbCards] CHECK CONSTRAINT [FK_Cards_Suits]
GO

ALTER TABLE [dbo].[DbCards]  WITH CHECK ADD  CONSTRAINT [FK_DbCards_DbBeloteCards] FOREIGN KEY([BeloteCard])
REFERENCES [dbo].[DbBeloteCards] ([Value])
GO

ALTER TABLE [dbo].[DbCards] CHECK CONSTRAINT [FK_DbCards_DbBeloteCards]
GO

ALTER TABLE [dbo].[DbCards]  WITH CHECK ADD  CONSTRAINT [FK_DbCards_DbDealings] FOREIGN KEY([DealingId])
REFERENCES [dbo].[DbDealings] ([Id])
GO

ALTER TABLE [dbo].[DbCards] CHECK CONSTRAINT [FK_DbCards_DbDealings]
GO


