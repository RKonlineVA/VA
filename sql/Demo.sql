USE [X12EDI75IngestionDb]
GO

/****** Object:  Index [PK_Edi275Imports]    Script Date: 6/30/2026 11:00:30 PM ******/
ALTER TABLE [dbo].[Edi275Imports] DROP CONSTRAINT [PK_Edi275Imports] WITH ( ONLINE = OFF )
GO
/****** Object:  Index [PK_Edi275Imports]    Script Date: 6/30/2026 11:00:47 PM ******/
ALTER TABLE [dbo].[Edi275Imports] DROP CONSTRAINT [PK_Edi275Imports] WITH ( ONLINE = OFF )
GO



Drop table __EFMigrationsHistory
Drop table Edi275PatientReports
Drop table Edi275Attachments
Drop table Edi275Imports

Drop table Edi275ParseLogs


Delete from __EFMigrationsHistory
Delete from Edi275PatientReports
Delete from Edi275Attachments
Delete from Edi275Imports
Delete from Edi275ParseLogs





select * from __EFMigrationsHistory
select * from Edi275Imports
select * from Edi275Attachments
select * from Edi275ParseLogs
select * from Edi275PatientReports