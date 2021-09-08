CREATE DATABASE IF NOT EXISTS SocialNetwork;

USE SocialNetwork;

CREATE TABLE IF NOT EXISTS `User` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT, 
    `Username` NVARCHAR(255) NOT NULL, 
    `Email` NVARCHAR(255) NOT NULL, 
    `PasswordHash` NVARCHAR(255) NOT NULL, 
    `RegisteredAt` DATETIME NOT NULL, 
    CONSTRAINT `PK_User` PRIMARY KEY (`Id`)) ENGINE = INNODB;

alter table User add index idx_username (Username);

    
CREATE TABLE IF NOT EXISTS  `UserProfile` (
    `UserId` BIGINT NOT NULL, 
    `FirstName` NVARCHAR(255) NOT NULL, 
    `LastName` NVARCHAR(255) NOT NULL, 
    `Age` INTEGER NOT NULL, 
    `Interests` LONGTEXT CHARACTER SET utf8 NOT NULL, 
    `City` NVARCHAR(255) NOT NULL, 
    CONSTRAINT `PK_UserProfile` PRIMARY KEY (`UserId`),
    CONSTRAINT `FK_UserProfile_UserId_User_Id` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `User` (`Id`)) ENGINE = INNODB;

alter table UserProfile add index idx_first_last_name (LastName(5), FirstName(7));


CREATE TABLE IF NOT EXISTS Friendship ( 
    `RequesterId` BIGINT NOT NULL,
    `AddresseeId` BIGINT NOT NULL, 
    `Created` DATETIME NOT NULL,
    `Status` INTEGER NOT NULL, 

    CONSTRAINT Friendship_PK PRIMARY KEY (RequesterId, AddresseeId),
    CONSTRAINT FriendshipToRequester_FK FOREIGN KEY (RequesterId) REFERENCES UserProfile (UserId),
    CONSTRAINT FriendshipToAddressee_FK FOREIGN KEY (AddresseeId) REFERENCES UserProfile (UserId)
) ENGINE = INNODB;


CREATE TABLE IF NOT EXISTS  `UserPost` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,  
    `UserId` BIGINT NOT NULL, 
    `Text` TEXT NOT NULL, 
    `Created` DATETIME NOT NULL,
    `Updated` DATETIME NOT NULL,
    CONSTRAINT `PK_UserPost` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserPost_UserId_UserProfile_Id` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `UserProfile` (`UserId`)) ENGINE = INNODB;


CREATE TABLE IF NOT EXISTS  `Chat` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `IsPersonal` BOOLEAN NOT NULL,
    CONSTRAINT `PK_Chat` PRIMARY KEY (`Id`)) ENGINE = INNODB;


CREATE TABLE IF NOT EXISTS  `ChatMember` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `ChatId` BIGINT NOT NULL,
    `UserId` BIGINT NOT NULL,
    CONSTRAINT `PK_ChatMember` PRIMARY KEY (`Id`)) ENGINE = INNODB;


CREATE TABLE IF NOT EXISTS  `ChatMessage` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `ChatId` BIGINT NOT NULL,  
    `ChatLocalId` INTEGER NOT NULL, 
    `Text` TEXT NOT NULL, 
    `Created` DATETIME NOT NULL,
    `Updated` DATETIME NOT NULL,
    `IsDeleted` BOOLEAN NOT NULL,
    CONSTRAINT `PK_Message` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ChatMessage_ChatId_Chat_Id` 
        FOREIGN KEY (`ChatId`) 
        REFERENCES `Chat` (`Id`)) ENGINE = INNODB;
