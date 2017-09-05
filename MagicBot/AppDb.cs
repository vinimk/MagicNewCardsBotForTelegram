using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MagicBot
{
    public class Database
    {

        private static String _connectionString;

        public static void SetConnectionString(String connectionString)
        {
            _connectionString = connectionString;
        }

        #region Is In Methods

        public static async Task<Boolean> IsSpoilInDatabase(SpoilItem spoil)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                Int64 count = -1;
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT  count(1)
                                            FROM SpoilItem
                                            WHERE 
                                            Folder = @Folder AND
                                            Date = @Date AND
                                            CardUrl = CardUrl";

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Folder",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Folder,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Date",
                        DbType = DbType.DateTime,
                        Value = spoil.Date,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrl",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl,
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
            }
        }

        public static async Task<Boolean> IsChatInDatabase(Chat chat)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                Int64 count = -1;
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }
                using (MySqlCommand cmd = conn.CreateCommand())
                {
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
            }
        }
        #endregion

        #region Get Methods

        public static async Task<List<Chat>> GetAllChats()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                List<Chat> retList = new List<Chat>();
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
                                            FROM Chat ";

                    using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Chat chat = new Chat();
                            chat.Id = await reader.GetFieldValueAsync<Int64>(0);
                            chat.Title = await reader.GetFieldValueAsync<String>(1);
                            chat.FirstName = await reader.GetFieldValueAsync<String>(2);
                            ChatType type = (ChatType)Enum.Parse(typeof(ChatType), await reader.GetFieldValueAsync<String>(3));
                            chat.Type = type;
                            retList.Add(chat);
                        }
                    }
                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                return retList;
            }
        }

        #endregion

        #region Insert Methods
        public static async Task<Int32> InsertSpoil(SpoilItem spoil)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                Int32 ret = -1;
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO SpoilItem
                                                (Folder,
                                                Date,
                                                CardUrl,
                                                Name,
                                                FullUrlWebSite,
                                                ManaCost,
                                                Type,
                                                Text,
                                                Flavor,
                                                Illustrator,
                                                Power,
                                                Toughness,
                                                ImageUrlWebSite,
                                                AdditionalImageUrlWebSite)
                                        VALUES
                                                (@Folder,
                                                @Date,
                                                @CardUrl,
                                                @Name,
                                                @FullUrlWebSite,
                                                @ManaCost,
                                                @Type,
                                                @Text,
                                                @Flavor,
                                                @Illustrator,
                                                @Power,
                                                @Toughness,
                                                @ImageUrlWebSite,
                                                @AdditionalImageUrlWebSite)";


                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Folder",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Folder,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Date",
                        DbType = DbType.DateTime,
                        Value = spoil.Date,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrl",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@FullUrlWebSite",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.FullUrlWebSite,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Name",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Name,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@ManaCost",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.ManaCost,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Type",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Type,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Text",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Text,
                    });
                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Flavor",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Flavor,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Illustrator",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Illustrator,
                    });

                    if (spoil.Power >= 0)
                    {
                        cmd.Parameters.Add(new MySqlParameter()
                        {
                            ParameterName = "@Power",
                            DbType = DbType.VarNumeric,
                            Value = spoil.Power,
                        });
                    }
                    else
                    {
                        cmd.Parameters.Add(new MySqlParameter()
                        {
                            ParameterName = "@Power",
                            DbType = DbType.VarNumeric,
                            Value = null,
                        });
                    }

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Toughness",
                        DbType = DbType.Int32,
                        Value = spoil.Toughness,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@ImageUrlWebSite",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.ImageUrlWebSite,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@AdditionalImageUrlWebSite",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.AdditionalImageUrlWebSite,
                    });

                    await cmd.ExecuteNonQueryAsync();
                    ret = (Int32)cmd.LastInsertedId;
                }

                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                return ret;
            }
        }

        public static async Task<Int64> InsertChat(Chat chat)
        {
            Int64 ret = -1;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
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
        }
    }
}