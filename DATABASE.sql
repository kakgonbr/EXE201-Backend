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
    AvatarLink varchar(256),
    Location varchar(256),
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

CREATE TABLE WorkshopLevels
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    Name varchar(20) NOT NULL,

    CONSTRAINT ck_workshoplevel_name CHECK (Name IN ('Elementary', 'Intermediate', 'Advanced'))
)

CREATE TABLE Workshops
(
    Id int IDENTITY(1, 1) PRIMARY KEY,
    ThumbnailLink varchar(256),
    Title nvarchar(100) NOT NULL,
    Description nvarchar(max),
    Location nvarchar(256) NOT NULL,
    CategoryId int NOT NULL,
    Duration int NOT NULL, -- in minutes
    LevelId int NOT NULL,
    Language varchar(2) NOT NULL DEFAULT 'en', -- ISO language code
    CreatedBy int NOT NULL,
    CreatedOn datetime NOT NULL DEFAULT GETDATE(),
    Status varchar(10) NOT NULL DEFAULT 'draft',

    CONSTRAINT ck_workshop_status CHECK (Status IN ('ended', 'removed', 'verified', 'draft', 'pending')),
    CONSTRAINT fk_workshop_category FOREIGN KEY (CategoryId) REFERENCES WorkshopCategories,
    CONSTRAINT fk_workshop_level FOREIGN KEY (LevelId) REFERENCES WorkshopLevels,
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
    Status varchar(10) NOT NULL DEFAULT 'paid',
    BookedOn datetime NOT NULL DEFAULT GETDATE(),

    CONSTRAINT ck_workshopparticipant_status CHECK (STATUS IN ('paid', 'checked in')),
    CONSTRAINT fk_workshopparticipant_workshopticket FOREIGN KEY (TicketId) REFERENCES WorkshopTickets,
    CONSTRAINT fk_workshopparticipant_user FOREIGN KEY (ParticipantId) REFERENCES Users,
    CONSTRAINT pk_workshopparticipant PRIMARY KEY (Participantid, TicketId)
)

CREATE TABLE WorkshopLikes
(
    UserId int NOT NULL,
    WorkshopId int NOT NULL

    CONSTRAINT pk_workshoplike PRIMARY KEY (UserId, WorkshopId),
    CONSTRAINT fk_workshoplike_user FOREIGN KEY (UserId) REFERENCES Users,
    CONSTRAINT fk_workshoplike_workshop FOREIGN KEY (WorkshopId) REFERENCES Workshops
)

