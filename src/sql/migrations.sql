create database if not exists SocialNetwork;

use SocialNetwork;

CREATE TABLE if not exists `User` (
	`Id` BIGINT NOT NULL AUTO_INCREMENT, 
    `Username` NVARCHAR(255) NOT NULL, 
    `Email` NVARCHAR(255) NOT NULL, 
    `PasswordHash` NVARCHAR(255) NOT NULL, 
    `RegisteredAt` DATETIME NOT NULL, 
    CONSTRAINT `PK_User` PRIMARY KEY (`Id`)) ENGINE = INNODB;
    
CREATE TABLE if not exists  `UserProfile` (
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

CREATE TABLE if not exists Friendship ( 
    RequesterId BIGINT NOT NULL,
    AddresseeId BIGINT NOT NULL, 
    CreatedDateTime DATETIME NOT NULL,

    CONSTRAINT Friendship_PK PRIMARY KEY (RequesterId, AddresseeId),
    CONSTRAINT FriendshipToRequester_FK FOREIGN KEY (RequesterId) REFERENCES UserProfile (UserId),
    CONSTRAINT FriendshipToAddressee_FK FOREIGN KEY (AddresseeId) REFERENCES UserProfile (UserId)
) ENGINE = INNODB;

