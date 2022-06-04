CREATE TABLE [dbo].[Products] (
    [Id]                   INT             IDENTITY (1, 1) NOT NULL,
    [Admin]                NVARCHAR (MAX)  NULL,
    [Name]                 NVARCHAR (MAX)  NULL,
    [ShortName]            NVARCHAR (MAX)  NULL,
    [Description]          NVARCHAR (MAX)  NULL,
    [Details]              NVARCHAR (MAX)  NULL,
    [Specification]        NVARCHAR (MAX)  NULL,
    [Slug]                 NVARCHAR (450)  NOT NULL,
    [Active]               BIT             NOT NULL,
    [CategoryId]           INT             NULL,
    [Rating]               INT             NOT NULL,
    [OldPrice]             DECIMAL (18, 2) NOT NULL,
    [NewPrice]             DECIMAL (18, 2) NOT NULL,
    [Quantity]             DECIMAL (18, 2) NOT NULL,
    [IsOffer]              BIT             NOT NULL,
    [Offer]                DECIMAL (18, 2) NOT NULL,
    [MetaTitle]            NVARCHAR (MAX)  NULL,
    [MetaKeywords]         NVARCHAR (MAX)  NULL,
    [MetaDescription]      NVARCHAR (MAX)  NULL,
    [Image]                NVARCHAR (MAX)  NULL,
    [CountView]            INT             NOT NULL,
    [CountSale]            INT             NOT NULL,
    [Position]             INT             NOT NULL,
    [MainCharacteristics]  NVARCHAR (MAX)  NULL,
    [TechnicalDescription] NVARCHAR (MAX)  NULL,
    [General]              NVARCHAR (MAX)  NULL,
    [Garantie]             NVARCHAR (MAX)  NULL,
    [VenduWith]            NVARCHAR (MAX)  NULL,
    [InsertDate]           DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [LastUpdate]           DATETIME2 (7)   DEFAULT (getdate()) NULL,
    [SearchTerms]          NVARCHAR (500)  NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]),
    CONSTRAINT [UXC_Product_Slug] UNIQUE NONCLUSTERED ([Slug] ASC)
);






GO
CREATE NONCLUSTERED INDEX [IX_Products_SearchTerms]
    ON [dbo].[Products]([SearchTerms] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Products_CategoryId]
    ON [dbo].[Products]([CategoryId] ASC);

