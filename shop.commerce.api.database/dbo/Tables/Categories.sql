CREATE TABLE [dbo].[Categories] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (MAX) NULL,
    [Slug]             NVARCHAR (MAX) NULL,
    [Active]           BIT            NOT NULL,
    [HasChildren]      BIT            NOT NULL,
    [Level]            INT            NOT NULL,
    [ParentId]         INT            NULL,
    [Icon]             NVARCHAR (MAX) NULL,
    [CountProducts]    INT            NOT NULL,
    [CountAllProducts] INT            NOT NULL,
    [Position]         INT            NOT NULL,
    [InsertDate]       DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    [LastUpdate]       DATETIME2 (7)  DEFAULT (getdate()) NULL,
    [SearchTerms]      NVARCHAR (500) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Categories_Categories_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Categories] ([Id])
);



GO
CREATE NONCLUSTERED INDEX [IX_Categories_SearchTerms]
    ON [dbo].[Categories]([SearchTerms] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Categories_ParentId]
    ON [dbo].[Categories]([ParentId] ASC);

