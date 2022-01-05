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
    [InsertDate]    DATETIME2 (7)   NOT NULL,
    [LastUpdate]    DATETIME2 (7)   NOT NULL,
    [AddressIp] VARCHAR(50) NULL, 
    [Browser] VARCHAR(255) NULL, 
    [SearchTerms] NVARCHAR(MAX) NOT NULL DEFAULT '', 
    --[ProcessAt]     DATETIME2 (7)   NULL,
    --[DeliveredAt]     DATETIME2 (7)   NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id] ASC)
);



