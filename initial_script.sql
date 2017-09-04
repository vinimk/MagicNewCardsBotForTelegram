--
--  `Chat` table
--

CREATE TABLE IF NOT EXISTS `Chat` (
  `ChatId` bigint(20) NOT NULL,
  `FirstName` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Title` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Type` varchar(300) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Place to save all chats that can be ';

-- --------------------------------------------------------

--
--  `SpoilItem` table
--

CREATE TABLE IF NOT EXISTS `SpoilItem` (
  `SpoilItemId` int(11) NOT NULL,
  `Status` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Folder` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Date` datetime DEFAULT NULL,
  `Message` varchar(1000) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CardUrl` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Table where all the new spoils cards are saved';

--
--
-- `Chat`
--
ALTER TABLE `Chat`
  ADD PRIMARY KEY (`ChatId`);

--
--  `SpoilItem`
--
ALTER TABLE `SpoilItem`
  ADD PRIMARY KEY (`SpoilItemId`);

--
-- AUTO_INCREMENT `SpoilItem`
--
ALTER TABLE `SpoilItem`
  MODIFY `SpoilItemId` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=45;