CREATE TABLE Payments
(
    ParticipantId int NOT NULL,
    TicketId int NOT NULL,
    Amount decimal(18, 2) NOT NULL,
    Status varchar(10) NOT NULL DEFAULT 'failed',
    CreatedOn datetime NOT NULL DEFAULT GETDATE(),

    CONSTRAINT pk_payment PRIMARY KEY (ParticipantId, TicketId),
    CONSTRAINT ck_payment_status CHECK (Status IN ('failed', 'success', 'pending')),
    CONSTRAINT ck_payment_amount CHECK (Amount > 0)
    -- CONSTRAINT fk_payment_workshopparticipant FOREIGN KEY (ParticipantId, TicketId) REFERENCES WorkshopParticipants
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
('staff1@example.com', '', 'AQAAAAIAAYagAAAAEC+EIwIzuiMSML2WhOWrh4RXtiohR0R6er4bsOUj1LKoNLrk4iRoSc6ah3qLgBcHtQ==', 'staff', 'staff1', 1, 1, ''),
('user1@example.com', '', 'AQAAAAIAAYagAAAAEC+EIwIzuiMSML2WhOWrh4RXtiohR0R6er4bsOUj1LKoNLrk4iRoSc6ah3qLgBcHtQ==', 'user', 'user1', 1, 1, ''),
('user2@example.com', '', 'AQAAAAIAAYagAAAAEC+EIwIzuiMSML2WhOWrh4RXtiohR0R6er4bsOUj1LKoNLrk4iRoSc6ah3qLgBcHtQ==', 'user', 'user2', 1, 1, ''),
('host1@example.com', '', 'AQAAAAIAAYagAAAAEC+EIwIzuiMSML2WhOWrh4RXtiohR0R6er4bsOUj1LKoNLrk4iRoSc6ah3qLgBcHtQ==', 'host', 'host1', 1, 1, ''),
('host2@example.com', '', 'AQAAAAIAAYagAAAAEC+EIwIzuiMSML2WhOWrh4RXtiohR0R6er4bsOUj1LKoNLrk4iRoSc6ah3qLgBcHtQ==', 'host', 'host2', 1, 1, '')

DECLARE @user1 INT, @user2 INT, @host1 INT, @host2 INT;
SELECT @user1 = Id FROM Users WHERE Email = 'user1@example.com';
SELECT @user2 = Id FROM Users WHERE Email = 'user2@example.com';
SELECT @host1 = Id FROM Users WHERE Email = 'host1@example.com';
SELECT @host2 = Id FROM Users WHERE Email = 'host2@example.com';

IF NOT EXISTS (SELECT 1 FROM WorkshopLevels WHERE Name = 'Elementary')
    INSERT INTO WorkshopLevels (Name) VALUES ('Elementary');

IF NOT EXISTS (SELECT 1 FROM WorkshopLevels WHERE Name = 'Intermediate')
    INSERT INTO WorkshopLevels (Name) VALUES ('Intermediate');

IF NOT EXISTS (SELECT 1 FROM WorkshopLevels WHERE Name = 'Advanced')
    INSERT INTO WorkshopLevels (Name) VALUES ('Advanced');

IF NOT EXISTS (SELECT 1 FROM WorkshopCategories WHERE Name = 'Programming')
    INSERT INTO WorkshopCategories (Name) VALUES ('Programming');

IF NOT EXISTS (SELECT 1 FROM WorkshopCategories WHERE Name = 'Art')
    INSERT INTO WorkshopCategories (Name) VALUES ('Art');

IF NOT EXISTS (SELECT 1 FROM WorkshopCategories WHERE Name = 'Photography')
    INSERT INTO WorkshopCategories (Name) VALUES ('Photography');

DECLARE @catProg INT, @catArt INT, @catPhoto INT;
DECLARE @lvlElem INT, @lvlInter INT, @lvlAdv INT;
SELECT @catProg = Id FROM WorkshopCategories WHERE Name = 'Programming';
SELECT @catArt = Id FROM WorkshopCategories WHERE Name = 'Art';
SELECT @catPhoto = Id FROM WorkshopCategories WHERE Name = 'Photography';
SELECT @lvlElem = Id FROM WorkshopLevels WHERE Name = 'Elementary';
SELECT @lvlInter = Id FROM WorkshopLevels WHERE Name = 'Intermediate';
SELECT @lvlAdv = Id FROM WorkshopLevels WHERE Name = 'Advanced';

-- Insert two sample workshops (hosts are existing users). Capture ids.
DECLARE @wk1 INT, @wk2 INT;

INSERT INTO Workshops (Title, Description, Location, CategoryId, Duration, LevelId, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Intro to Modern C# (.NET 8)',
    'A hands-on workshop covering modern C# features and building a small web API on .NET 8.',
    'Room A - Building 1',
    @catProg,
    120,
    @lvlElem,
    'en',
    @host1,
    'verified',
    'https://via.placeholder.com/300'
);
SET @wk1 = SCOPE_IDENTITY();

INSERT INTO Workshops (Title, Description, Location, CategoryId, Duration, LevelId, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Landscape Photography Basics',
    'Learn composition, camera settings, and editing for striking landscape photos.',
    'Studio 2 - Photography Wing',
    @catPhoto,
    90,
    @lvlInter,
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


-- Additional test data to exercise WorkshopRepository.SearchAsync filters and sorts
-- Appended after existing inserts

-- Add more categories
IF NOT EXISTS (SELECT 1 FROM WorkshopCategories WHERE Name = 'Music')
    INSERT INTO WorkshopCategories (Name) VALUES ('Music');

IF NOT EXISTS (SELECT 1 FROM WorkshopCategories WHERE Name = 'Cooking')
    INSERT INTO WorkshopCategories (Name) VALUES ('Cooking');

IF NOT EXISTS (SELECT 1 FROM WorkshopCategories WHERE Name = 'Design')
    INSERT INTO WorkshopCategories (Name) VALUES ('Design');

DECLARE @catMusic INT, @catCooking INT, @catDesign INT;
SELECT @catMusic = Id FROM WorkshopCategories WHERE Name = 'Music';
SELECT @catCooking = Id FROM WorkshopCategories WHERE Name = 'Cooking';
SELECT @catDesign = Id FROM WorkshopCategories WHERE Name = 'Design';

-- Reuse existing users: @host1, @host2, @user1, @user2
-- Ensure variables are set (they were set earlier in file). If not present, select again.
DECLARE @maybe_user1 INT, @maybe_user2 INT, @maybe_host1 INT, @maybe_host2 INT;
SELECT @maybe_user1 = Id FROM Users WHERE Email = 'user1@example.com';
SELECT @maybe_user2 = Id FROM Users WHERE Email = 'user2@example.com';
SELECT @maybe_host1 = Id FROM Users WHERE Email = 'host1@example.com';
SELECT @maybe_host2 = Id FROM Users WHERE Email = 'host2@example.com';

-- Insert more workshops with varied levels, durations, languages, statuses
-- Some workshops will be "verified" with schedules+tickets (included in search)
-- Some will be "verified" but have schedules with NO tickets (excluded by initial filter)
-- Some will be "draft" or other statuses (excluded by status check)
DECLARE @wk3 INT, @wk4 INT, @wk5 INT, @wk6 INT, @wk7 INT, @wk8 INT, @wk9 INT;

-- Advanced ASP.NET Performance (Programming, advanced, long duration, multiple ticket prices)
INSERT INTO Workshops (Title, Description, Location, CategoryId, Duration, LevelId, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Advanced ASP.NET Performance',
    'Deep dive: profiling, caching, and high throughput patterns for ASP.NET on .NET 8.',
    'Room B - Building 1',
    @catProg,
    180,
    @lvlAdv,
    'en',
    @maybe_host1,
    'verified',
    'https://via.placeholder.com/300'
);
SET @wk3 = SCOPE_IDENTITY();

-- Watercolor for Beginners (Art, elementary, short duration)
INSERT INTO Workshops (Title, Description, Location, CategoryId, Duration, LevelId, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Watercolor for Beginners',
    'Hands-on introduction to watercolor techniques, brushes, and color mixing.',
    'Studio 1 - Art Wing',
    @catArt,
    90,
    @lvlElem,
    'en',
    @maybe_host2,
    'verified',
    'https://via.placeholder.com/300'
);
SET @wk4 = SCOPE_IDENTITY();

-- Guitar Basics (Music, elementary, short)
INSERT INTO Workshops (Title, Description, Location, CategoryId, Duration, LevelId, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Guitar Basics: First Chords',
    'Learn open chords, strumming patterns, and play your first song.',
    'Music Room - Building 2',
    @catMusic,
    60,
    @lvlElem,
    'en',
    @maybe_host1,
    'verified',
    'https://via.placeholder.com/300'
);
SET @wk5 = SCOPE_IDENTITY();

-- Sourdough Baking (Cooking, intermediate) - include mixed ticket prices and reviews
INSERT INTO Workshops (Title, Description, Location, CategoryId, Duration, LevelId, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Sourdough Baking: From Starter to Loaf',
    'Fermentation, starter maintenance, shaping and scoring techniques.',
    'Kitchen 3 - Culinary Center',
    @catCooking,
    240,
    @lvlInter,
    'en',
    @maybe_host2,
    'verified',
    'https://via.placeholder.com/300'
);
SET @wk6 = SCOPE_IDENTITY();

-- UX Design Crash (Design, intermediate) - will have schedules but NO tickets to test exclusion
INSERT INTO Workshops (Title, Description, Location, CategoryId, Duration, LevelId, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'UX Design Crash Course',
    'Rapid introduction into user research, wireframes and prototyping.',
    'Room C - Building 3',
    @catDesign,
    120,
    @lvlInter,
    'en',
    @maybe_host1,
    'verified',
    'https://via.placeholder.com/300'
);
SET @wk7 = SCOPE_IDENTITY();

-- Night Photography Advanced (Photography, advanced) - different language to test language field
INSERT INTO Workshops (Title, Description, Location, CategoryId, Duration, LevelId, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Night Photography: Advanced Techniques',
    'Long exposure, star trails, and light painting.',
    'Outdoor Terrace - Photography Wing',
    @catPhoto,
    150,
    @lvlAdv,
    'en',
    @maybe_host2,
    'verified',
    'https://via.placeholder.com/300'
);
SET @wk8 = SCOPE_IDENTITY();

-- Draft / awaiting workshop (should be excluded by status)
INSERT INTO Workshops (Title, Description, Location, CategoryId, Duration, LevelId, Language, CreatedBy, Status, ThumbnailLink)
VALUES (
    'Cooking: Seasonal Salads (Draft)',
    'Draft workshop not yet visible publicly.',
    'Kitchen 1 - Culinary Center',
    @catCooking,
    45,
    @lvlElem,
    'en',
    @maybe_host1,
    'draft',
    'https://via.placeholder.com/300'
);
SET @wk9 = SCOPE_IDENTITY();

-- Create schedules for the new workshops
-- Keep StartOn > GETDATE() to satisfy constraint ck_workshopschedule_time

DECLARE
    @s3_1 INT, @s3_2 INT,
    @s4_1 INT,
    @s5_1 INT,
    @s6_1 INT, @s6_2 INT,
    @s7_1 INT,
    @s8_1 INT;

-- Advanced ASP.NET Performance schedules (one near, one far) -> multiple tickets (prices differ)
INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk3, CAST(DATEADD(day, 2, GETDATE()) AS date), 0);
SET @s3_1 = SCOPE_IDENTITY();

INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk3, CAST(DATEADD(day, 40, GETDATE()) AS date), 0);
SET @s3_2 = SCOPE_IDENTITY();

