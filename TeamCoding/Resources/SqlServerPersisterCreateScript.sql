/****** Object:  Table [dbo].[TeamCodingSync] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TeamCodingSync](
	[Id] [varchar](512) NOT NULL,
	[Model] [varbinary](max) NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_TeamCodingSync] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Table used to sync models in the Visual Studio Team Coding extension' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeamCodingSync'
GO


-- You will also need to ensure the Service Broker is enabled for the database, you can use the below query.
-- ALTER DATABASE CURRENT SET ENABLE_BROKER