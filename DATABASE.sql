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
    ThumbnailLink varchar(256),
    Title nvarchar(100) NOT NULL,
    Description nvarchar(max),
    Location nvarchar(256) NOT NULL,
    InstructorName nvarchar(100) NOT NULL,
    InstructorImgLink varchar(256),
    CategoryId int NOT NULL,
    Duration int NOT NULL, -- in minutes
    Level varchar(20) NOT NULL DEFAULT 'elementary',
    Language varchar(2) NOT NULL DEFAULT 'en', -- ISO language code
    CreatedBy int NOT NULL,
    CreatedOn datetime NOT NULL DEFAULT GETDATE(),
    Status varchar(10) NOT NULL DEFAULT 'draft',

    CONSTRAINT ck_workshop_status CHECK (Status IN ('draft', 'awaiting', 'verified', 'removed', 'ended')),
    CONSTRAINT ck_workshop_level CHECK (Level IN ('elementary', 'intermediate', 'advanced')),
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
    StartOn date NOT NULL,
    CreatedFromRepeat bit NOT NULL DEFAULT 0,

    CONSTRAINT ck_workshopschedule_time CHECK (StartOn > GETDATE()),
    CONSTRAINT fk_workshopschedule_workshop FOREIGN KEY (WorkshopId) REFERENCES Workshops
)

CREATE TABLE WorkshopTickets
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    TicketType varchar(10) NOT NULL DEFAULT 'morning',
    StartTime time NOT NULL,
    EndTime time NOT NULL,
    WorkshopScheduleId int NOT NULL,
    MaxTickets int NOT NULL,
    Price decimal(18, 2) NOT NULL,

    CONSTRAINT ck_workshoptickets_price CHECK (Price > 0),
    CONSTRAINT ck_workshoptickets_maxtickets CHECK (MaxTickets > 0),
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

-- Abc@12345
INSERT INTO Users
(Email, PhoneNumber, PasswordHash, Role, Name, Verified, IsActive, GoogleUserId)
VALUES
('user1@example.com', '', 'AQAAAAIAAYagAAAAEC+EIwIzuiMSML2WhOWrh4RXtiohR0R6er4bsOUj1LKoNLrk4iRoSc6ah3qLgBcHtQ==', 'user', 'user1', 1, 1, ''),
('user2@example.com', '', 'AQAAAAIAAYagAAAAEC+EIwIzuiMSML2WhOWrh4RXtiohR0R6er4bsOUj1LKoNLrk4iRoSc6ah3qLgBcHtQ==', 'user', 'user2', 1, 1, ''),
('host1@example.com', '', 'AQAAAAIAAYagAAAAEC+EIwIzuiMSML2WhOWrh4RXtiohR0R6er4bsOUj1LKoNLrk4iRoSc6ah3qLgBcHtQ==', 'host', 'host1', 1, 1, ''),
('host2@example.com', '', 'AQAAAAIAAYagAAAAEC+EIwIzuiMSML2WhOWrh4RXtiohR0R6er4bsOUj1LKoNLrk4iRoSc6ah3qLgBcHtQ==', 'host', 'host2', 1, 1, '')

DECLARE @user1 INT, @user2 INT, @host1 INT, @host2 INT;
SELECT @user1 = Id FROM Users WHERE Email = 'user1@example.com';
SELECT @user2 = Id FROM Users WHERE Email = 'user2@example.com';
SELECT @host1 = Id FROM Users WHERE Email = 'host1@example.com';
SELECT @host2 = Id FROM Users WHERE Email = 'host2@example.com';

IF NOT EXISTS (SELECT 1 FROM WorkshopCategories WHERE Name = 'Programming')
    INSERT INTO WorkshopCategories (Name) VALUES ('Programming');

IF NOT EXISTS (SELECT 1 FROM WorkshopCategories WHERE Name = 'Art')
    INSERT INTO WorkshopCategories (Name) VALUES ('Art');

IF NOT EXISTS (SELECT 1 FROM WorkshopCategories WHERE Name = 'Photography')
    INSERT INTO WorkshopCategories (Name) VALUES ('Photography');

DECLARE @catProg INT, @catArt INT, @catPhoto INT;
SELECT @catProg = Id FROM WorkshopCategories WHERE Name = 'Programming';
SELECT @catArt = Id FROM WorkshopCategories WHERE Name = 'Art';
SELECT @catPhoto = Id FROM WorkshopCategories WHERE Name = 'Photography';

-- Insert two sample workshops (hosts are existing users). Capture ids.
DECLARE @wk1 INT, @wk2 INT;