-- Watercolor schedule
INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk4, CAST(DATEADD(day, 5, GETDATE()) AS date), 0);
SET @s4_1 = SCOPE_IDENTITY();

-- Guitar Basics schedule within 1 day
INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk5, CAST(DATEADD(day, 1, GETDATE()) AS date), 0);
SET @s5_1 = SCOPE_IDENTITY();

-- Sourdough Baking schedules: one soon, one later
INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk6, CAST(DATEADD(day, 7, GETDATE()) AS date), 0);
SET @s6_1 = SCOPE_IDENTITY();

INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk6, CAST(DATEADD(day, 20, GETDATE()) AS date), 0);
SET @s6_2 = SCOPE_IDENTITY();

-- UX Design Crash: schedule exists but we will not add tickets (to test exclusion)
INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk7, CAST(DATEADD(day, 3, GETDATE()) AS date), 0);
SET @s7_1 = SCOPE_IDENTITY();

-- Night Photography schedule
INSERT INTO WorkshopSchedules (WorkshopId, StartOn, CreatedFromRepeat)
VALUES (@wk8, CAST(DATEADD(day, 14, GETDATE()) AS date), 0);
SET @s8_1 = SCOPE_IDENTITY();

-- Insert tickets with varied prices and ticket types
DECLARE
    @t_w3_s1_ticket1 INT, @t_w3_s1_ticket2 INT,
    @t_w3_s2_ticket1 INT,
    @t_w4_s1_morning INT,
    @t_w5_s1_morning INT,
    @t_w6_s1_full INT, @t_w6_s2_half INT,
    @t_w8_s1_evening INT;

