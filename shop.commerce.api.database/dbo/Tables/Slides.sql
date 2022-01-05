CREATE TABLE [dbo].[Slides] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Title]       NVARCHAR (MAX) NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Image]       NVARCHAR (MAX) NULL,
    [Link]        NVARCHAR (MAX) NULL,
    [InsertDate]  DATETIME2 (7)  NOT NULL,
    [LastUpdate]  DATETIME2 (7)  NOT NULL,
    [Index] INT NOT NULL DEFAULT 0, 
    [Active] BIT NOT NULL DEFAULT 1, 
    [SearchTerms] NVARCHAR(MAX) NOT NULL DEFAULT '', 
    CONSTRAINT [PK_Slides] PRIMARY KEY CLUSTERED ([Id] ASC)
);



