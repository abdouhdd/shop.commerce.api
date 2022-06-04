CREATE TABLE [dbo].[ProductImages] (
    [ProductId]   INT            NOT NULL,
    [Filename]    NVARCHAR (450) NOT NULL,
    [IsMaster]    BIT            NOT NULL,
    [Position]    INT            NOT NULL,
    [InsertDate]  DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    [LastUpdate]  DATETIME2 (7)  DEFAULT (getdate()) NULL,
    [SearchTerms] NVARCHAR (500) NULL,
    [Id]          NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_ProductImages] PRIMARY KEY CLUSTERED ([ProductId] ASC, [Filename] ASC),
    CONSTRAINT [FK_ProductImages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UXC_ProductImage_Id] UNIQUE NONCLUSTERED ([Id] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_ProductImages_SearchTerms]
    ON [dbo].[ProductImages]([SearchTerms] ASC);