-- Advanced ASP.NET Performance (s3_1) tickets
INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('full', '09:00:00', '17:00:00', @s3_1, 30, 121000.00);
SET @t_w3_s1_ticket1 = SCOPE_IDENTITY();

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('student', '09:00:00', '17:00:00', @s3_1, 10, 300000.00);
SET @t_w3_s1_ticket2 = SCOPE_IDENTITY();

-- Advanced ASP.NET Performance (s3_2) single cheaper remote ticket
INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('remote', '10:00:00', '15:00:00', @s3_2, 100, 500000.00);
SET @t_w3_s2_ticket1 = SCOPE_IDENTITY();

-- Watercolor (s4_1)
INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('half', '10:00:00', '12:30:00', @s4_1, 12, 550000.00);
SET @t_w4_s1_morning = SCOPE_IDENTITY();

-- Guitar Basics (s5_1)
INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('morning', '10:00:00', '11:30:00', @s5_1, 8, 280000.00);
SET @t_w5_s1_morning = SCOPE_IDENTITY();

-- Sourdough Baking (two schedules with different price tiers)
INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('full', '09:00:00', '16:00:00', @s6_1, 15, 150000.00);
SET @t_w6_s1_full = SCOPE_IDENTITY();

INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('short', '10:00:00', '13:00:00', @s6_2, 20, 800000.00);
SET @t_w6_s2_half = SCOPE_IDENTITY();

