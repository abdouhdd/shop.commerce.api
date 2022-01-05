CREATE TABLE [dbo].[ProductImages] (
    [ProductId]  INT            NOT NULL,
    [Filename]   NVARCHAR (450) NOT NULL,
    [Id]  NVARCHAR (450) NOT NULL,
    [IsMaster]   BIT            NOT NULL,
    [InsertDate] DATETIME2 (7)  NOT NULL,
    [LastUpdate] DATETIME2 (7)  NOT NULL,
    [Position] INT NOT NULL DEFAULT 0, 
    [SearchTerms] NVARCHAR(MAX) NOT NULL DEFAULT '', 
    CONSTRAINT [FK_ProductImages_Products] FOREIGN KEY (ProductId) REFERENCES Products (Id),
    CONSTRAINT [PK_ProductImages] PRIMARY KEY CLUSTERED ([ProductId] ASC, [Filename] ASC),
    CONSTRAINT [UXC_ProductImage_Id] UNIQUE NONCLUSTERED ([Id] ASC)
);

