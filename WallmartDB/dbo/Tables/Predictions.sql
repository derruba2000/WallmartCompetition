CREATE TABLE [dbo].[Predictions]
(
	[UpperBound] [int] NULL,
	[ForecastSales] [int] NULL,
	[ActualSales] [int] NULL,
	[LowerBound] [int] NULL,
	[Date] [varchar](50) NULL,
	[Index] [int] NULL,
	[ProcessID] [nvarchar](36) NULL,
	[Startdate] [nvarchar](10) NULL,
	[WindowSize] [int] NULL,
	[Product] [nvarchar](250) NULL,
	[executed_at] DATETIME DEFAULT GETDATE()
)