-- Night Photography (evening)
INSERT INTO WorkshopTickets (TicketType, StartTime, EndTime, WorkshopScheduleId, MaxTickets, Price)
VALUES ('evening', '20:00:00', '23:00:00', @s8_1, 12, 123000.00);
SET @t_w8_s1_evening = SCOPE_IDENTITY();

-- NOTE: UX Design Crash (@wk7) intentionally has a schedule but NO tickets inserted so it will be excluded by search initial WHERE

-- Add workshop images for variety
INSERT INTO WorkshopImages (WorkshopId, ImgLink) VALUES (@wk3, 'https://via.placeholder.com/400');
INSERT INTO WorkshopImages (WorkshopId, ImgLink) VALUES (@wk4, 'https://via.placeholder.com/400');
INSERT INTO WorkshopImages (WorkshopId, ImgLink) VALUES (@wk5, 'https://via.placeholder.com/400');
INSERT INTO WorkshopImages (WorkshopId, ImgLink) VALUES (@wk6, 'https://via.placeholder.com/400');
INSERT INTO WorkshopImages (WorkshopId, ImgLink) VALUES (@wk8, 'https://via.placeholder.com/400');

-- Add reviews to test Rating sort (ratings are ints; average will be used)
INSERT INTO WorkshopReviews (WorkshopId, UserId, Title, Description, Rating)
VALUES (@wk3, @maybe_user1, 'Excellent deep-dive', 'Detailed and practical profiling tips.', 5);

INSERT INTO WorkshopReviews (WorkshopId, UserId, Title, Description, Rating)
VALUES (@wk3, @maybe_user2, 'Very technical', 'Great content but intense for some.', 4);

INSERT INTO WorkshopReviews (WorkshopId, UserId, Title, Description, Rating)
VALUES (@wk4, @maybe_user1, 'Lovely session', 'Perfect for beginners.', 4);

INSERT INTO WorkshopReviews (WorkshopId, UserId, Title, Description, Rating)
VALUES (@wk6, @maybe_user2, 'Amazing bread', 'Hands-on and tasty results.', 5);

-- Add likes (WorkshopLikes) to test inclusion of Users navigation if needed in UI
INSERT INTO WorkshopLikes (UserId, WorkshopId) VALUES (@maybe_user1, @wk3);
INSERT INTO WorkshopLikes (UserId, WorkshopId) VALUES (@maybe_user2, @wk6);
INSERT INTO WorkshopLikes (UserId, WorkshopId) VALUES (@maybe_user1, @wk4);

-- Add participants and payments to create more realistic dataset
INSERT INTO WorkshopParticipants (ParticipantId, TicketId, Status)
VALUES (@maybe_user1, @t_w3_s1_ticket1, 'paid');

INSERT INTO Payments (ParticipantId, TicketId, Amount, Status)
VALUES (@maybe_user1, @t_w3_s1_ticket1, 120000.00, 'success');

INSERT INTO WorkshopParticipants (ParticipantId, TicketId, Status)
VALUES (@maybe_user2, @t_w6_s1_full, 'paid');

INSERT INTO Payments (ParticipantId, TicketId, Amount, Status)
VALUES (@maybe_user2, @t_w6_s1_full, 150000.00, 'success');

GO