/****** Object:  Table [dbo].[TestResults]   Script Date: 8/18/2015 16:07:00 ******/
USE [ODataValidationSuite]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE TestResults ALTER COLUMN RuleName [nvarchar](100) NOT NULL

GO
