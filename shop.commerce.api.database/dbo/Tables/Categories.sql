CREATE TABLE [dbo].[Categories] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (MAX) NULL,
    [Slug]        NVARCHAR (MAX) NULL,
    [Active]      BIT            NOT NULL,
    [HasChildren] BIT            NOT NULL,
    [Level]       INT            NOT NULL,
    [ParentId]    INT            NULL,
    [Icon] VARCHAR(255) NULL, 
    [CountProducts] INT NOT NULL DEFAULT 0, 
    [CountAllProducts] INT NOT NULL DEFAULT 0, 
    [Position] INT NOT NULL DEFAULT 0, 
    [InsertDate]  DATETIME2 (7)  NOT NULL,
    [LastUpdate]  DATETIME2 (7)  NOT NULL,
    [SearchTerms] NVARCHAR(MAX) NOT NULL DEFAULT '', 
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Category_Category] FOREIGN KEY (ParentId) REFERENCES Categories ([Id])
);
