CREATE VIEW [dbo].[vw_Train_Eval]
	AS SELECT Top (1000) * FROM dbo.RAW_Train_Eval
