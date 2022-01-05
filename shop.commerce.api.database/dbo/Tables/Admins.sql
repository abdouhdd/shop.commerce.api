CREATE TABLE [dbo].[Admins] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [InsertDate]       DATETIME2 (7)  NOT NULL,
    [LastUpdate]       DATETIME2 (7)  NOT NULL,
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
    [CountFailLogin] INT NOT NULL DEFAULT 0, 
    [Status] INT NOT NULL DEFAULT 1, 
    [CountDesactive] INT NOT NULL DEFAULT 0, 
    [DateDesactive] DATETIME NULL, 
    [SearchTerms] NVARCHAR(MAX) NOT NULL DEFAULT '', 
    CONSTRAINT [PK_Admins] PRIMARY KEY CLUSTERED ([Id] ASC)
);

