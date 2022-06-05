CREATE TABLE [dbo].[OrderItems] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [OrderItemNumber] NVARCHAR (MAX)  NULL,
    [OrderId]         INT             NOT NULL,
    [ProductId]       INT             NOT NULL,
    [Qty]             DECIMAL (18, 2) NOT NULL,
    [Price]           DECIMAL (18, 2) NOT NULL,
    [TotalPrice]      DECIMAL (18, 2) NOT NULL,
    [InsertDate]      DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [LastUpdate]      DATETIME2 (7)   DEFAULT (getdate()) NULL,
    [SearchTerms]     NVARCHAR (500)  NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId]
    ON [dbo].[OrderItems]([OrderId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_OrderItems_ProductId]
    ON [dbo].[OrderItems]([ProductId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_OrderItems_SearchTerms]
    ON [dbo].[OrderItems]([SearchTerms] ASC);

