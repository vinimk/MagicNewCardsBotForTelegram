using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MagicNewCardsBot
{
    public class Database
    {
        private static String _connectionString;

        public static readonly Int32 MAX_CARDS = 200;

        public enum CardStatus
        {
            NotFound = 0,
            Complete = 1,
            WithoutRarity = 2
        }

        public static void SetConnectionString(String connectionString)
        {
            _connectionString = connectionString;
        }

        #region Update Methods
        async public static Task UpdateIsSentAsync(Card card, Boolean isSent)
        {
            using MySqlConnection conn = new(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE  ScryfallCard
                                        SET     IsCardSent =    @IsCardSent
                                        WHERE
                                            FullUrlWebSite = @FullUrlWebSite";

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@FullUrlWebSite",
                    DbType = DbType.StringFixedLength,
                    Value = card.FullUrlWebSite,
                });
                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@IsCardSent",
                    DbType = DbType.Boolean,
                    Value = isSent,
                });

                await cmd.ExecuteNonQueryAsync();
            }
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        async public static Task UpdateHasRarityAsync(Card card, Boolean hasRarity)
        {
            using MySqlConnection conn = new(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE  ScryfallCard
                                        SET     HasRarity =    @HasRarity
                                        WHERE
                                            FullUrlWebSite = @FullUrlWebSite";

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@FullUrlWebSite",
                    DbType = DbType.StringFixedLength,
                    Value = card.FullUrlWebSite,
                });
                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@HasRarity",
                    DbType = DbType.Boolean,
                    Value = hasRarity,
                });

                await cmd.ExecuteNonQueryAsync();
            }
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        async public static Task UpdateWantedRaritiesForChatAsync(Chat chat, string wantedRarities)
        {
            using MySqlConnection conn = new(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE  Chat
                                        SET     WantedRarities =    @WantedRarities
                                        WHERE
                                            ChatId = @ChatId";

                if (wantedRarities == "ALL")
                {
                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@WantedRarities",
                        DbType = DbType.StringFixedLength,
                        Value = DBNull.Value,
                    });
                }
                else
                {
                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@WantedRarities",
                        DbType = DbType.String,
                        Value = wantedRarities,
                    });
                }
                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@ChatId",
                    DbType = DbType.Int64,
                    Value = chat.Id,
                });

                await cmd.ExecuteNonQueryAsync();
            }
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        #endregion

        #region Is In Methods


        async public static Task<Boolean> IsExtraSideInDatabase(Card mainCard, Boolean isSent)
        {
            using MySqlConnection conn = new(_connectionString);
            Int64 count = -1;
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            foreach (Card card in mainCard.ExtraSides)
            {
                if (!string.IsNullOrWhiteSpace(card.FullUrlWebSite))
                {
                    using MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = @"SELECT  count(1)
                                            FROM ScryfallCard
                                            WHERE
                                            FullUrlWebSite = @FullUrlWebSite AND
                                            IsCardSent = @IsCardSent AND
											Date > @Date";

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@FullUrlWebSite",
                        DbType = DbType.StringFixedLength,
                        Value = card.FullUrlWebSite,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@IsCardSent",
                        DbType = DbType.Boolean,
                        Value = isSent,
                    });

                    //dominaria workaround 
                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Date",
                        MySqlDbType = MySqlDbType.DateTime,
                        Value = new DateTime(2018, 03, 11, 0, 0, 0), //the day that scryfall sent all the new card 
                    });

                    using DbDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        count = await reader.GetFieldValueAsync<Int64>(0);
                        if (count > 0)
                            return true;
                    }
                }
            }

            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }

            return false;
        }

        async public static Task<CardStatus> GetCardStatus(Card card, Boolean isSent)
        {
            using MySqlConnection conn = new(_connectionString);
            int? hasRarity = -1;
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            using MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT HasRarity
                                            FROM ScryfallCard
                                            WHERE
                                            FullUrlWebSite = @FullUrlWebSite AND
                                            IsCardSent = @IsCardSent AND
											Date > @Date";

            cmd.Parameters.Add(new MySqlParameter()
            {
                ParameterName = "@FullUrlWebSite",
                DbType = DbType.StringFixedLength,
                Value = card.FullUrlWebSite,
            });

            cmd.Parameters.Add(new MySqlParameter()
            {
                ParameterName = "@IsCardSent",
                DbType = DbType.Boolean,
                Value = isSent,
            });

            //dominaria workaround 
            cmd.Parameters.Add(new MySqlParameter()
            {
                ParameterName = "@Date",
                MySqlDbType = MySqlDbType.DateTime,
                Value = new DateTime(2018, 03, 11, 0, 0, 0), //the day that scryfall sent all the new card 
            });

            using (DbDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    hasRarity = await reader.GetFieldValueAsync<int>(0);
                }
            }

            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }

            if (hasRarity.HasValue)
            {
                if (hasRarity.Value == 1)
                {
                    return CardStatus.Complete;
                }
                else
                {
                    return CardStatus.WithoutRarity;
                }
            }

            return CardStatus.NotFound;
        }

        async public static Task<Boolean> ChatExistsAsync(Chat chat)
        {
            using MySqlConnection conn = new(_connectionString);
            Int64 count = -1;
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT count(1)
                                            FROM Chat
                                            WHERE
                                            ChatId = @ChatId";

            cmd.Parameters.Add(new MySqlParameter()
            {
                ParameterName = "@ChatId",
                DbType = DbType.Int64,
                Value = chat.Id,
            });

            using (DbDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    count = await reader.GetFieldValueAsync<Int64>(0);
                }
            }

            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }

            return count > 0;
        }
        #endregion

        #region Get Methods

        async public static Task<List<Set>> GetAllCrawlableSetsAsync()
        {
            using MySqlConnection conn = new(_connectionString);
            List<Set> retList = new();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT  SetID,
                                            ifNull(SetURL,''),
                                            ifNull(SetName,'')
                                            FROM Sets 
                                            WHERE ShouldCrawl = 1
                                            ORDER BY SetID desc";

                using DbDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Set set = new()
                    {
                        ID = await reader.GetFieldValueAsync<Int64>(0),
                        URL = await reader.GetFieldValueAsync<String>(1),
                        Name = await reader.GetFieldValueAsync<String>(2),
                    };
                    retList.Add(set);
                }
            }
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            return retList;
        }

        async public static IAsyncEnumerable<Chat> GetChatsAsync(string wantedRarities = null)
        {
            using MySqlConnection conn = new(_connectionString);
            List<Chat> retList = new();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT  ChatId,
                                            ifNull(Title,''),
                                            ifNull(FirstName,''),
                                            ifNull(Type,'')
                                            FROM Chat 
                                            WHERE IsBlocked = 0 
                                            ";

                if (!String.IsNullOrWhiteSpace(wantedRarities))
                {
                    cmd.CommandText += "AND WantedRarities like @WantedRarities";
                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@WantedRarities",
                        DbType = DbType.String,
                        Value = $"%{wantedRarities}%",
                    });
                }
                else
                {
                    cmd.CommandText += "AND WantedRarities IS NULL";
                }

                using DbDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ChatType type = (ChatType)Enum.Parse(typeof(ChatType), await reader.GetFieldValueAsync<String>(3));
                    Chat chat = new()
                    {
                        Id = await reader.GetFieldValueAsync<Int64>(0),
                        Title = await reader.GetFieldValueAsync<String>(1),
                        FirstName = await reader.GetFieldValueAsync<String>(2),
                        Type = type,
                    };
                    yield return chat;
                }
            }
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        #endregion

        #region Insert Methods

        async public static Task InsertScryfallCardAsync(Card card, bool isSent = false, bool hasRarity = false)
        {
            if (await GetCardStatus(card, false) != CardStatus.NotFound)
                return;

            using MySqlConnection conn = new(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO ScryfallCard
                                                (ScryfallCardId,
                                                Name,
                                                FullUrlWebSite,
                                                IsCardSent,
                                                HasRarity)
                                        VALUES
                                                (@ScryfallCardId,
                                                @Name,
                                                @FullUrlWebSite,
                                                @IsCardSent,
                                                @HasRarity
                                                )";

                String id;
                if (card.GetType() == typeof(ScryfallCard))
                {
                    id = ((ScryfallCard)card).id;
                }
                else
                {
                    id = Guid.NewGuid().ToString();
                }

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@ScryfallCardId",
                    DbType = DbType.StringFixedLength,
                    Value = id,
                });

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@Name",
                    DbType = DbType.StringFixedLength,
                    Value = card.Name,
                });

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@FullUrlWebSite",
                    DbType = DbType.StringFixedLength,
                    Value = card.FullUrlWebSite,
                });

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@IsCardSent",
                    DbType = DbType.Boolean,
                    Value = isSent,
                });

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@HasRarity",
                    DbType = DbType.Boolean,
                    Value = hasRarity,
                });

                await cmd.ExecuteNonQueryAsync();

            }
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Insert new log info
        /// </summary>
        /// <param name="methodName">name of the method</param>
        /// <param name="spoilName">name of the spoil(if any)</param>
        /// <param name="message">message of the log</param>
        /// <returns>ID of the saved log</returns>
        async public static Task<Int32> InsertLogAsync(String methodName, String spoilName, String message)
        {
            try
            {
                Int32 result = -1;
                using MySqlConnection conn = new(_connectionString);
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Log
                                            (Message,
                                            Method,
                                            SpoilName
                                            )
                                    VALUES
                                            (@Message,
                                            @Method,
                                            @SpoilName)";

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Message",
                        DbType = DbType.StringFixedLength,
                        Value = message,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Method",
                        DbType = DbType.StringFixedLength,
                        Value = methodName,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@SpoilName",
                        DbType = DbType.StringFixedLength,
                        Value = spoilName,
                    });

                    await cmd.ExecuteNonQueryAsync();
                    result = (Int32)cmd.LastInsertedId;
                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                return result;
            }
            catch (Exception)
            {
                Console.WriteLine("Error inserting log, possible that the server was offline");
                return -1;
            }
        }

        async public static Task<Int64> InsertChatAsync(Chat chat)
        {
            Int64 ret = -1;
            using MySqlConnection conn = new(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Chat
                                            (ChatId,
                                            Title,
                                            FirstName,
                                            Type)
                                    VALUES
                                            (@ChatId,
                                            @Title,
                                            @FirstName,
                                            @Type)";

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@ChatId",
                    DbType = DbType.Int64,
                    Value = chat.Id,
                });

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@Title",
                    DbType = DbType.StringFixedLength,
                    Value = chat.Title,
                });

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@FirstName",
                    DbType = DbType.StringFixedLength,
                    Value = chat.FirstName,
                });

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@Type",
                    DbType = DbType.StringFixedLength,
                    Value = chat.Type.ToString(),
                });

                await cmd.ExecuteNonQueryAsync();
                ret = (Int64)cmd.LastInsertedId;
            }
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            return ret;

        }
        #endregion

        #region Delete Methods
        async public static Task<int> DeleteFromChatAsync(Chat chat)
        {
            return await DeleteFromChatAsync(chat.Id);
        }
        async public static Task<int> DeleteFromChatAsync(long chatId)
        {
            int result;
            using MySqlConnection conn = new(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"DELETE FROM Chat
                                        WHERE ChatId = @ChatId";

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@ChatId",
                    DbType = DbType.Int64,
                    Value = chatId,
                });

                result = await cmd.ExecuteNonQueryAsync();
            }
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            return result;
        }
        #endregion
    }
}