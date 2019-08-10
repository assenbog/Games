USE [BridgeBelot]
GO

/****** Object:  Table [dbo].[DbDealings]    Script Date: 11/06/2019 12:31:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DbDealings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
 CONSTRAINT [PK_Dealings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Dealings] UNIQUE NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DbDealings]  WITH CHECK ADD  CONSTRAINT [FK_DbDealings_DbSortOrders] FOREIGN KEY([SortOrder])
REFERENCES [dbo].[DbSortOrders] ([Value])
GO

ALTER TABLE [dbo].[DbDealings] CHECK CONSTRAINT [FK_DbDealings_DbSortOrders]
GO
