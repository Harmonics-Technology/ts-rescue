START TRANSACTION;

ALTER TABLE `employeeInformation` ADD `isEligibleForLeave` tinyint(1) NULL;

ALTER TABLE `employeeInformation` ADD `numberOfDaysEligible` int NULL;

ALTER TABLE `employeeInformation` ADD `numberOfHoursEligible` int NULL;

ALTER TABLE `employeeInformation` ADD `timeSheetGenerationStartDate` datetime(6) NULL;

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230405122932_twenty-nine', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `employeeInformation` DROP COLUMN `timeSheetGenerationStartDate`;

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230405141102_thirty', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `employeeInformation` ADD `timeSheetGenerationStartDate` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00';

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230405141206_thirty-one', '6.0.3');

COMMIT;

START TRANSACTION;

CREATE TABLE `leaveTypes` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `name` longtext CHARACTER SET utf8mb4 NULL,
    `leaveTypeIcon` longtext CHARACTER SET utf8mb4 NULL,
    `dateCreated` datetime(6) NOT NULL,
    `dateModified` datetime(6) NOT NULL,
    CONSTRAINT `pK_leaveTypes` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `leaves` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `employeeInformationId` char(36) COLLATE ascii_general_ci NOT NULL,
    `leaveTypeId` char(36) COLLATE ascii_general_ci NOT NULL,
    `startDate` datetime(6) NULL,
    `endDate` datetime(6) NULL,
    `reasonForLeave` longtext CHARACTER SET utf8mb4 NULL,
    `workAssigneeId` char(36) COLLATE ascii_general_ci NULL,
    `statusId` int NOT NULL,
    `dateCreated` datetime(6) NOT NULL,
    `dateModified` datetime(6) NOT NULL,
    CONSTRAINT `pK_leaves` PRIMARY KEY (`id`),
    CONSTRAINT `fK_leaves_employeeInformation_employeeInformationId` FOREIGN KEY (`employeeInformationId`) REFERENCES `employeeInformation` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fK_leaves_leaveTypes_leaveTypeId` FOREIGN KEY (`leaveTypeId`) REFERENCES `leaveTypes` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fK_leaves_statuses_statusId` FOREIGN KEY (`statusId`) REFERENCES `statuses` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fK_leaves_users_workAssigneeId` FOREIGN KEY (`workAssigneeId`) REFERENCES `Users` (`id`)
) CHARACTER SET=utf8mb4;

CREATE INDEX `iX_leaves_employeeInformationId` ON `leaves` (`employeeInformationId`);

CREATE INDEX `iX_leaves_leaveTypeId` ON `leaves` (`leaveTypeId`);

CREATE INDEX `iX_leaves_statusId` ON `leaves` (`statusId`);

CREATE INDEX `iX_leaves_workAssigneeId` ON `leaves` (`workAssigneeId`);

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230411231105_thirty-two', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `timeSheets` ADD `onLeave` tinyint(1) NOT NULL DEFAULT FALSE;

ALTER TABLE `timeSheets` ADD `onLeaveAndEligibleForLeave` tinyint(1) NOT NULL DEFAULT FALSE;

ALTER TABLE `leaves` MODIFY COLUMN `startDate` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE `leaves` MODIFY COLUMN `endDate` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE `employeeInformation` ADD `numberOfEligibleLeaveDaysTaken` int NOT NULL DEFAULT 0;

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230413001750_thirty-three', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `employeeInformation` ADD `employeeType` longtext CHARACTER SET utf8mb4 NULL;

CREATE TABLE `shifts` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `userId` char(36) COLLATE ascii_general_ci NOT NULL,
    `start` datetime(6) NOT NULL,
    `end` datetime(6) NOT NULL,
    `hours` int NOT NULL,
    `title` longtext CHARACTER SET utf8mb4 NULL,
    `color` longtext CHARACTER SET utf8mb4 NULL,
    `repeatQuery` longtext CHARACTER SET utf8mb4 NULL,
    `note` longtext CHARACTER SET utf8mb4 NULL,
    `isPublished` tinyint(1) NOT NULL,
    `isSwapped` tinyint(1) NOT NULL,
    `swapStatusId` int NULL,
    `dateCreated` datetime(6) NOT NULL,
    `dateModified` datetime(6) NOT NULL,
    CONSTRAINT `pK_shifts` PRIMARY KEY (`id`),
    CONSTRAINT `fK_shifts_users_userId` FOREIGN KEY (`userId`) REFERENCES `Users` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `iX_shifts_userId` ON `shifts` (`userId`);

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230416203535_thirty-four', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `shifts` ADD `shiftToSwapId` char(36) COLLATE ascii_general_ci NULL;

CREATE INDEX `iX_shifts_shiftToSwapId` ON `shifts` (`shiftToSwapId`);

CREATE INDEX `iX_shifts_swapStatusId` ON `shifts` (`swapStatusId`);

ALTER TABLE `shifts` ADD CONSTRAINT `fK_shifts_shifts_shiftToSwapId` FOREIGN KEY (`shiftToSwapId`) REFERENCES `shifts` (`id`);

ALTER TABLE `shifts` ADD CONSTRAINT `fK_shifts_statuses_swapStatusId` FOREIGN KEY (`swapStatusId`) REFERENCES `statuses` (`id`);

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230418121820_thirty-five', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `shifts` ADD `shiftSwappedId` char(36) COLLATE ascii_general_ci NULL;

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230420094856_thirty-six', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `shifts` DROP FOREIGN KEY `fK_shifts_shifts_shiftToSwapId`;

ALTER TABLE `shifts` DROP FOREIGN KEY `fK_shifts_statuses_swapStatusId`;

ALTER TABLE `shifts` DROP INDEX `iX_shifts_shiftToSwapId`;

ALTER TABLE `shifts` DROP INDEX `iX_shifts_swapStatusId`;

ALTER TABLE `shifts` DROP COLUMN `isSwapped`;

ALTER TABLE `shifts` DROP COLUMN `shiftSwappedId`;

ALTER TABLE `shifts` DROP COLUMN `swapStatusId`;

ALTER TABLE `shifts` RENAME COLUMN `shiftToSwapId` TO `swapId`;

CREATE TABLE `swaps` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `swapperId` char(36) COLLATE ascii_general_ci NOT NULL,
    `swapeeId` char(36) COLLATE ascii_general_ci NOT NULL,
    `shiftId` char(36) COLLATE ascii_general_ci NOT NULL,
    `shiftId1` char(36) COLLATE ascii_general_ci NULL,
    `shiftToSwapId` char(36) COLLATE ascii_general_ci NOT NULL,
    `shiftToSwapId1` char(36) COLLATE ascii_general_ci NULL,
    `statusId` int NOT NULL,
    `isApproved` tinyint(1) NOT NULL,
    `dateCreated` datetime(6) NOT NULL,
    `dateModified` datetime(6) NOT NULL,
    CONSTRAINT `pK_swaps` PRIMARY KEY (`id`),
    CONSTRAINT `fK_swaps_shifts_shiftId1` FOREIGN KEY (`shiftId1`) REFERENCES `shifts` (`id`),
    CONSTRAINT `fK_swaps_shifts_shiftToSwapId1` FOREIGN KEY (`shiftToSwapId1`) REFERENCES `shifts` (`id`),
    CONSTRAINT `fK_swaps_statuses_statusId` FOREIGN KEY (`statusId`) REFERENCES `statuses` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fK_swaps_users_swapeeId` FOREIGN KEY (`swapeeId`) REFERENCES `Users` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fK_swaps_users_swapperId` FOREIGN KEY (`swapperId`) REFERENCES `Users` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `iX_swaps_shiftId1` ON `swaps` (`shiftId1`);

CREATE INDEX `iX_swaps_shiftToSwapId1` ON `swaps` (`shiftToSwapId1`);

CREATE INDEX `iX_swaps_statusId` ON `swaps` (`statusId`);

CREATE INDEX `iX_swaps_swapeeId` ON `swaps` (`swapeeId`);

CREATE INDEX `iX_swaps_swapperId` ON `swaps` (`swapperId`);

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230420143628_thirty-seven', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `swaps` DROP FOREIGN KEY `fK_swaps_shifts_shiftId1`;

ALTER TABLE `swaps` DROP FOREIGN KEY `fK_swaps_shifts_shiftToSwapId1`;

ALTER TABLE `swaps` DROP FOREIGN KEY `fK_swaps_users_swapeeId`;

ALTER TABLE `swaps` DROP FOREIGN KEY `fK_swaps_users_swapperId`;

ALTER TABLE `swaps` DROP INDEX `iX_swaps_shiftId1`;

ALTER TABLE `swaps` DROP INDEX `iX_swaps_shiftToSwapId1`;

ALTER TABLE `swaps` DROP INDEX `iX_swaps_swapeeId`;

ALTER TABLE `swaps` DROP INDEX `iX_swaps_swapperId`;

ALTER TABLE `swaps` DROP COLUMN `shiftId1`;

ALTER TABLE `swaps` DROP COLUMN `shiftToSwapId1`;

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230420172339_thirty-nine', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `swaps` ADD `shiftId1` char(36) COLLATE ascii_general_ci NULL;

ALTER TABLE `swaps` ADD `shiftToSwapId1` char(36) COLLATE ascii_general_ci NULL;

CREATE INDEX `iX_swaps_shiftId1` ON `swaps` (`shiftId1`);

CREATE INDEX `iX_swaps_shiftToSwapId1` ON `swaps` (`shiftToSwapId1`);

CREATE INDEX `iX_swaps_swapeeId` ON `swaps` (`swapeeId`);

CREATE INDEX `iX_swaps_swapperId` ON `swaps` (`swapperId`);

CREATE INDEX `iX_shifts_swapId` ON `shifts` (`swapId`);

ALTER TABLE `shifts` ADD CONSTRAINT `fK_shifts_swaps_swapId1` FOREIGN KEY (`swapId`) REFERENCES `swaps` (`id`);

ALTER TABLE `swaps` ADD CONSTRAINT `fK_swaps_shifts_shiftId1` FOREIGN KEY (`shiftId1`) REFERENCES `shifts` (`id`);

ALTER TABLE `swaps` ADD CONSTRAINT `fK_swaps_shifts_shiftToSwapId1` FOREIGN KEY (`shiftToSwapId1`) REFERENCES `shifts` (`id`);

ALTER TABLE `swaps` ADD CONSTRAINT `fK_swaps_users_swapeeId` FOREIGN KEY (`swapeeId`) REFERENCES `Users` (`id`) ON DELETE CASCADE;

ALTER TABLE `swaps` ADD CONSTRAINT `fK_swaps_users_swapperId` FOREIGN KEY (`swapperId`) REFERENCES `Users` (`id`) ON DELETE CASCADE;

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230420173423_fourty', '6.0.3');

COMMIT;

START TRANSACTION;

ALTER TABLE `shifts` DROP FOREIGN KEY `fK_shifts_swaps_swapId1`;

ALTER TABLE `swaps` DROP FOREIGN KEY `fK_swaps_shifts_shiftId1`;

ALTER TABLE `swaps` DROP FOREIGN KEY `fK_swaps_shifts_shiftToSwapId1`;

ALTER TABLE `swaps` DROP INDEX `iX_swaps_shiftId1`;

ALTER TABLE `swaps` DROP INDEX `iX_swaps_shiftToSwapId1`;

ALTER TABLE `shifts` DROP INDEX `iX_shifts_swapId`;

ALTER TABLE `swaps` DROP COLUMN `shiftId1`;

ALTER TABLE `swaps` DROP COLUMN `shiftToSwapId1`;

ALTER TABLE `shifts` DROP COLUMN `swapId`;

CREATE INDEX `iX_swaps_shiftId` ON `swaps` (`shiftId`);

CREATE INDEX `iX_swaps_shiftToSwapId` ON `swaps` (`shiftToSwapId`);

ALTER TABLE `swaps` ADD CONSTRAINT `fK_swaps_shifts_shiftId` FOREIGN KEY (`shiftId`) REFERENCES `shifts` (`id`) ON DELETE CASCADE;

ALTER TABLE `swaps` ADD CONSTRAINT `fK_swaps_shifts_shiftToSwapId` FOREIGN KEY (`shiftToSwapId`) REFERENCES `shifts` (`id`) ON DELETE CASCADE;

INSERT INTO `__EFMigrationsHistory` (`migrationId`, `productVersion`)
VALUES ('20230420174154_fourty-one', '6.0.3');

COMMIT;

