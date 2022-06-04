﻿CREATE TABLE [dbo].[OrderTrackings] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [OrderId]     INT            NOT NULL,
    [Status]      INT            NOT NULL,
    [Date]        DATETIME2 (7)  NOT NULL,
    [InsertDate]  DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    [LastUpdate]  DATETIME2 (7)  DEFAULT (getdate()) NULL,
    [SearchTerms] NVARCHAR (500) NULL,
    CONSTRAINT [PK_OrderTrackings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_OrderTrackings_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_OrderTrackings_OrderId]
    ON [dbo].[OrderTrackings]([OrderId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_OrderTrackings_SearchTerms]
    ON [dbo].[OrderTrackings]([SearchTerms] ASC);

