﻿// This file is part of Mystery Dungeon eXtended.

// Copyright (C) 2015 Pikablu, MDX Contributors, PMU Staff

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.

// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.


namespace DataManager.Players
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using PMDCP.Core;
    using PMDCP.DatabaseConnector;
    using PMDCP.DatabaseConnector.MySql;

    public class PlayerDataManager
    {
        #region Methods

        public static void ChangePassword(IDatabase database, string accountName, string currentPassword, string newPassword) {
            database.UpdateRow("accounts", new IGenericDataColumn[] {
                database.CreateColumn(false, "Password")
            }, "accounts.AccountName = @AccountName AND accounts.Password = @CurrentPassword", new { AccountName = accountName, Password = newPassword, CurrentPassword = currentPassword });
        }

        public static void ChangePlayerEmail(PMDCP.DatabaseConnector.MySql.MySql database, string accountName, string currentEmail, string newEmail) {
            database.UpdateRow("accounts", new IGenericDataColumn[] {
                database.CreateColumn(false, "Email")
            }, "accounts.AccountName = @AccountName AND accounts.Email = @CurrentEmail", new { AccountName = accountName, CurrentEmail = currentEmail, Email = newEmail });
        }

        public static void CreateNewAccount(PMDCP.DatabaseConnector.MySql.MySql database, string accountName, string encryptedPassword, string email) {
            database.AddRow("accounts", new IDataColumn[] {
                database.CreateColumn(false, "AccountName", accountName),
                database.CreateColumn(false, "Password", encryptedPassword),
                database.CreateColumn(false, "Email", email)
            });
        }

        public static void CreateNewCharacter(PMDCP.DatabaseConnector.MySql.MySql database, string accountName, int charNum, PlayerData playerData) {
            // Add the character to the 'characters' table
            database.AddRow("characters", new IDataColumn[] {
                database.CreateColumn(false, "AccountName", accountName),
                database.CreateColumn(false, "Slot", charNum.ToString()),
                database.CreateColumn(false, "CharID", playerData.CharID)
            });

            SavePlayerCharacteristics(database, playerData);
            // SavePlayerExpKit(database, playerData);
            SavePlayerLocation(database, playerData);
            SavePlayerTeam(database, playerData);
            SavePlayerItemGenerals(database, playerData);
        }

        #region Copy
        /*
        public static void CopyCharacter(PMDCP.DatabaseConnector.MySql.MySql database, string characterIDFrom, string characterIDTo) {
            string query;
            MultiRowInsert multiRowInsert;

            //move bank
            database.ExecuteNonQuery("DELETE FROM bank WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();
                
            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move statistics
            database.ExecuteNonQuery("DELETE FROM character_statistics WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT character_statistics.TotalPlayTime, character_statistics.LastIPAddressUsed, " +
                "character_statistics.LastMacAddressUsed, character_statistics.LastLogin, character_statistics.LastLogout, " +
                "character_statistics.LastPlayTime, character_statistics.LastOS, character_statistics.LastDotNetVersion " +
                "FROM character_statistics " +
                "WHERE character_statistics.CharID = \'" + characterIDFrom + "\'";

            multiRowInsert = new MultiRowInsert(database, "character_statistics", "CharID", "TotalPlayTime", "LastIPAddressUsed", "LastMacAddressUsed",
                "LastLogin", "LastLogout", "LastPlayTime", "LastOS", "LastDotNetVersion");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["TotalPlayTime"].ValueString, columnCollection["LastIPAddressUsed"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["LastMacAddressUsed"].ValueString, columnCollection["LastLogin"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["LastLogout"].ValueString, columnCollection["LastPlayTime"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["LastOS"].ValueString, columnCollection["LastDotNetVersion"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move characteristics
            database.ExecuteNonQuery("DELETE FROM characteristics WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT characteristics.Name, characteristics.Access, characteristics.ActiveSlot, " +
                "characteristics.PK, characteristics.Solid, characteristics.Status, characteristics.Veteran, InTempMode, Dead " +
                "FROM characteristics " +
                "WHERE characteristics.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "characteristics", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["Name"].ValueString, columnCollection["Access"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["ActiveSlot"].ValueString, columnCollection["PK"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Solid"].ValueString.ToBool().ToIntString(), columnCollection["Status"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Veteran"].ValueString.ToBool().ToIntString(), columnCollection["InTempMode"].ValueString.ToBool().ToIntString(), columnCollection["Dead"].ValueString.ToBool().ToIntString());

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move completed mail
            database.ExecuteNonQuery("DELETE FROM completed_mail WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT completed_mail.1, completed_mail.Code, " +
                "FROM completed_mail " +
                "WHERE completed_mail.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "completed_mail", "CharID", "Slot", "Code");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["Slot"].ValueString, columnCollection["Code"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move dungeons
            database.ExecuteNonQuery("DELETE FROM dungeons WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT dungeons.DungeonID, dungeons.CompletionCount, " +
                "FROM dungeons " +
                "WHERE dungeons.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "dungeons", "CharID", "DungeonID", "CompletionCount");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["DungeonID"].ValueString, columnCollection["CompletionCount"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move expkit
            database.ExecuteNonQuery("DELETE FROM expkit WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT expkit.AvailableModules " +
                "FROM expkit " +
                "WHERE expkit.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "expkit", "CharID", "AvailableModules");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["AvailableModules"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move friends
            database.ExecuteNonQuery("DELETE FROM friends WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT friends.FriendListSlot, friends.FriendName " +
                "FROM friends " +
                "WHERE friends.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "friends", "CharID", "FriendListSlot", "FriendName");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["FriendListSlot"].ValueString, columnCollection["FriendName"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move guild
            database.ExecuteNonQuery("DELETE FROM guild WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT guild.GuildName, guild.GuildAccess " +
                "FROM guild " +
                "WHERE guild.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "guild", "CharID", "GuildName", "GuildAccess");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["GuildName"].ValueString, columnCollection["GuildAccess"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move inventory
            database.ExecuteNonQuery("DELETE FROM inventory WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT inventory.ItemSlot, inventory.ItemNum, inventory.Amount, " +
                "inventory.Sticky, inventory.Tag " +
                "FROM inventory " +
                "WHERE inventory.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "inventory", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move items
            database.ExecuteNonQuery("DELETE FROM items WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT items.MaxInv, items.MaxInv " +
                "FROM items " +
                "WHERE items.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "items", "CharID", "MaxInv", "MaxBank");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["MaxInv"].ValueString, columnCollection["MaxInv"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move job list
            database.ExecuteNonQuery("DELETE FROM job_list WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT job_list.JobListSlot, job_list.Code, job_list.Accepted, " +
                "job_list.MaxSends, job_list.CurrentSends " +
                "FROM job_list " +
                "WHERE job_list.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "job_list", "CharID", "JobListSlot", "Code", "Accepted", "MaxSends", "CurrentSends");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["JobListSlot"].ValueString, columnCollection["Code"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Accepted"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["MaxSends"].ValueString, columnCollection["CurrentSends"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move location
            database.ExecuteNonQuery("DELETE FROM location WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT location.Map, location.X, location.Y, location.Direction " +
                "FROM location " +
                "WHERE location.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "location", "CharID", "Map", "X", "Y", "Direction");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["Map"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["X"].ValueString, columnCollection["Y"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Direction"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move mapload trigger events
            database.ExecuteNonQuery("DELETE FROM map_load_trigger_event WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT map_load_trigger_event.ID, map_load_trigger_event.MapID, " +
                "FROM map_load_trigger_event " +
                "WHERE map_load_trigger_event.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "map_load_trigger_event", "CharID", "ID", "MapID");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ID"].ValueString, columnCollection["MapID"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move mission board
            database.ExecuteNonQuery("DELETE FROM mission_board WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT mission_board.LastGenerationDate, " +
                "FROM mission_board " +
                "WHERE mission_board.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "mission_board", "CharID", "LastGenerationDate");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["LastGenerationDate"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move mission board missions
            database.ExecuteNonQuery("DELETE FROM mission_board_missions WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT mission_board_missions.Slot, mission_board_missions.Code " +
                "FROM mission_board_missions " +
                "WHERE mission_board_missions.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "mission_board_missions", "CharID", "Slot", "Code");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["Slot"].ValueString, columnCollection["Code"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move missions
            database.ExecuteNonQuery("DELETE FROM missions WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT missions.MissionExp " +
                "FROM missions " +
                "WHERE missions.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "missions", "CharID", "MissionExp");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["MissionExp"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move recruit data
            database.ExecuteNonQuery("DELETE FROM recruit_data WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT recruit_data.RecruitIndex, recruit_data.UsingTempStats, recruit_data.Name, recruit_data.Species, " +
                "recruit_data.Nickname, recruit_data.NpcBase, recruit_data.Sex, recruit_data.Shiny, recruit_data.Form, " +
                "recruit_data.HeldItemSlot, recruit_data.Level, recruit_data.Experience, recruit_data.HP, recruit_data.StatusAilment, " +
                "recruit_data.StatusAilmentCounter, recruit_data.IQ, recruit_data.Belly, recruit_data.MaxBelly, recruit_data.AttackBonus, " +
                "recruit_data.DefenseBonus, recruit_data.SpeedBonus, recruit_data.SpecialAttackBonus, recruit_data.SpecialDefenseBonus " +
                "FROM recruit_data " +
                "WHERE recruit_data.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "recruit_data", "CharID", "RecruitIndex", "UsingTempStats", "Name",
                "Species", "Nickname", "NpcBase", "Sex", "Shiny", "Form",
                "HeldItemSlot", "Level", "Experience", "HP", "StatusAilment",
                "StatusAilmentCounter", "IQ", "Belly", "MaxBelly", "AttackBonus",
                "DefenseBonus", "SpeedBonus", "SpecialAttackBonus", "SpecialDefenseBonus");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["RecruitIndex"].ValueString, columnCollection["UsingTempStats"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Name"].ValueString, columnCollection["Species"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Nickname"].ValueString.ToBool().ToIntString(), columnCollection["NpcBase"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Sex"].ValueString, columnCollection["Shiny"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Form"].ValueString, columnCollection["HeldItemSlot"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Level"].ValueString, columnCollection["Experience"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["HP"].ValueString, columnCollection["StatusAilment"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["StatusAilmentCounter"].ValueString, columnCollection["IQ"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Belly"].ValueString, columnCollection["MaxBelly"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["AttackBonus"].ValueString, columnCollection["DefenseBonus"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["SpeedBonus"].ValueString, columnCollection["SpecialAttackBonus"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["SpecialDefenseBonus"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move recruit list
            database.ExecuteNonQuery("DELETE FROM recruit_list WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move recruit moves
            database.ExecuteNonQuery("DELETE FROM recruit_moves WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move recruit volatile status
            database.ExecuteNonQuery("DELETE FROM recruit_volatile_status WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move script ex
            database.ExecuteNonQuery("DELETE FROM script_extras_general WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move step counter trigger event
            database.ExecuteNonQuery("DELETE FROM step_counter_trigger_event WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move step on tile trigger event
            database.ExecuteNonQuery("DELETE FROM stepped_on_tile_trigger_event WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move story
            database.ExecuteNonQuery("DELETE FROM story WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move story chapters
            database.ExecuteNonQuery("DELETE FROM story_chapters WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move story helper state settings
            database.ExecuteNonQuery("DELETE FROM story_helper_state_settings WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move team
            database.ExecuteNonQuery("DELETE FROM team WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

            //move rigger events
            database.ExecuteNonQuery("DELETE FROM trigger_events WHERE CharID = \'" + characterIDTo + "\'");
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + characterIDFrom + "\';";
            multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum", "Amount", "Sticky", "Tag");
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(characterIDTo);
                multiRowInsert.AddColumnData(columnCollection["ItemSlot"].ValueString, columnCollection["ItemNum"].ValueString);
                multiRowInsert.AddColumnData(columnCollection["Amount"].ValueString, columnCollection["Sticky"].ValueString.ToBool().ToIntString());
                multiRowInsert.AddColumnData(columnCollection["Tag"].ValueString);

                multiRowInsert.AddRowClosing();

            }
            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());

        }
        */
        #endregion

        public static void UnlinkCharacter(IDatabase database, string accountName, int slot) {
            database.DeleteRow("characters", @"UPPER(""AccountName"") = UPPER(@AccountName) AND Slot = @Slot", new { AccountName = accountName, Slot = slot });
        }

        public static void DeleteCharacter(PMDCP.DatabaseConnector.MySql.MySql database, string characterID) {
            database.DeleteRow("bank", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("character_statistics", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("characteristics", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("dungeons", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("expkit", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("friends", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("guild", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("inventory", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("items", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("job_list", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("location", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("map_load_trigger_event", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("mission_board_missions", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("missions", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("recruit_data", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("recruit_list", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("recruit_moves", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("recruit_volatile_status", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("script_extras_general", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("step_counter_trigger_event", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("stepped_on_tile_trigger_event", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("story", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("story_chapters", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("story_helper_state_settings", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("team", @"""CharID"" = @CharID", new { CharID = characterID });
            database.DeleteRow("trigger_events", @"""CharID"" = @CharID", new { CharID = characterID });
        }

        public static void DeleteAccount(PMDCP.DatabaseConnector.MySql.MySql database, string accountName) {
            // Delete the account record
            database.DeleteRow("accounts", @"""AccountName"" = @AccountName", new { AccountName = accountName });
            for (int i = 1; i <= 3; i++) {
                // Delete all of the characters
                if (!IsCharacterSlotEmpty(database, accountName, i)) {
                    string characterID = RetrieveAccountCharacterID(database, accountName, i);
                    if (!string.IsNullOrEmpty(characterID)) {
                        UnlinkCharacter(database, accountName, i);
                        DeleteCharacter(database, characterID);
                    }
                }
            }
        }

        public static bool IsAccountNameTaken(PMDCP.DatabaseConnector.MySql.MySql database, string accountName) {
            string query = "SELECT accounts.AccountName FROM accounts " +
                "WHERE UPPER(accounts.AccountName) = UPPER(\'" + database.VerifyValueString(accountName) + "\')";

            bool accountFound = false;
            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                if (row["AccountName"].ValueString.ToUpper() == accountName.ToUpper()) {
                    accountFound = true;
                }
            }

            return accountFound;
        }

        public static bool IsCharacterSlotEmpty(PMDCP.DatabaseConnector.MySql.MySql database, string accountName, int slot) {
            string query = "SELECT characters.CharID " +
                "FROM characters " +
                "WHERE characters.AccountName = \'" + database.VerifyValueString(accountName) + "\' " +
                "AND characters.Slot = \'" + slot.ToString() + "\'";
            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                if (!string.IsNullOrEmpty(row["CharID"].ValueString)) {
                    return false;
                } else {
                    return true;
                }
            } else {
                return true;
            }
        }

        public static string FindLinkedDiscordCharacter(MySql database, ulong discordID)
        {
            var query = "SELECT CharID FROM characteristics WHERE LinkedDiscordId = " + discordID;

            var row = database.RetrieveRow(query);
            if (row != null)
            {
                return row["CharID"].ValueString;
            }

            return null;
        }

        public static ulong FindLinkedCharacterDiscord(MySql database, string charID)
        {
            var query = "SELECT LinkedDiscordId FROM characteristics WHERE CharID = \'" + charID + "\'";

            var row = database.RetrieveRow(query);
            if (row != null)
            {
                return row["LinkedDiscordId"].ValueString.ToUlng();
            }

            return 0;
        }

        public static string GetCharacterName(MySql database, string charID)
        {
            var query = "SELECT Name FROM characteristics WHERE CharID = \'" + charID + "\'";

            var row = database.RetrieveRow(query);
            if (row != null)
            {
                return row["Name"].ValueString;
            }

            return null;
        }

        public static bool IsRecruitSlotEmpty(PMDCP.DatabaseConnector.MySql.MySql database, string charID, int recruitIndex) {
            string query = "SELECT recruit_data.CharID " +
                "FROM recruit_data " +
                "WHERE recruit_data.CharID = \'" + database.VerifyValueString(charID) + "\' " +
                "AND recruit_data.RecruitIndex = \'" + recruitIndex.ToString() + "\' " +
                "AND recruit_data.UsingTempStats = \'0\'";
            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                if (!string.IsNullOrEmpty(row["CharID"].ValueString)) {
                    return false;
                } else {
                    return true;
                }
            } else {
                return true;
            }
        }

        public static int FindOpenRecruitmentSlot(PMDCP.DatabaseConnector.MySql.MySql database, string charID) {
            string query = "SELECT recruit_data.RecruitIndex " +
                "FROM recruit_data " +
                "WHERE recruit_data.CharID = \'" + database.VerifyValueString(charID) + "\' " +
                "AND recruit_data.Name = \'#DELETED#\' " +
                "AND recruit_data.Level = \'-1\' " +
                "AND recruit_data.IQ = \'-1\'";

            int slot = -1;
            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                slot = row["RecruitIndex"].ValueString.ToInt();
            }

            if (slot == -1) {
                query = "SELECT recruit_data.RecruitIndex " +
                    "FROM recruit_data " +
                    "WHERE recruit_data.CharID = \'" + database.VerifyValueString(charID) + "\' " +
                    "ORDER BY recruit_data.RecruitIndex DESC";

                row = database.RetrieveRow(query);
                if (row != null) {
                    slot = row["RecruitIndex"].ValueString.ToInt() + 1;
                }
            }

            return slot;
        }

        public static bool IsPasswordCorrect(PMDCP.DatabaseConnector.MySql.MySql database, string accountName, string encryptedPassword) {
            string query = "SELECT accounts.AccountName, accounts.Password " +
                "FROM accounts " +
                "WHERE UPPER(accounts.AccountName) = UPPER(\'" + database.VerifyValueString(accountName) + "\') " +
                "AND accounts.Password = \'" + database.VerifyValueString(encryptedPassword) + "\'";

            bool passwordCorrect = false;
            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                if (row["AccountName"].ValueString.ToUpper() == accountName.ToUpper() &&
                    row["Password"].ValueString == encryptedPassword) {
                    passwordCorrect = true;
                }
            }

            return passwordCorrect;
        }

        /*
        public static bool IsWonderMailCompleted(PMDCP.DatabaseConnector.MySql.MySql database, string charID, string wonderMail) {
            string query = "SELECT completed_mail.Code " +
                "FROM completed_mail " +
                "WHERE completed_mail.CharID = \'" + database.VerifyValueString(charID) + "\' " +
                "AND completed_mail.Code = \'" + database.VerifyValueString(wonderMail) + "\';";
            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                if (!string.IsNullOrEmpty(row["Code"].ValueString)) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        public static void LoadPlayerCompletedMail(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT completed_mail.Code " +
               "FROM completed_mail " +
               "WHERE completed_mail.CharID = \'" + playerData.CharID + "\';";
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                string code = columnCollection["Code"].ValueString;

                playerData.CompletedMail.Add(code);
            }
        }
        */

        public static bool LoadPlayerData(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT accounts.Email, characteristics.Name, characteristics.Access, characteristics.ActiveSlot, characteristics.PK, " +
                "characteristics.Solid, characteristics.Status, characteristics.Veteran, characteristics.InTempMode, characteristics.Dead, " +
                "characteristics.CanLinkDiscord, characteristics.LinkedDiscordId, " +
                "characteristics.IsSandboxed, " +
                "expkit.AvailableModules, " +
                "location.Map, location.X, location.Y, location.Direction, " +
                "guild.GuildName, guild.GuildAccess, " +
                "story.CurrentChapter, story.CurrentSegment, " +
                "items.MaxInv, items.MaxBank, " +
                "missions.MissionExp, missions.LastGenTime, missions.Completions " +
                "FROM characteristics " +
                "LEFT OUTER JOIN characters ON characters.CharID = characteristics.CharID " +
                "LEFT OUTER JOIN accounts ON accounts.AccountName = characters.AccountName " +
                "LEFT OUTER JOIN expkit ON expkit.CharID = characteristics.CharID " +
                "LEFT OUTER JOIN location ON characteristics.CharID = location.CharID " +
                "LEFT OUTER JOIN guild ON characteristics.CharID = guild.CharID " +
                "LEFT OUTER JOIN story ON characteristics.CharID = story.CharID " +
                "LEFT OUTER JOIN items ON characteristics.CharID = items.CharID " +
                "LEFT OUTER JOIN missions ON items.CharID = missions.CharID " +
                "WHERE characteristics.CharID = \'" + database.VerifyValueString(playerData.CharID) + "\';";

            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                playerData.Email = row["Email"].ValueString;
                playerData.Name = row["Name"].ValueString;
                playerData.Access = row["Access"].ValueString.ToByte();
                playerData.ActiveSlot = row["ActiveSlot"].ValueString.ToInt();
                playerData.PK = row["PK"].ValueString.ToBool();
                playerData.Solid = row["Solid"].ValueString.ToBool();
                playerData.Status = row["Status"].ValueString;
                playerData.Veteran = row["Veteran"].ValueString.ToBool();
                playerData.InTempMode = row["InTempMode"].ValueString.ToBool();
                playerData.Dead = row["Dead"].ValueString.ToBool();
                playerData.CanLinkDiscord = row["CanLinkDiscord"].ValueString.ToBool();
                playerData.LinkedDiscordId = row["LinkedDiscordId"].ValueString.ToUlng();
                playerData.IsSandboxed = row["IsSandboxed"].ValueString.ToBool();

                playerData.AvailableModules = row["AvailableModules"].ValueString;

                playerData.Map = row["Map"].ValueString;
                playerData.X = row["X"].ValueString.ToInt();
                playerData.Y = row["Y"].ValueString.ToInt();
                playerData.Direction = row["Direction"].ValueString.ToByte();

                playerData.GuildName = row["GuildName"].ValueString;
                playerData.GuildAccess = row["GuildAccess"].ValueString.ToByte();

                playerData.CurrentChapter = row["CurrentChapter"].ValueString;
                playerData.CurrentSegment = row["CurrentSegment"].ValueString.ToInt(-1);

                playerData.MaxInv = row["MaxInv"].ValueString.ToInt(-1);
                playerData.MaxBank = row["MaxBank"].ValueString.ToInt(-1);

                playerData.MissionExp = row["MissionExp"].ValueString.ToInt(0);
                playerData.LastGenTime = row["LastGenTime"].ValueString.ToInt();
                playerData.MissionCompletions = row["Completions"].ValueString.ToInt();
            } else {
                return false;
            }

            // Load team data
            query = "SELECT team.Slot, team.RecruitIndex, team.UsingTempStats " +
                "FROM team " +
                "WHERE team.CharID = \'" + playerData.CharID + "\' " +
                "AND team.Slot >= 0 " +
                "AND team.Slot < 4";
            List<DataColumnCollection> rows = database.RetrieveRows(query);
            if (rows != null) {
                for (int i = 0; i < rows.Count; i++) {
                    int slot = rows[i]["Slot"].ValueString.ToInt();
                    playerData.TeamMembers[slot] = new PlayerDataTeamMember();
                    playerData.TeamMembers[slot].RecruitIndex = rows[i]["RecruitIndex"].ValueString.ToInt();
                    playerData.TeamMembers[slot].UsingTempStats = rows[i]["UsingTempStats"].ValueString.ToBool();
                }
            }

            if (playerData.TeamMembers[0].RecruitIndex == -1) {
                playerData.TeamMembers[0].RecruitIndex = 0;
            }

            // Load inventory
            query = "SELECT inventory.ItemSlot, inventory.ItemNum, inventory.Amount, " +
                "inventory.Sticky, inventory.Tag, inventory.is_sandboxed " +
                "FROM inventory " +
                "WHERE inventory.CharID = \'" + playerData.CharID + "\';";
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                Characters.InventoryItem invItem = new Characters.InventoryItem();
                int itemSlot = columnCollection["ItemSlot"].ValueString.ToInt(-1);
                if (itemSlot > -1) {
                    invItem.Num = columnCollection["ItemNum"].ValueString.ToInt(0);
                    invItem.Amount = columnCollection["Amount"].ValueString.ToInt(0);
                    invItem.Sticky = columnCollection["Sticky"].ValueString.ToBool();
                    invItem.Tag = columnCollection["Tag"].ValueString;
                    invItem.IsSandboxed = columnCollection["is_sandboxed"].ValueString.ToBool();

                    playerData.Inventory.Add(itemSlot, invItem);
                }
            }
            for (int i = 1; i <= playerData.MaxInv; i++) {
                if (playerData.Inventory.ContainsKey(i) == false) {
                    Characters.InventoryItem invItem = new Characters.InventoryItem();
                    playerData.Inventory.Add(i, invItem);
                }
            }

            // Load bank
            query = "SELECT bank.ItemSlot, bank.ItemNum, bank.Amount, " +
                "bank.Sticky, bank.Tag " +
                "FROM bank " +
                "WHERE bank.CharID = \'" + playerData.CharID + "\';";
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                Characters.InventoryItem invItem = new Characters.InventoryItem();
                int itemSlot = columnCollection["ItemSlot"].ValueString.ToInt(-1);
                if (itemSlot > -1) {
                    invItem.Num = columnCollection["ItemNum"].ValueString.ToInt(0);
                    invItem.Amount = columnCollection["Amount"].ValueString.ToInt(0);
                    invItem.Sticky = columnCollection["Sticky"].ValueString.ToBool();
                    invItem.Tag = columnCollection["Tag"].ValueString;

                    playerData.Bank.Add(itemSlot, invItem);
                }
            }
            for (int i = 1; i <= playerData.MaxBank; i++) {
                if (playerData.Bank.ContainsKey(i) == false) {
                    Characters.InventoryItem invItem = new Characters.InventoryItem();
                    playerData.Bank.Add(i, invItem);
                }
            }

            return true;
        }

        public static void LoadPlayerDungeonCompletionCounts(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT dungeons.DungeonID, dungeons.CompletionCount " +
                "FROM dungeons " +
                "WHERE dungeons.CharID = \'" + playerData.CharID + "\';";

            List<DataColumnCollection> rows = database.RetrieveRows(query);
            if (rows != null) {
                for (int i = 0; i < rows.Count; i++) {
                    int dungeonID = rows[i]["DungeonID"].ValueString.ToInt();
                    int completionCount = rows[i]["CompletionCount"].ValueString.ToInt();

                    int index = playerData.DungeonCompletionCounts.IndexOfKey(dungeonID);
                    if (index > -1) {
                        playerData.DungeonCompletionCounts.Values[index] = completionCount;
                    } else {
                        playerData.DungeonCompletionCounts.Add(dungeonID, completionCount);
                    }
                }
            }
            playerData.DungeonCompletionCountsLoaded = true;
        }

        public static void LoadPlayerFriendsList(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT friends.FriendName " +
                "FROM friends " +
                "WHERE friends.CharID = \'" + playerData.CharID + "\';";
            List<DataColumnCollection> rows = database.RetrieveRows(query);
            if (rows != null) {
                for (int i = 0; i < rows.Count; i++) {
                    playerData.Friends.QuickAdd(rows[i]["FriendName"].ValueString);
                }
            }
            playerData.Friends.Loaded = true;
        }

        public static List<GuildMemberData> LoadGuildInfo(PMDCP.DatabaseConnector.MySql.MySql database, string guildName) {
            string query = "SELECT guild.GuildAccess, characteristics.Name, character_statistics.LastLogin " +
                "FROM guild " +
                "LEFT JOIN characteristics ON (guild.CharID = characteristics.CharID) " +
                "LEFT JOIN character_statistics ON (guild.CharID = character_statistics.CharID) " +
                "WHERE guild.GuildName = \'" + guildName + "\';";

            List<GuildMemberData> guildMembers = new List<GuildMemberData>();
            List<DataColumnCollection> rows = database.RetrieveRows(query);
            if (rows != null) {
                for (int i = 0; i < rows.Count; i++) {
                    GuildMemberData data = new GuildMemberData();
                    data.GuildAccess = rows[i]["GuildAccess"].ValueString.ToInt();
                    data.Name = rows[i]["Name"].ValueString;
                    data.LastLogin = rows[i]["LastLogin"].ValueString;
                    guildMembers.Add(data);
                }
            }
            return guildMembers;

        }

        public static ListPair<string, int> LoadGuild(PMDCP.DatabaseConnector.MySql.MySql database, string guildName) {
            string query = "SELECT guild.CharID, guild.GuildAccess " +
                "FROM guild " +
                "WHERE guild.GuildName = \'" + guildName + "\';";

            ListPair<string, int> guildMembers = new ListPair<string, int>();
            List<DataColumnCollection> rows = database.RetrieveRows(query);
            if (rows != null) {
                for (int i = 0; i < rows.Count; i++) {
                    guildMembers.Add(rows[i]["CharID"].ValueString, rows[i]["GuildAccess"].ValueString.ToInt());
                }
            }
            return guildMembers;

        }

        public static void SetGuildAccess(PMDCP.DatabaseConnector.MySql.MySql database, string charID, int access) {
            database.ExecuteNonQuery("UPDATE guild SET GuildAccess = " + access + " WHERE CharID = \'" + charID + "\'");
        }

        public static string GetGuildName(PMDCP.DatabaseConnector.MySql.MySql database, string charID) {
            string query = "SELECT guild.GuildName " +
                "FROM guild " +
                "WHERE guild.CharID = \'" + charID + "\';";


            List<DataColumnCollection> rows = database.RetrieveRows(query);
            if (rows != null) {
                return rows[0]["GuildName"].ValueString;
            }
            return null;
        }

        public static void AddGuildMember(PMDCP.DatabaseConnector.MySql.MySql database, string guildName, string charID) {
            database.UpdateOrInsert("guild", new IDataColumn[] {
                database.CreateColumn(false, "CharID", charID),
                database.CreateColumn(false, "GuildName", guildName),
                database.CreateColumn(false, "GuildAccess", "1")
            });
        }

        public static void RemoveGuildMember(IDatabase database, string charID) {
            database.DeleteRow("guild", @"""CharID"" = @CharID", new { CharID = charID });
        }

        public static void RemoveGuild(IDatabase database, string guildName) {
            database.DeleteRow("guild", @"""GuildName"" = @GuildName", new { GuildName = guildName });
        }

        public static void LoadPlayerIgnoreList(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            //string query = "SELECT friends.FriendName " +
            //    "FROM friends " +
            //    "WHERE friends.CharID = \'" + playerData.CharID + "\';";
            //List<DataColumnCollection> rows = database.RetrieveRows(query);
            //if (rows != null) {
            //    for (int i = 0; i < rows.Count; i++) {
            //        playerData.Friends.QuickAdd(rows[i]["FriendName"].ValueString);
            //    }
            //}
            //playerData.Friends.Loaded = true;
        }

        public static void LoadPlayerJobList(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT job_list.Accepted, job_list.SendsRemaining, " +
                "job_list.ClientIndex, job_list.TargetIndex, job_list.RewardIndex, job_list.MissionType, " +
                "job_list.Data1, job_list.Data2, job_list.DungeonIndex, job_list.Goal, " +
                "job_list.RDungeon, job_list.StartScript, job_list.WinScript, job_list.LoseScript " +
                "FROM job_list " +
                "WHERE job_list.CharID = \'" + playerData.CharID + "\' " +
                "ORDER BY job_list.JobListSlot";

            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                PlayerDataJobListItem jobItem = new PlayerDataJobListItem();

                jobItem.Accepted = columnCollection["Accepted"].ValueString.ToInt();
                jobItem.SendsRemaining = columnCollection["SendsRemaining"].ValueString.ToInt();
                jobItem.MissionClientIndex = columnCollection["ClientIndex"].ValueString.ToInt();
                jobItem.TargetIndex = columnCollection["TargetIndex"].ValueString.ToInt();
                jobItem.RewardIndex = columnCollection["RewardIndex"].ValueString.ToInt();
                jobItem.MissionType = columnCollection["MissionType"].ValueString.ToInt();
                jobItem.Data1 = columnCollection["Data1"].ValueString.ToInt();
                jobItem.Data2 = columnCollection["Data2"].ValueString.ToInt();
                jobItem.DungeonIndex = columnCollection["DungeonIndex"].ValueString.ToInt();
                jobItem.GoalMapIndex = columnCollection["Goal"].ValueString.ToInt();
                jobItem.RDungeon = columnCollection["RDungeon"].ValueString.ToBool();
                jobItem.StartStoryScript = columnCollection["StartScript"].ValueString.ToInt();
                jobItem.WinStoryScript = columnCollection["WinScript"].ValueString.ToInt();
                jobItem.LoseStoryScript = columnCollection["LoseScript"].ValueString.ToInt();

                playerData.JobList.Add(jobItem);
            }
        }

        public static void LoadPlayerMissionBoardMissions(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT mission_board_missions.Slot, " +
                "mission_board_missions.ClientIndex, mission_board_missions.TargetIndex, mission_board_missions.RewardIndex, mission_board_missions.MissionType, " +
                "mission_board_missions.Data1, mission_board_missions.Data2, mission_board_missions.DungeonIndex, mission_board_missions.Goal, " +
                "mission_board_missions.RDungeon, mission_board_missions.StartScript, mission_board_missions.WinScript, mission_board_missions.LoseScript " +
               "FROM mission_board_missions " +
               "WHERE mission_board_missions.CharID = \'" + playerData.CharID + "\' " +
               "ORDER BY mission_board_missions.Slot";

            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                int slot = columnCollection["Slot"].ValueString.ToInt();
                PlayerDataJobListItem job = new PlayerDataJobListItem();
                job.MissionClientIndex = columnCollection["ClientIndex"].ValueString.ToInt();
                job.TargetIndex = columnCollection["TargetIndex"].ValueString.ToInt();
                job.RewardIndex = columnCollection["RewardIndex"].ValueString.ToInt();
                job.MissionType = columnCollection["MissionType"].ValueString.ToInt();
                job.Data1 = columnCollection["Data1"].ValueString.ToInt();
                job.Data2 = columnCollection["Data2"].ValueString.ToInt();
                job.DungeonIndex = columnCollection["DungeonIndex"].ValueString.ToInt();
                job.GoalMapIndex = columnCollection["Goal"].ValueString.ToInt();
                job.RDungeon = columnCollection["RDungeon"].ValueString.ToBool();
                job.StartStoryScript = columnCollection["StartScript"].ValueString.ToInt();
                job.WinStoryScript = columnCollection["WinScript"].ValueString.ToInt();
                job.LoseStoryScript = columnCollection["LoseScript"].ValueString.ToInt();
                playerData.MissionBoardMissions.Add(job);
            }

        }

        public static IEnumerable<AssemblyRecruitData> LoadPlayerAssemblyRecruits(PMDCP.DatabaseConnector.MySql.MySql database, string charID) {
            string query = "SELECT recruit_data.RecruitIndex, recruit_data.Name, " +
               "recruit_data.Species, recruit_data.Sex, recruit_data.Shiny, recruit_data.Form, recruit_data.Level " +
               "FROM recruit_data " +
               "WHERE recruit_data.CharID = \'" + database.VerifyValueString(charID) + "\' " +
               "AND recruit_data.RecruitIndex >= \'0\' " +
               "AND recruit_data.UsingTempStats = \'0\' " +
               "AND recruit_data.Name <> \'#DELETED#\' " +
               "AND recruit_data.Level > \'-1\' " +
               "AND recruit_data.Shiny > \'-1\' " +
               "AND recruit_data.Form > \'-1\' " +
               "AND recruit_data.Sex > \'-1\' " +
               "AND recruit_data.IQ > \'-1\' " +
               "ORDER BY recruit_data.RecruitIndex";

            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                int recruitIndex = columnCollection["RecruitIndex"].ValueString.ToInt();

                AssemblyRecruitData recruitData = new AssemblyRecruitData();
                recruitData.RecruitIndex = recruitIndex;
                recruitData.Name = columnCollection["Name"].ValueString;
                recruitData.Species = columnCollection["Species"].ValueString.ToInt();
                recruitData.Sex = columnCollection["Sex"].ValueString.ToInt();
                recruitData.Shiny = columnCollection["Shiny"].ValueString.ToInt();
                recruitData.Form = columnCollection["Form"].ValueString.ToInt();
                recruitData.Level = columnCollection["Level"].ValueString.ToInt();


                yield return recruitData;
            }
        }

        public static RecruitData LoadPlayerRecruit(PMDCP.DatabaseConnector.MySql.MySql database, string charID, int recruitIndex, bool usingTempStats) {
            RecruitData recruitData = new RecruitData();

            // Load general
            string query = "SELECT recruit_data.Name, recruit_data.Nickname, " +
                "recruit_data.NpcBase, recruit_data.Species, recruit_data.Sex, recruit_data.Shiny, recruit_data.Form, recruit_data.HeldItemSlot, " +
                "recruit_data.Level, recruit_data.Experience, recruit_data.HP, " +
                "recruit_data.StatusAilment, recruit_data.StatusAilmentCounter, " +
                "recruit_data.IQ, recruit_data.Belly, recruit_data.MaxBelly, recruit_data.AttackBonus, " +
                "recruit_data.DefenseBonus, recruit_data.SpeedBonus, recruit_data.SpecialAttackBonus, " +
                "recruit_data.SpecialDefenseBonus " +
                "FROM recruit_data " +
                "WHERE recruit_data.CharID = \'" + database.VerifyValueString(charID) + "\' " +
                "AND recruit_data.RecruitIndex = \'" + recruitIndex + "\' " +
                "AND recruit_data.UsingTempStats = \'" + usingTempStats.ToIntString() + "\';";

            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                recruitData.UsingTempStats = usingTempStats;
                recruitData.Name = row["Name"].ValueString;
                recruitData.Nickname = row["Nickname"].ValueString.ToBool();
                recruitData.NpcBase = row["NpcBase"].ValueString.ToInt();
                recruitData.Species = row["Species"].ValueString.ToInt();
                recruitData.Sex = row["Sex"].ValueString.ToByte();
                recruitData.Shiny = row["Shiny"].ValueString.ToInt();
                recruitData.Form = row["Form"].ValueString.ToInt();
                recruitData.HeldItemSlot = row["HeldItemSlot"].ValueString.ToInt(-1);
                recruitData.Level = row["Level"].ValueString.ToInt();
                recruitData.Exp = row["Experience"].ValueString.ToUlng();
                recruitData.HP = row["HP"].ValueString.ToInt();
                recruitData.StatusAilment = row["StatusAilment"].ValueString.ToInt();
                recruitData.StatusAilmentCounter = row["StatusAilmentCounter"].ValueString.ToInt();
                recruitData.IQ = row["IQ"].ValueString.ToInt();
                recruitData.Belly = row["Belly"].ValueString.ToInt();
                recruitData.MaxBelly = row["MaxBelly"].ValueString.ToInt();
                recruitData.AtkBonus = row["AttackBonus"].ValueString.ToInt();
                recruitData.DefBonus = row["DefenseBonus"].ValueString.ToInt();
                recruitData.SpeedBonus = row["SpeedBonus"].ValueString.ToInt();
                recruitData.SpclAtkBonus = row["SpecialAttackBonus"].ValueString.ToInt();
                recruitData.SpclDefBonus = row["SpecialDefenseBonus"].ValueString.ToInt();
            } else {
                return null;
            }

            // Load moves
            query = "SELECT recruit_moves.MoveSlot, recruit_moves.MoveNum, " +
               "recruit_moves.CurrentPP, recruit_moves.MaxPP " +
               "FROM recruit_moves " +
               "WHERE recruit_moves.CharID = \'" + charID + "\' " +
               "AND recruit_moves.RecruitIndex = \'" + recruitIndex + "\' " +
               "AND recruit_moves.UsingTempStats = \'" + usingTempStats.ToIntString() + "\' " +
               "ORDER BY recruit_moves.MoveSlot";

            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                int slot = columnCollection["MoveSlot"].ValueString.ToInt();

                recruitData.Moves[slot] = new Characters.Move();
                recruitData.Moves[slot].MoveNum = columnCollection["MoveNum"].ValueString.ToInt();
                recruitData.Moves[slot].CurrentPP = columnCollection["CurrentPP"].ValueString.ToInt();
                recruitData.Moves[slot].MaxPP = columnCollection["MaxPP"].ValueString.ToInt();
                //recruitData.Moves[slot].Sealed = columnCollection["Sealed"].ValueString.ToBool();
            }

            // Load volatile statuses
            query = "SELECT recruit_volatile_status.Name, recruit_volatile_status.Emoticon, " +
               "recruit_volatile_status.Counter, recruit_volatile_status.Tag " +
               "FROM recruit_volatile_status " +
               "WHERE recruit_volatile_status.CharID = \'" + charID + "\' " +
               "AND recruit_volatile_status.RecruitIndex = \'" + recruitIndex + "\' " +
               "AND recruit_volatile_status.UsingTempStats = \'" + usingTempStats.ToIntString() + "\' " +
               "ORDER BY recruit_volatile_status.StatusNum";

            recruitData.VolatileStatus.Clear();
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                Characters.VolatileStatus volatileStatus = new Characters.VolatileStatus();

                volatileStatus.Name = columnCollection["Name"].ValueString;
                volatileStatus.Emoticon = columnCollection["Emoticon"].ValueString.ToInt();
                volatileStatus.Counter = columnCollection["Counter"].ValueString.ToInt();
                volatileStatus.Tag = columnCollection["Tag"].ValueString;

                recruitData.VolatileStatus.Add(volatileStatus);
            }

            return recruitData;
        }

        public static void LoadPlayerRecruitList(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT recruit_list.PokemonID, recruit_list.Status " +
              "FROM recruit_list " +
              "WHERE recruit_list.CharID = \'" + playerData.CharID + "\'";

            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                int pokemonID = columnCollection["PokemonID"].ValueString.ToInt();
                if (playerData.RecruitList.ContainsKey(pokemonID) == false) {
                    playerData.RecruitList.Add(pokemonID, columnCollection["Status"].ValueString.ToByte());
                }
            }

            playerData.RecruitListLoaded = true;
        }

        public static void LoadPlayerStatistics(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT character_statistics.TotalPlayTime, character_statistics.LastIPAddressUsed, " +
                "character_statistics.LastMacAddressUsed, character_statistics.LastLogin, character_statistics.LastLogout, " +
                "character_statistics.LastPlayTime, character_statistics.LastOS, character_statistics.LastDotNetVersion " +
                "FROM character_statistics " +
                "WHERE character_statistics.CharID = \'" + playerData.CharID + "\'";

            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                playerData.TotalPlayTime = TimeSpan.Parse(row["TotalPlayTime"].ValueString);
                playerData.LastIPAddressUsed = row["LastIPAddressUsed"].ValueString;
                playerData.LastMacAddressUsed = row["LastMacAddressUsed"].ValueString;
                playerData.LastLogin = DateTime.ParseExact(row["LastLogin"].ValueString, "yyyy:MM:dd HH:mm:ss", null);
                playerData.LastLogout = DateTime.ParseExact(row["LastLogout"].ValueString, "yyyy:MM:dd HH:mm:ss", null);
                playerData.LastPlayTime = TimeSpan.Parse(row["LastPlayTime"].ValueString);
                playerData.LastOS = row["LastOS"].ValueString;
                playerData.LastDotNetVersion = row["LastDotNetVersion"].ValueString;
            }
        }

        public static void LoadPlayerStoryChapters(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT story_chapters.Chapter, story_chapters.Complete " +
               "FROM story_chapters " +
               "WHERE story_chapters.CharID = \'" + playerData.CharID + "\';";
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                int chapter = columnCollection["Chapter"].ValueString.ToInt();
                bool complete = columnCollection["Complete"].ValueString.ToBool();

                playerData.StoryChapters.Add(chapter, complete);
            }
        }

        public static void LoadPlayerStoryHelperStateSettings(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT story_helper_state_settings.SettingKey, story_helper_state_settings.Value " +
               "FROM story_helper_state_settings " +
               "WHERE story_helper_state_settings.CharID = \'" + playerData.CharID + "\'";

            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                playerData.StoryHelperStateSettings.Add(
                    columnCollection["SettingKey"].ValueString,
                    columnCollection["Value"].ValueString);
            }
        }

        public static void LoadPlayerTriggerEvents(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            string query = "SELECT trigger_events.ID, trigger_events.Type, trigger_events.Action, " +
               "trigger_events.TriggerCommand, trigger_events.AutoRemove " +
               "FROM trigger_events " +
               "WHERE trigger_events.CharID = \'" + playerData.CharID + "\'";

            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                PlayerDataTriggerEvent triggerEvent = new PlayerDataTriggerEvent();
                triggerEvent.Items.Add("Type", columnCollection["Type"].ValueString);
                triggerEvent.Items.Add("ID", columnCollection["ID"].ValueString);
                triggerEvent.Items.Add("Action", columnCollection["Action"].ValueString);
                triggerEvent.Items.Add("TriggerCommand", columnCollection["TriggerCommand"].ValueString);
                triggerEvent.Items.Add("AutoRemove", columnCollection["AutoRemove"].ValueString);

                playerData.TriggerEvents.Add(triggerEvent);
            }

            for (int i = 0; i < playerData.TriggerEvents.Count; i++) {
                if (playerData.TriggerEvents[i].Items.GetValue("Type") == "1") {
                    query = "SELECT map_load_trigger_event.MapID " +
                        "FROM map_load_trigger_event " +
                        "WHERE map_load_trigger_event.CharID = \'" + playerData.CharID + "\' " +
                        "AND map_load_trigger_event.ID = \'" + playerData.TriggerEvents[i].Items.GetValue("ID") + "\'";

                    foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                        playerData.TriggerEvents[i].Items.Add("MapID", columnCollection["MapID"].ValueString);
                    }
                } else if (playerData.TriggerEvents[i].Items.GetValue("Type") == "2") {
                    query = "SELECT stepped_on_tile_trigger_event.MapID, stepped_on_tile_trigger_event.X, " +
                        "stepped_on_tile_trigger_event.Y " +
                        "FROM stepped_on_tile_trigger_event " +
                        "WHERE stepped_on_tile_trigger_event.CharID = \'" + playerData.CharID + "\' " +
                        "AND stepped_on_tile_trigger_event.ID = \'" + playerData.TriggerEvents[i].Items.GetValue("ID") + "\'";

                    foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                        playerData.TriggerEvents[i].Items.Add("MapID", columnCollection["MapID"].ValueString);
                        playerData.TriggerEvents[i].Items.Add("X", columnCollection["X"].ValueString);
                        playerData.TriggerEvents[i].Items.Add("Y", columnCollection["Y"].ValueString);
                    }
                } else if (playerData.TriggerEvents[i].Items.GetValue("Type") == "3") {
                    query = "SELECT step_counter_trigger_event.Steps, step_counter_trigger_event.StepsCounted " +
                        "FROM step_counter_trigger_event " +
                        "WHERE step_counter_trigger_event.CharID = \'" + playerData.CharID + "\' " +
                        "AND step_counter_trigger_event.ID = \'" + playerData.TriggerEvents[i].Items.GetValue("ID") + "\'";

                    foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                        playerData.TriggerEvents[i].Items.Add("Steps", columnCollection["Steps"].ValueString);
                        playerData.TriggerEvents[i].Items.Add("StepsCounted", columnCollection["StepsCounted"].ValueString);
                    }
                }
            }
        }

        public static string RetrieveAccountCharacterID(PMDCP.DatabaseConnector.MySql.MySql database, string accountName, int characterSlot) {
            string query = "SELECT characters.CharID " +
               "FROM characters " +
               "WHERE characters.AccountName = \'" + database.VerifyValueString(accountName) + "\' " +
               "AND characters.Slot = \'" + characterSlot.ToString() + "\'";

            DataColumnCollection row = database.RetrieveRow(query); ;
            if (row != null) {
                return row["CharID"].ValueString;
            } else {
                return null;
            }
        }

        public static string[] RetrieveAccountCharacters(PMDCP.DatabaseConnector.MySql.MySql database, string accountName) {
            string query = "SELECT characters.Slot, characteristics.Name " +
               "FROM characters " +
               "JOIN characteristics ON characters.CharID = characteristics.CharID " +
               "WHERE characters.AccountName = \'" + database.VerifyValueString(accountName) + "\' " +
               "ORDER BY characters.Slot";

            string[] characters = new string[3];
            for (int i = 0; i < characters.Length; i++) {
                characters[i] = null;
            }
            foreach (DataColumnCollection columnCollection in database.RetrieveRowsEnumerable(query)) {
                int slot = columnCollection["Slot"].ValueString.ToInt();
                characters[slot - 1] = columnCollection["Name"].ValueString;
            }

            return characters;
        }

        public static string RetrieveCharacterID(PMDCP.DatabaseConnector.MySql.MySql database, string characterName) {
            string query = "SELECT characteristics.CharID " +
                "FROM characteristics " +
                "WHERE UPPER(characteristics.Name) = UPPER(\'" + database.VerifyValueString(characterName) + "\');";

            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                return row["CharID"].ValueString;
            } else {
                return null;
            }
        }

        public static string RetrieveCharacterName(PMDCP.DatabaseConnector.MySql.MySql database, string characterID) {
            string query = "SELECT characteristics.Name " +
                "FROM characteristics " +
                "WHERE characteristics.CharID = \'" + database.VerifyValueString(characterID) + "\';";

            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                return row["Name"].ValueString;
            } else {
                return null;
            }
        }

        public static bool RetrieveCharacterInformation(PMDCP.DatabaseConnector.MySql.MySql database, string characterName, ref string characterID, ref string characterAccount, ref int characterSlot) {
            string query = "SELECT characteristics.CharID, characters.AccountName, characters.Slot " +
                "FROM characteristics " +
                "JOIN characters ON characters.CharID = characteristics.CharID " +
                "WHERE characteristics.Name = \'" + database.VerifyValueString(characterName) + "\'";

            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                characterID = row["CharID"].ValueString;
                characterAccount = row["AccountName"].ValueString;
                characterSlot = row["Slot"].ValueString.ToInt();

                return true;
            } else {
                return false;
            }
        }

        public static bool RetrievePlayerStoryChapterState(PMDCP.DatabaseConnector.MySql.MySql database, string characterID, int chapter) {
            string query = "SELECT story_chapters.Complete " +
              "FROM story_chapters " +
              "WHERE story_chapters.CharID = \'" + database.VerifyValueString(characterID) + "\' " +
              "AND story_chapters.Chapter = \'" + chapter + "\'";
            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                return row["Complete"].ValueString.ToBool();
            } else {
                return false;
            }
        }

        public static void SavePlayerStoryChapterState(PMDCP.DatabaseConnector.MySql.MySql database, string characterID, int chapter, bool value) {
            database.UpdateOrInsert("story_chapters", new IDataColumn[] {
                database.CreateColumn(false, "CharID", characterID),
                database.CreateColumn(false, "Chapter", chapter.ToString()),
                database.CreateColumn(false, "Complete", value.ToIntString())
            });
        }

        public static int RetrievePlayerDungeonCompletionCount(PMDCP.DatabaseConnector.MySql.MySql database, string characterID, int dungeonID) {
            string query = "SELECT dungeons.CompletionCount " +
              "FROM dungeons " +
              "WHERE dungeons.CharID = \'" + database.VerifyValueString(characterID) + "\' " +
              "AND dungeons.DungeonID = \'" + dungeonID + "\'";
            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                return row["CompletionCount"].ValueString.ToInt();
            } else {
                return -1;
            }
        }

        public static void SavePlayerDungeonCompletionCount(PMDCP.DatabaseConnector.MySql.MySql database, string characterID, int dungeonID, int completionCount) {
            database.UpdateOrInsert("dungeons", new IDataColumn[] {
                database.CreateColumn(false, "CharID", characterID),
                database.CreateColumn(false, "DungeonID", dungeonID.ToString()),
                database.CreateColumn(false, "CompletionCount", completionCount.ToString())
            });
        }

        public static byte RetrievePlayerRecruitListStatus(PMDCP.DatabaseConnector.MySql.MySql database, string characterID, int pokemonID) {
            string query = "SELECT recruit_list.Status " +
              "FROM recruit_list " +
              "WHERE recruit_list.CharID = \'" + database.VerifyValueString(characterID) + "\' " +
              "AND recruit_list.PokemonID = \'" + pokemonID + "\'";

            DataColumnCollection row = database.RetrieveRow(query);
            if (row != null) {
                return row["Status"].ValueString.ToByte();
            } else {
                return 0;
            }
        }

        public static void SavePlayerRecruitListStatus(PMDCP.DatabaseConnector.MySql.MySql database, string characterID, int pokemonID, byte status) {
            database.UpdateOrInsert("recruit_list", new IDataColumn[] {
                database.CreateColumn(false, "CharID", characterID),
                database.CreateColumn(false, "PokemonID", pokemonID.ToString()),
                database.CreateColumn(false, "Status", status.ToString())
            });
        }


        public static void SavePlayerAvailableExpKitModules(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.UpdateOrInsert("expkit", new IDataColumn[]
            {
                database.CreateColumn(false, "CharID", playerData.CharID),
                database.CreateColumn(false, "AvailableModules", playerData.AvailableModules.ToString())
            });
        }

        public static void SavePlayerBank(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.DeleteRow("bank", @"""CharID"" = @CharID AND ""ItemSlot"" > @ItemSlot", new { CharID = playerData.CharID, ItemSlot = playerData.MaxBank });

            MultiRowInsert multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum",
                "Amount", "Sticky", "Tag");
            for (int i = 0; i < playerData.Bank.Count; i++) {
                Characters.InventoryItem invItem = playerData.Bank.ValueByIndex(i);
                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(playerData.CharID);
                multiRowInsert.AddColumnData(playerData.Bank.KeyByIndex(i), invItem.Num, invItem.Amount);
                multiRowInsert.AddColumnData(invItem.Sticky.ToIntString(), invItem.Tag);

                multiRowInsert.AddRowClosing();
            }

            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());
        }

        public static void SavePlayerBankUpdates(PMDCP.DatabaseConnector.MySql.MySql database, string charID, ListPair<int, Characters.InventoryItem> updateList) {
            MultiRowInsert multiRowInsert = new MultiRowInsert(database, "bank", "CharID", "ItemSlot", "ItemNum",
                "Amount", "Sticky", "Tag");

            for (int i = 0; i < updateList.Count; i++) {
                Characters.InventoryItem invItem = updateList.ValueByIndex(i);

                multiRowInsert.AddRowOpening();

                multiRowInsert.AddColumnData(charID);
                multiRowInsert.AddColumnData(updateList.KeyByIndex(i), invItem.Num, invItem.Amount);
                multiRowInsert.AddColumnData(invItem.Sticky.ToIntString(), invItem.Tag);

                multiRowInsert.AddRowClosing();
            }

            database.ExecuteNonQuery(multiRowInsert.GetSqlQuery());
        }

        public static void SavePlayerBankItem(PMDCP.DatabaseConnector.MySql.MySql database, string charID, int slot, Characters.InventoryItem item) {
            database.UpdateOrInsert("bank", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", charID),
                    database.CreateColumn(false, "ItemSlot", slot.ToString()),
                    database.CreateColumn(false, "ItemNum", item.Num.ToString()),
                    database.CreateColumn(false, "Amount", item.Amount.ToString()),
                    database.CreateColumn(false, "Sticky", item.Sticky.ToIntString()),
                    database.CreateColumn(false, "Tag", item.Tag)
                });
        }

        public static void SavePlayerCharacteristics(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.UpdateOrInsert("characteristics", new IDataColumn[]
            {
                database.CreateColumn(false, "CharID", playerData.CharID),
                database.CreateColumn(false, "Name", playerData.Name),
                database.CreateColumn(false, "Access", playerData.Access.ToString()),
                database.CreateColumn(false, "ActiveSlot", playerData.ActiveSlot.ToString()),
                database.CreateColumn(false, "PK", playerData.PK.ToIntString()),
                database.CreateColumn(false, "Solid", playerData.Solid.ToIntString()),
                database.CreateColumn(false, "Status", playerData.Status),
                database.CreateColumn(false, "Veteran", playerData.Veteran.ToIntString()),
                database.CreateColumn(false, "InTempMode", playerData.InTempMode.ToIntString()),
                database.CreateColumn(false, "Dead", playerData.Dead.ToIntString()),
                database.CreateColumn(false, "CanLinkDiscord", playerData.CanLinkDiscord.ToIntString()),
                database.CreateColumn(false, "LinkedDiscordId", playerData.LinkedDiscordId.ToString()),
                database.CreateColumn(false, "IsSandboxed", playerData.IsSandboxed.ToIntString())
            });
        }

        /*
        public static void SavePlayerCompletedMail(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.ExecuteNonQuery("DELETE FROM completed_mail WHERE CharID = \'" + playerData.CharID + "\' " +
            "AND completed_mail.Slot > " + (playerData.CompletedMail.Count - 1));

            for (int i = 0; i < playerData.CompletedMail.Count; i++) {
                database.UpdateOrInsert("completed_mail", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "Slot", i.ToString()),
                    database.CreateColumn(false, "Code", playerData.CompletedMail[i])
                });
            }
        }

        public static void AddPlayerCompletedMail(PMDCP.DatabaseConnector.MySql.MySql database, string charID, string code) {
            int count = CountPlayerCompletedMail(database, charID);
            database.AddRow("completed_mail", new IDataColumn[] {
                database.CreateColumn(false, "CharID", charID),
                database.CreateColumn(false, "Slot", (count + 1).ToString()),
                database.CreateColumn(false, "Code", code)
            });
        }

        public static int CountPlayerCompletedMail(PMDCP.DatabaseConnector.MySql.MySql database, string charID) {
            Object returnVal = database.ExecuteQuery("SELECT COUNT(*) FROM completed_mail WHERE CharID = \'" + charID + "\'");
            return Convert.ToInt32(returnVal);
        }
        */

        public static void SavePlayerDungeons(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            StringBuilder dungeonsToDelete = new StringBuilder();
            for (int i = 0; i < playerData.DungeonCompletionCounts.Count; i++) {
                dungeonsToDelete.Append("AND DungeonID <> \'");
                dungeonsToDelete.Append(playerData.DungeonCompletionCounts.KeyByIndex(i));
                if (i == playerData.DungeonCompletionCounts.Count - 1) {
                    dungeonsToDelete.Append("\'");
                } else {
                    dungeonsToDelete.Append("\' ");
                }
            }
            database.ExecuteNonQuery("DELETE FROM dungeons WHERE CharID = \'" + playerData.CharID + "\' " +
                dungeonsToDelete.ToString());

            for (int i = 0; i < playerData.DungeonCompletionCounts.Count; i++) {
                database.UpdateOrInsert("dungeons", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "DungeonID", playerData.DungeonCompletionCounts.KeyByIndex(i).ToString()),
                    database.CreateColumn(false, "CompletionCount", playerData.DungeonCompletionCounts.ValueByIndex(i).ToString())
                });
            }
        }

        public static void SavePlayerFriendsList(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.DeleteRow("friends", @"""CharID"" = @CharID AND FriendListSlot > @FriendListSlot", new { CharID = playerData.CharID, FriendListSlot = (playerData.Friends.Count - 1) });

            for (int i = 0; i < playerData.Friends.Count; i++) {
                database.UpdateOrInsert("friends", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "FriendListSlot", i.ToString()),
                    database.CreateColumn(false, "FriendName", playerData.Friends[i])
                });
            }
        }

        public static void SavePlayerIgnoreList(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            //database.ExecuteNonQuery("DELETE FROM friends WHERE CharID = \'" + playerData.CharID + "\' " +
            //    "AND FriendListSlot > " + (playerData.Friends.Count - 1));
            //database.DeleteRow("friends", "CharID = \'" + playerData.CharID + "\'");

            //for (int i = 0; i < playerData.Friends.Count; i++) {
            //    database.UpdateOrInsert("friends", new IDataColumn[] {
            //        database.CreateColumn(false, "CharID", playerData.CharID),
            //        database.CreateColumn(false, "FriendListSlot", i.ToString()),
            //        database.CreateColumn(false, "FriendName", playerData.Friends[i])
            //    });
            //}
        }


        public static void SavePlayerGuild(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.ExecuteNonQuery("DELETE FROM guild WHERE CharID = \'" + playerData.CharID + "\'");
            if (!String.IsNullOrEmpty(playerData.GuildName) && playerData.GuildAccess > 0) {
                database.UpdateOrInsert("guild", new IDataColumn[]
            {
                database.CreateColumn(false, "CharID", playerData.CharID),
                database.CreateColumn(false, "GuildName", playerData.GuildName),
                database.CreateColumn(false, "GuildAccess", playerData.GuildAccess.ToString())
            });
            }
        }

        public static void SavePlayerInventory(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.ExecuteNonQuery("DELETE FROM inventory WHERE CharID = \'" + playerData.CharID + "\' " +
                "AND ItemSlot > " + (playerData.MaxInv));

            for (int i = 0; i < playerData.Inventory.Count; i++) {
                Characters.InventoryItem invItem = playerData.Inventory.ValueByIndex(i);
                database.UpdateOrInsert("inventory", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "ItemSlot", playerData.Inventory.KeyByIndex(i).ToString()),
                    database.CreateColumn(false, "ItemNum", invItem.Num.ToString()),
                    database.CreateColumn(false, "Amount", invItem.Amount.ToString()),
                    database.CreateColumn(false, "Sticky", invItem.Sticky.ToIntString()),
                    database.CreateColumn(false, "Tag", invItem.Tag),
                    database.CreateColumn(false, "is_sandboxed", invItem.IsSandboxed.ToIntString())
                });
            }
        }

        public static void SavePlayerInventoryItem(PMDCP.DatabaseConnector.MySql.MySql database, string charID, int slot, Characters.InventoryItem item) {
            database.UpdateOrInsert("inventory", new IDataColumn[] {
                database.CreateColumn(false, "CharID", charID),
                database.CreateColumn(false, "ItemSlot", slot.ToString()),
                database.CreateColumn(false, "ItemNum", item.Num.ToString()),
                database.CreateColumn(false, "Amount", item.Amount.ToString()),
                database.CreateColumn(false, "Sticky", item.Sticky.ToIntString()),
                database.CreateColumn(false, "Tag", item.Tag),
                database.CreateColumn(false, "is_sandboxed", item.IsSandboxed.ToIntString())
            });
        }

        public static void SavePlayerInventoryUpdates(PMDCP.DatabaseConnector.MySql.MySql database, string charID, ListPair<int, Characters.InventoryItem> updateList) {
            for (int i = 0; i < updateList.Count; i++) {
                Characters.InventoryItem invItem = updateList.ValueByIndex(i);
                database.UpdateOrInsert("inventory", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", charID),
                    database.CreateColumn(false, "ItemSlot", updateList.KeyByIndex(i).ToString()),
                    database.CreateColumn(false, "ItemNum", invItem.Num.ToString()),
                    database.CreateColumn(false, "Amount", invItem.Amount.ToString()),
                    database.CreateColumn(false, "Sticky", invItem.Sticky.ToIntString()),
                    database.CreateColumn(false, "Tag", invItem.Tag),
                    database.CreateColumn(false, "is_sandboxed", invItem.IsSandboxed.ToIntString())
                });
            }
        }

        public static void SavePlayerItemGenerals(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.UpdateOrInsert("items", new IDataColumn[]
            {
                database.CreateColumn(false, "CharID", playerData.CharID),
                database.CreateColumn(false, "MaxInv", playerData.MaxInv.ToString()),
                database.CreateColumn(false, "MaxBank", playerData.MaxBank.ToString())
            });
        }

        public static void SavePlayerJobList(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.ExecuteNonQuery("DELETE FROM job_list WHERE CharID = \'" + playerData.CharID + "\'");

            for (int i = 0; i < playerData.JobList.Count; i++) {
                PlayerDataJobListItem jobItem = playerData.JobList[i];

                database.UpdateOrInsert("job_list", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "JobListSlot", i.ToString()),
                    database.CreateColumn(false, "Accepted", jobItem.Accepted.ToString()),
                    database.CreateColumn(false, "SendsRemaining", jobItem.SendsRemaining.ToString()),
                    database.CreateColumn(false, "ClientIndex", jobItem.MissionClientIndex.ToString()),
                    database.CreateColumn(false, "TargetIndex", jobItem.TargetIndex.ToString()),
                    database.CreateColumn(false, "RewardIndex", jobItem.RewardIndex.ToString()),
                    database.CreateColumn(false, "MissionType", jobItem.MissionType.ToString()),
                    database.CreateColumn(false, "Data1", jobItem.Data1.ToString()),
                    database.CreateColumn(false, "Data2", jobItem.Data2.ToString()),
                    database.CreateColumn(false, "DungeonIndex", jobItem.DungeonIndex.ToString()),
                    database.CreateColumn(false, "Goal", jobItem.GoalMapIndex.ToString()),
                    database.CreateColumn(false, "RDungeon", jobItem.RDungeon.ToIntString()),
                    database.CreateColumn(false, "StartScript", jobItem.StartStoryScript.ToString()),
                    database.CreateColumn(false, "WinScript", jobItem.WinStoryScript.ToString()),
                    database.CreateColumn(false, "LoseScript", jobItem.LoseStoryScript.ToString())
                });
            }
        }

        public static void SavePlayerLocation(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.UpdateOrInsert("location", new IDataColumn[]
            {
                database.CreateColumn(false, "CharID", playerData.CharID),
                database.CreateColumn(false, "Map", playerData.Map),
                database.CreateColumn(false, "X", playerData.X.ToString()),
                database.CreateColumn(false, "Y", playerData.Y.ToString()),
                database.CreateColumn(false, "Direction", playerData.Direction.ToString())
            });
        }

        /*
        public static void SavePlayerMissionBoardGenerals(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.UpdateOrInsert("mission_board", new IDataColumn[] {
                database.CreateColumn(false, "CharID", playerData.CharID),
                database.CreateColumn(false, "LastGenerationDate", playerData.MissionBoardLastGenerationDate.Ticks.ToString())
            });
        }
        */

        public static void SavePlayerMissionBoardMissions(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.ExecuteNonQuery("DELETE FROM mission_board_missions WHERE CharID = \'" + playerData.CharID + "\'");
            for (int i = 0; i < playerData.MissionBoardMissions.Count; i++) {
                database.UpdateOrInsert("mission_board_missions", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "Slot", i.ToString()),
                    database.CreateColumn(false, "ClientIndex", playerData.MissionBoardMissions[i].MissionClientIndex.ToString()),
                    database.CreateColumn(false, "TargetIndex", playerData.MissionBoardMissions[i].TargetIndex.ToString()),
                    database.CreateColumn(false, "RewardIndex", playerData.MissionBoardMissions[i].RewardIndex.ToString()),
                    database.CreateColumn(false, "MissionType", playerData.MissionBoardMissions[i].MissionType.ToString()),
                    database.CreateColumn(false, "Data1", playerData.MissionBoardMissions[i].Data1.ToString()),
                    database.CreateColumn(false, "Data2", playerData.MissionBoardMissions[i].Data2.ToString()),
                    database.CreateColumn(false, "DungeonIndex", playerData.MissionBoardMissions[i].DungeonIndex.ToString()),
                    database.CreateColumn(false, "Goal", playerData.MissionBoardMissions[i].GoalMapIndex.ToString()),
                    database.CreateColumn(false, "RDungeon", playerData.MissionBoardMissions[i].RDungeon.ToIntString()),
                    database.CreateColumn(false, "StartScript", playerData.MissionBoardMissions[i].StartStoryScript.ToString()),
                    database.CreateColumn(false, "WinScript", playerData.MissionBoardMissions[i].WinStoryScript.ToString()),
                    database.CreateColumn(false, "LoseScript", playerData.MissionBoardMissions[i].LoseStoryScript.ToString())
                });
            }
        }

        public static void SavePlayerMissionGenerals(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.UpdateOrInsert("missions", new IDataColumn[]
            {
                database.CreateColumn(false, "CharID", playerData.CharID),
                database.CreateColumn(false, "MissionExp", playerData.MissionExp.ToString()),
                database.CreateColumn(false, "LastGenTime", playerData.LastGenTime.ToString()),
                database.CreateColumn(false, "Completions", playerData.MissionCompletions.ToString())
            });
        }

        public static void SavePlayerRecruit(PMDCP.DatabaseConnector.MySql.MySql database, string charID, int recruitIndex, RecruitData recruitData) {
            database.UpdateOrInsert("recruit_data", new IDataColumn[] {
                database.CreateColumn(false, "CharID", charID),
                database.CreateColumn(false, "RecruitIndex", recruitIndex.ToString()),
                database.CreateColumn(false, "UsingTempStats", recruitData.UsingTempStats.ToIntString()),
                database.CreateColumn(false, "Name", recruitData.Name),
                database.CreateColumn(false, "Nickname", recruitData.Nickname.ToIntString()),
                database.CreateColumn(false, "NpcBase", recruitData.NpcBase.ToString()),
                database.CreateColumn(false, "Species", recruitData.Species.ToString()),
                database.CreateColumn(false, "Sex", recruitData.Sex.ToString()),
                database.CreateColumn(false, "Shiny", recruitData.Shiny.ToString()),
                database.CreateColumn(false, "Form", recruitData.Form.ToString()),
                database.CreateColumn(false, "HeldItemSlot", recruitData.HeldItemSlot.ToString()),
                database.CreateColumn(false, "Level", recruitData.Level.ToString()),
                database.CreateColumn(false, "Experience", recruitData.Exp.ToString()),
                database.CreateColumn(false, "HP", recruitData.HP.ToString()),
                database.CreateColumn(false, "StatusAilment", recruitData.StatusAilment.ToString()),
                database.CreateColumn(false, "StatusAilmentCounter", recruitData.StatusAilmentCounter.ToString()),
                database.CreateColumn(false, "IQ", recruitData.IQ.ToString()),
                database.CreateColumn(false, "Belly", recruitData.Belly.ToString()),
                database.CreateColumn(false, "MaxBelly", recruitData.MaxBelly.ToString()),
                database.CreateColumn(false, "AttackBonus", recruitData.AtkBonus.ToString()),
                database.CreateColumn(false, "DefenseBonus", recruitData.DefBonus.ToString()),
                database.CreateColumn(false, "SpeedBonus", recruitData.SpeedBonus.ToString()),
                database.CreateColumn(false, "SpecialAttackBonus", recruitData.SpclAtkBonus.ToString()),
                database.CreateColumn(false, "SpecialDefenseBonus", recruitData.SpclDefBonus.ToString())
            });

            for (int i = 0; i < recruitData.Moves.Length; i++) {
                database.UpdateOrInsert("recruit_moves", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", charID),
                    database.CreateColumn(false, "RecruitIndex", recruitIndex.ToString()),
                    database.CreateColumn(false, "UsingTempStats", recruitData.UsingTempStats.ToIntString()),
                    database.CreateColumn(false, "MoveSlot", i.ToString()),
                    database.CreateColumn(false, "MoveNum", recruitData.Moves[i].MoveNum.ToString()),
                    database.CreateColumn(false, "CurrentPP", recruitData.Moves[i].CurrentPP.ToString()),
                    database.CreateColumn(false, "MaxPP", recruitData.Moves[i].MaxPP.ToString()),
                    //database.CreateColumn(false, "Sealed", recruitData.Moves[i].Sealed.ToIntString()),
                });
            }

            // Delete extra statuses
            database.ExecuteNonQuery("DELETE FROM recruit_volatile_status WHERE CharID = \'" + charID + "\' " +
              "AND RecruitIndex = \'" + recruitIndex + "\' " +
              "AND StatusNum > " + (recruitData.VolatileStatus.Count - 1));
            // Update statuses
            for (int i = 0; i < recruitData.VolatileStatus.Count; i++) {
                database.UpdateOrInsert("recruit_volatile_status", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", charID),
                    database.CreateColumn(false, "RecruitIndex", recruitIndex.ToString()),
                    database.CreateColumn(false, "UsingTempStats", recruitData.UsingTempStats.ToIntString()),
                    database.CreateColumn(false, "StatusNum", i.ToString()),
                    database.CreateColumn(false, "Name", recruitData.VolatileStatus[i].Name),
                    database.CreateColumn(false, "Emoticon", recruitData.VolatileStatus[i].Emoticon.ToString()),
                    database.CreateColumn(false, "Counter", recruitData.VolatileStatus[i].Counter.ToString()),
                    database.CreateColumn(false, "Tag", recruitData.VolatileStatus[i].Tag),
                });
            }
        }

        /// <summary>
        /// Will mark a recruit slot as deleted, allowing it to be overwritten. If inTempMode is true, the entry will be deleted instead.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="charID">The char ID.</param>
        /// <param name="recruitIndex">Index of the recruit.</param>
        /// <param name="inTempMode">if set to <c>true</c>, the temporary recruit will be deleted. Otherwise, the main recruit will be deleted.</param>
        public static void DeletePlayerRecruit(PMDCP.DatabaseConnector.MySql.MySql database, string charID, int recruitIndex, bool inTempMode) {
            if (inTempMode == false) {
                // Mark recruit slot as deleted
                database.UpdateOrInsert("recruit_data", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", charID),
                    database.CreateColumn(false, "RecruitIndex", recruitIndex.ToString()),
                    database.CreateColumn(false, "UsingTempStats", inTempMode.ToIntString()),
                    database.CreateColumn(false, "Name", "#DELETED#"),
                    database.CreateColumn(false, "Level", "-1"),
                    database.CreateColumn(false, "IQ", "-1")
                });
            } else {
                // This recruit is in temp stat mode, so just delete it's temp stat mode copy
                database.ExecuteNonQuery("DELETE FROM recruit_data WHERE CharID = \'" + charID + "\' " +
                  "AND RecruitIndex = \'" + recruitIndex + "\' " +
                  "AND UsingTempStats = \'" + inTempMode.ToIntString() + "\'");
            }
            // Delete moves
            database.ExecuteNonQuery("DELETE FROM recruit_moves WHERE CharID = \'" + charID + "\' " +
              "AND RecruitIndex = \'" + recruitIndex + "\' " +
              "AND UsingTempStats = \'" + inTempMode.ToIntString() + "\'");
            // Delete extra statuses
            database.ExecuteNonQuery("DELETE FROM recruit_volatile_status WHERE CharID = \'" + charID + "\' " +
              "AND RecruitIndex = \'" + recruitIndex + "\' " +
              "AND UsingTempStats = \'" + inTempMode.ToIntString() + "\'");
        }

        public static void SavePlayerRecruitList(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            for (int i = 0; i < playerData.StoryHelperStateSettings.Count; i++) {
                database.UpdateOrInsert("recruit_list", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "PokemonID", playerData.RecruitList.KeyByIndex(i).ToString()),
                    database.CreateColumn(false, "Status", playerData.RecruitList.ValueByIndex(i).ToString())
                });
            }
        }

        public static void SavePlayerStatistics(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.UpdateOrInsert("character_statistics", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "TotalPlayTime", playerData.TotalPlayTime.ToString()),
                    database.CreateColumn(false, "LastIPAddressUsed", playerData.LastIPAddressUsed),
                    database.CreateColumn(false, "LastMacAddressUsed", playerData.LastMacAddressUsed),
                    database.CreateColumn(false, "LastLogin", playerData.LastLogin.ToString("yyyy:MM:dd HH:mm:ss")),
                    database.CreateColumn(false, "LastLogout", playerData.LastLogout.ToString("yyyy:MM:dd HH:mm:ss")),
                    database.CreateColumn(false, "LastPlayTime", playerData.LastPlayTime.ToString()),
                    database.CreateColumn(false, "LastOS", playerData.LastOS),
                    database.CreateColumn(false, "LastDotNetVersion", playerData.LastDotNetVersion)
                });
        }

        public static void SavePlayerStoryChapters(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            for (int i = 0; i < playerData.StoryChapters.Count; i++) {
                database.UpdateOrInsert("story_chapters", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "Chapter", playerData.StoryChapters.KeyByIndex(i).ToString()),
                    database.CreateColumn(false, "Complete", playerData.StoryChapters.ValueByIndex(i).ToIntString())
                });
            }
        }

        public static void SavePlayerStoryGenerals(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            database.UpdateOrInsert("story", new IDataColumn[]
            {
                database.CreateColumn(false, "CharID", playerData.CharID),
                database.CreateColumn(false, "CurrentChapter", playerData.CurrentChapter),
                database.CreateColumn(false, "CurrentSegment", playerData.CurrentSegment.ToString())
            });
        }

        public static void SavePlayerStoryHelperStateSettings(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            for (int i = 0; i < playerData.StoryHelperStateSettings.Count; i++) {
                database.UpdateOrInsert("story_helper_state_settings", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "SettingKey", playerData.StoryHelperStateSettings.KeyByIndex(i)),
                    database.CreateColumn(false, "Value", playerData.StoryHelperStateSettings.ValueByIndex(i))
                });
            }
        }

        public static void SavePlayerTeam(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            for (int i = 0; i < playerData.TeamMembers.Length; i++) {
                database.UpdateOrInsert("team", new IDataColumn[]
                {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "Slot", i.ToString()),
                    database.CreateColumn(false, "RecruitIndex", playerData.TeamMembers[i].RecruitIndex.ToString()),
                    database.CreateColumn(false, "UsingTempStats", playerData.TeamMembers[i].UsingTempStats.ToIntString())
                });
            }
        }

        public static void SavePlayerTriggerEvents(PMDCP.DatabaseConnector.MySql.MySql database, PlayerData playerData) {
            for (int i = 0; i < playerData.TriggerEvents.Count; i++) {
                PlayerDataTriggerEvent triggerEvent = playerData.TriggerEvents[i];

                database.UpdateOrInsert("trigger_events", new IDataColumn[] {
                    database.CreateColumn(false, "CharID", playerData.CharID),
                    database.CreateColumn(false, "ID", triggerEvent.Items.GetValue("ID")),
                    database.CreateColumn(false, "Type", triggerEvent.Items.GetValue("Type")),
                    database.CreateColumn(false, "Action", triggerEvent.Items.GetValue("Action")),
                    database.CreateColumn(false, "TriggerCommand", triggerEvent.Items.GetValue("TriggerCommand")),
                    database.CreateColumn(false, "AutoRemove", triggerEvent.Items.GetValue("AutoRemove"))
                });

                switch (triggerEvent.Items.GetValue("Type")) {
                    case "1": {
                            database.UpdateOrInsert("map_load_trigger_event", new IDataColumn[] {
                                database.CreateColumn(false, "CharID", playerData.CharID),
                                database.CreateColumn(false, "ID", triggerEvent.Items.GetValue("ID")),
                                database.CreateColumn(false, "MapID", triggerEvent.Items.GetValue("MapID"))
                            });
                        }
                        break;
                    case "2": {
                            database.UpdateOrInsert("stepped_on_tile_trigger_event", new IDataColumn[] {
                                database.CreateColumn(false, "CharID", playerData.CharID),
                                database.CreateColumn(false, "ID", triggerEvent.Items.GetValue("ID")),
                                database.CreateColumn(false, "MapID", triggerEvent.Items.GetValue("MapID")),
                                database.CreateColumn(false, "X", triggerEvent.Items.GetValue("X")),
                                database.CreateColumn(false, "Y", triggerEvent.Items.GetValue("Y"))
                            });
                        }
                        break;
                    case "3": {
                            database.UpdateOrInsert("step_counter_trigger_event", new IDataColumn[] {
                                database.CreateColumn(false, "CharID", playerData.CharID),
                                database.CreateColumn(false, "ID", triggerEvent.Items.GetValue("ID")),
                                database.CreateColumn(false, "Steps", triggerEvent.Items.GetValue("Steps")),
                                database.CreateColumn(false, "StepsCounted", triggerEvent.Items.GetValue("StepsCounted")),
                            });
                        }
                        break;
                }
            }
        }

        #endregion Methods
    }
}
