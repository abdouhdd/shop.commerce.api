CREATE TABLE [dbo].[OrderItems] (
    [OrderItemNumber] NVARCHAR (450)  NOT NULL,
    [OrderId]         INT             NOT NULL,
    [ProductId]       INT             NOT NULL,
    [Qty]             DECIMAL (18, 2) NOT NULL,
    [Price]           DECIMAL (18, 2) NOT NULL,
    [TotalPrice]      DECIMAL (18, 2) NOT NULL,
    [SearchTerms] NVARCHAR(MAX) NOT NULL DEFAULT '', 
    [Id] INT NOT NULL DEFAULT 0, 
    [InsertDate]    DATETIME2 (7)   NOT NULL DEFAULT getdate(),
    [LastUpdate]    DATETIME2 (7)   NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([OrderItemNumber] ASC),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId]
    ON [dbo].[OrderItems]([OrderId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_OrderItems_ProductId]
    ON [dbo].[OrderItems]([ProductId] ASC);

