CREATE TABLE [dbo].[Orders] (
    [Id]            INT             IDENTITY (1, 1) NOT NULL,
    [OrderNumber]   NVARCHAR (MAX)  NULL,
    [FullName]      NVARCHAR (MAX)  NULL,
    [Phone]         NVARCHAR (MAX)  NULL,
    [Email]         NVARCHAR (MAX)  NULL,
    [Username]      NVARCHAR (MAX)  NULL,
    [Country]       NVARCHAR (MAX)  NULL,
    [City]          NVARCHAR (MAX)  NULL,
    [ZipCode]       NVARCHAR (MAX)  NULL,
    [Address]       NVARCHAR (MAX)  NULL,
    [TotalQty]      DECIMAL (18, 2) NOT NULL,
    [TotalAmount]   DECIMAL (18, 2) NOT NULL,
    [IsPaid]        BIT             NOT NULL,
    [Status]        INT             NOT NULL,
    [PaymentMethod] INT             NOT NULL,
    [OrdersNote]    NVARCHAR (MAX)  NULL,
    [AddressIp]     NVARCHAR (MAX)  NULL,
    [Browser]       NVARCHAR (MAX)  NULL,
    [InsertDate]    DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [LastUpdate]    DATETIME2 (7)   DEFAULT (getdate()) NULL,
    [SearchTerms]   NVARCHAR (500)  NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE NONCLUSTERED INDEX [IX_Orders_SearchTerms]
    ON [dbo].[Orders]([SearchTerms] ASC);

