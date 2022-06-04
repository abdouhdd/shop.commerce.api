CREATE TABLE [dbo].[Slides] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Title]       NVARCHAR (MAX) NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Image]       NVARCHAR (MAX) NULL,
    [Link]        NVARCHAR (MAX) NULL,
    [Index]       INT            NOT NULL,
    [Active]      BIT            NOT NULL,
    [InsertDate]  DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    [LastUpdate]  DATETIME2 (7)  DEFAULT (getdate()) NULL,
    [SearchTerms] NVARCHAR (500) NULL,
    CONSTRAINT [PK_Slides] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE NONCLUSTERED INDEX [IX_Slides_SearchTerms]
    ON [dbo].[Slides]([SearchTerms] ASC);

