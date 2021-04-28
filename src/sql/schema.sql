CREATE TABLE IF NOT EXISTS SocialNetwork;

USE SocialNetwork;

CREATE TABLE IF NOT EXISTS `User` (
	`Id` BIGINT NOT NULL AUTO_INCREMENT, 
    `Username` NVARCHAR(255) NOT NULL, 
    `Email` NVARCHAR(255) NOT NULL, 
    `PasswordHash` NVARCHAR(255) NOT NULL, 
    `RegisteredAt` DATETIME NOT NULL, 
    CONSTRAINT `PK_User` PRIMARY KEY (`Id`)) ENGINE = INNODB;
    
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

CREATE TABLE IF NOT EXISTS Friendship ( 
    `RequesterId` BIGINT NOT NULL,
    `AddresseeId` BIGINT NOT NULL, 
    `Created` DATETIME NOT NULL,
    `Status` INTEGER NOT NULL, 

    CONSTRAINT Friendship_PK PRIMARY KEY (RequesterId, AddresseeId),
    CONSTRAINT FriendshipToRequester_FK FOREIGN KEY (RequesterId) REFERENCES UserProfile (UserId),
    CONSTRAINT FriendshipToAddressee_FK FOREIGN KEY (AddresseeId) REFERENCES UserProfile (UserId)
) ENGINE = INNODB;

