CREATE TABLE [EntLib].[xtLog](
	[LogID] [INT] IDENTITY(1,1) NOT NULL,
	[EventID] [INT] NULL,
	[Priority] [INT] NOT NULL,
	[Severity] [NVARCHAR](32) NOT NULL,
	[Title] [NVARCHAR](256) NOT NULL,
	[Timestamp] [DATETIME] NOT NULL,
	[MachineName] [NVARCHAR](32) NOT NULL,
	[AssemblyName] [NVARCHAR](512) NOT NULL,
	[MethodName] [NVARCHAR](256) NOT NULL,
	[ClassName] [NVARCHAR](512) NOT NULL,
	[ThreadName] [NVARCHAR](512) NULL,
	[ExecutableName] [NVARCHAR](128) NULL,
	[Message] [NVARCHAR](1500) NULL,
	[FormattedMessage] [NVARCHAR](MAX) NULL,
	[UserName] [NVARCHAR](100) NULL,
	[ErrorID] [NVARCHAR](36) NULL
 CONSTRAINT [PK_xtLog] PRIMARY KEY CLUSTERED 
(
	[LogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]