INSERT INTO Workshops (Title, Description, Location, InstructorName, InstructorImgLink, CategoryId, Duration, Level, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Intro to Modern C# (.NET 8)',
    'A hands-on workshop covering modern C# features and building a small web API on .NET 8.',
    'Room A - Building 1',
    'host1',
    'https://via.placeholder.com/150',
    @catProg,
    120,
    'elementary',
    'en',
    @host1,
    'verified',
    'https://via.placeholder.com/300'
);
SET @wk1 = SCOPE_IDENTITY();

INSERT INTO Workshops (Title, Description, Location, InstructorName, InstructorImgLink, CategoryId, Duration, Level, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Landscape Photography Basics',
    'Learn composition, camera settings, and editing for striking landscape photos.',
    'Studio 2 - Photography Wing',
    'host2',
    'https://via.placeholder.com/150',
    @catPhoto,
    90,
    'intermediate',
    'vi',
    @host2,
    'verified',
    'https://via.placeholder.com/300'
);
SET @wk2 = SCOPE_IDENTITY();

DECLARE @sched1 INT, @sched2 INT, @sched3 INT, @sched4 INT;

INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk1, CAST(DATEADD(day, 3, GETDATE()) AS date), 0);
SET @sched1 = SCOPE_IDENTITY();

INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk1, CAST(DATEADD(day, 10, GETDATE()) AS date), 0);
SET @sched2 = SCOPE_IDENTITY();

INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk2, CAST(DATEADD(day, 5, GETDATE()) AS date), 0);
SET @sched3 = SCOPE_IDENTITY();

INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk2, CAST(DATEADD(day, 12, GETDATE()) AS date), 0);
SET @sched4 = SCOPE_IDENTITY();

DECLARE
    @t_w1_s1_morning INT, @t_w1_s1_afternoon INT, @t_w1_s1_evening INT,
    @t_w1_s2_morning INT, @t_w1_s2_evening INT,
    @t_w2_s3_morning INT, @t_w2_s3_afternoon INT,
    @t_w2_s4_evening INT;

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('morning', '09:00:00', '12:00:00', @sched1, 20, 400000);
SET @t_w1_s1_morning = SCOPE_IDENTITY();

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('afternoon', '14:00:00', '17:00:00', @sched1, 15, 600000);
SET @t_w1_s1_afternoon = SCOPE_IDENTITY();

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('evening', '18:00:00', '21:00:00', @sched1, 10, 200000);
SET @t_w1_s1_evening = SCOPE_IDENTITY();

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('morning', '09:00:00', '12:00:00', @sched2, 25, 140000);
SET @t_w1_s2_morning = SCOPE_IDENTITY();

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('evening', '18:00:00', '21:00:00', @sched2, 12, 200000);
SET @t_w1_s2_evening = SCOPE_IDENTITY();

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('morning', '09:00:00', '12:00:00', @sched3, 18, 300000);
SET @t_w2_s3_morning = SCOPE_IDENTITY();

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('afternoon', '14:00:00', '17:00:00', @sched3, 16, 150000);
SET @t_w2_s3_afternoon = SCOPE_IDENTITY();

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('evening', '18:00:00', '21:00:00', @sched4, 20, 500000);
SET @t_w2_s4_evening = SCOPE_IDENTITY();

INSERT INTO WorkshopParticipants (ParticipantId, TicketId, Status)
VALUES (@user1, @t_w1_s1_morning, 'paid');

INSERT INTO Payments (ParticipantId, TicketId, Amount, Status)
VALUES (@user1, @t_w1_s1_morning, 400000, 'success');

INSERT INTO WorkshopParticipants (ParticipantId, TicketId, Status)
VALUES (@user2, @t_w1_s1_afternoon, 'unpaid');

INSERT INTO WorkshopParticipants (ParticipantId, TicketId, Status)
VALUES (@user2, @t_w1_s2_morning, 'paid');

INSERT INTO Payments (ParticipantId, TicketId, Amount, Status)
VALUES (@user2, @t_w1_s2_morning, 140000, 'success');

INSERT INTO WorkshopParticipants (ParticipantId, TicketId, Status)
VALUES (@user1, @t_w2_s3_morning, 'paid');

INSERT INTO Payments (ParticipantId, TicketId, Amount, Status)
VALUES (@user1, @t_w2_s3_morning, 300000, 'success');

INSERT INTO WorkshopParticipants (ParticipantId, TicketId, Status)
VALUES (@user2, @t_w2_s3_afternoon, 'paid');

INSERT INTO Payments (ParticipantId, TicketId, Amount, Status)
VALUES (@user2, @t_w2_s3_afternoon, 150000, 'success');

INSERT INTO WorkshopParticipants (ParticipantId, TicketId, Status)
VALUES (@user1, @t_w2_s4_evening, 'unpaid');

GO