USE master
GO
IF EXISTS (SELECT *
           FROM sys.databases
           WHERE name = 'EXE-HH')
    BEGIN
        ALTER DATABASE [EXE-HH] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        DROP DATABASE [EXE-HH]
    END;
GO

CREATE DATABASE [EXE-HH]
GO

USE [EXE-HH]
GO

CREATE TABLE Users
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    Email varchar(100) NOT NULL UNIQUE,
    PhoneNumber varchar(20),
    PasswordHash varchar(256),
    Role varchar(10) NOT NULL DEFAULT 'user',
    Name nvarchar(100) NOT NULL,
    Verified bit NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GETDATE(),
    IsActive bit NOT NULL DEFAULT 0,
    GoogleUserId varchar(256),

    CONSTRAINT ck_user_role CHECK (Role IN ('user', 'host', 'staff'))
)

CREATE TABLE WorkshopCategories
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL
)

CREATE TABLE Workshops
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    Rating float NOT NULL DEFAULT 0,
    -- Reviews int NOT NULL DEFAULT 0,
    ThumbnailLink varchar(256),
    Title nvarchar(100) NOT NULL,
    Description nvarchar(max),
    Location nvarchar(256) NOT NULL,
    InstructorName nvarchar(100) NOT NULL,
    InstructorImgLink varchar(256),
    Price decimal(18, 2) NOT NULL,
    CategoryId int NOT NULL,
    CreatedBy int NOT NULL,
    CreatedOn datetime NOT NULL DEFAULT GETDATE(),
    Status varchar(10) NOT NULL DEFAULT 'draft',

    CONSTRAINT ck_workshop_price CHECK (Price > 0),
    CONSTRAINT ck_workshop_status CHECK (Status IN ('draft', 'awaiting', 'verified', 'removed', 'ended')),
    CONSTRAINT fk_workshop_category FOREIGN KEY (CategoryId) REFERENCES WorkshopCategories,
    CONSTRAINT fk_workshop_user FOREIGN KEY (CreatedBy) REFERENCES Users
)

CREATE TABLE WorkshopImages
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    WorkshopId int NOT NULL,
    ImgLink varchar(256) NOT NULL,

    CONSTRAINT fk_workshopimage_workshop FOREIGN KEY (WorkshopId) REFERENCES Workshops
)

CREATE TABLE WorkshopScheduleConfig
(
    WorkshopId int PRIMARY KEY,
    RepeatType varchar(10) NOT NULL DEFAULT 'week',
    Repeats varchar(256) NOT NULL, -- "mon,fri" / "3,8,21" / "1/1,5/6,20/12"

    CONSTRAINT ck_workshopscheduleconfig_repeattype CHECK (RepeatType IN ('week', 'month', 'year')),
    CONSTRAINT fk_workshopscheduleconfig_workshop FOREIGN KEY (WorkshopId) REFERENCES Workshops
)

CREATE TABLE WorkshopSchedules
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    WorkshopId int NOT NULL,
    StartTime datetime NOT NULL,
    EndTime datetime NOT NULL,
    CreatedFromRepeat bit NOT NULL DEFAULT 0,

    CONSTRAINT ck_workshopschedule_time CHECK (StartTime > GETDATE() AND StartTime < EndTime),
    CONSTRAINT fk_workshopschedule_workshop FOREIGN KEY (WorkshopId) REFERENCES Workshops
)

CREATE TABLE WorkshopTickets
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    TicketType varchar(10) NOT NULL DEFAULT 'normal',
    WorkshopScheduleId int NOT NULL,
    MaxTickets int NOT NULL,

    CONSTRAINT ck_workshoptickets_maxtickets CHECK (MaxTickets > 0),
    CONSTRAINT ck_workshoptickets_tickettype CHECK (TicketType IN ('normal', 'early')),
    CONSTRAINT fk_workshoptickets_workshopschedule FOREIGN KEY (WorkshopScheduleId) REFERENCES WorkshopSchedules
)

CREATE TABLE WorkshopParticipants
(
    ParticipantId int NOT NULL,
    TicketId int NOT NULL,
    Status varchar(10) NOT NULL DEFAULT 'unpaid',
    BookedOn datetime NOT NULL DEFAULT GETDATE(),

    CONSTRAINT ck_workshopparticipant_status CHECK (STATUS IN ('unpaid', 'paid', 'checked in')),
    CONSTRAINT fk_workshopparticipant_workshopticket FOREIGN KEY (TicketId) REFERENCES WorkshopTickets,
    CONSTRAINT fk_workshopparticipant_user FOREIGN KEY (ParticipantId) REFERENCES Users,
    CONSTRAINT pk_workshopparticipant PRIMARY KEY (Participantid, TicketId)
)

CREATE TABLE WorkshopLikes
(
    UserId int NOT NULL,
    WorkshopId int NOT NULL,

    CONSTRAINT pk_workshoplike PRIMARY KEY (UserId, WorkshopId),
    CONSTRAINT fk_workshoplike_user FOREIGN KEY (UserId) REFERENCES Users,
    CONSTRAINT fk_workshoplike_workshop FOREIGN KEY (WorkshopId) REFERENCES Workshops
)

CREATE TABLE Payments
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    ParticipantId int NOT NULL,
    TicketId int NOT NULL,
    Amount decimal(18, 2) NOT NULL,
    Status varchar(10) NOT NULL DEFAULT 'failed',
    CreatedOn datetime NOT NULL DEFAULT GETDATE(),

    CONSTRAINT ck_payment_status CHECK (Status IN ('failed', 'success', 'canceled')),
    CONSTRAINT ck_payment_amount CHECK (Amount > 0),
    CONSTRAINT fk_payment_workshopparticipant FOREIGN KEY (ParticipantId, TicketId) REFERENCES WorkshopParticipants
)

CREATE TABLE WorkshopReviews
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    WorkshopId int NOT NULL,
    UserId int NOT NULL,
    Title nvarchar(100) NOT NULL,
    Description nvarchar(max),
    Rating int NOT NULL,
    CreatedOn datetime NOT NULL DEFAULT GETDATE(),
    Response nvarchar(max),

    CONSTRAINT fk_workshopreview_workshop FOREIGN KEY (WorkshopId) REFERENCES Workshops,
    CONSTRAINT fk_workshopreview_user FOREIGN KEY (UserId) REFERENCES Users,
    CONSTRAINT ck_workshopreview_rating CHECK (Rating >= 0)
)