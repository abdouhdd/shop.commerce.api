CREATE TABLE [dbo].[Admins] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [InsertDate]       DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    [LastUpdate]       DATETIME2 (7)  DEFAULT (getdate()) NULL,
    [SearchTerms]      NVARCHAR (500) NULL,
    [Username]         NVARCHAR (MAX) NULL,
    [PasswordHash]     NVARCHAR (MAX) NULL,
    [SecurityStamp]    NVARCHAR (MAX) NULL,
    [Firstname]        NVARCHAR (MAX) NULL,
    [Lastname]         NVARCHAR (MAX) NULL,
    [Email]            NVARCHAR (MAX) NULL,
    [Phone]            NVARCHAR (MAX) NULL,
    [Country]          NVARCHAR (MAX) NULL,
    [City]             NVARCHAR (MAX) NULL,
    [RegistrationDate] DATETIME2 (7)  NOT NULL,
    [Role]             INT            NOT NULL,
    [CountFailLogin]   INT            NOT NULL,
    [CountDesactive]   INT            NOT NULL,
    [DateDesactive]    DATETIME2 (7)  NULL,
    [Status]           INT            NOT NULL,
    CONSTRAINT [PK_Admins] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_Admins_SearchTerms]
    ON [dbo].[Admins]([SearchTerms] ASC);

