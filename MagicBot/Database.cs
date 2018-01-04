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

        #region Update Methods
        public static void UpdateIsSent(Card card, Boolean isSent)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
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

                    cmd.ExecuteNonQuery();
                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        #endregion

        #region Is In Methods

        public static Boolean IsCardInDatabase(Card card, Boolean isSent)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                Int64 count = -1;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT  count(1)
                                            FROM ScryfallCard
                                            WHERE
                                            FullUrlWebSite = @FullUrlWebSite AND
                                            IsCardSent = @IsCardSent";


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

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            count = reader.GetFieldValue<Int64>(0);
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


        public static Boolean IsChatInDatabase(Chat chat)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                Int64 count = -1;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
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

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            count = reader.GetFieldValue<Int64>(0);
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

        public static List<Chat> GetAllChats()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                List<Chat> retList = new List<Chat>();
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT  ChatId,
                                            ifNull(Title,''),
                                            ifNull(FirstName,''),
                                            ifNull(Type,'')
                                            FROM Chat ";

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Chat chat = new Chat();
                            chat.Id = reader.GetFieldValue<Int64>(0);
                            chat.Title = reader.GetFieldValue<String>(1);
                            chat.FirstName = reader.GetFieldValue<String>(2);
                            ChatType type = (ChatType)Enum.Parse(typeof(ChatType), reader.GetFieldValue<String>(3));
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


        public static void InsertScryfallCard(Card card)
        {
            if (Database.IsCardInDatabase(card, false))
                return;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO ScryfallCard
                                                (ScryfallCardId,
                                                Name,
                                                FullUrlWebSite,
                                                IsCardSent)
                                        VALUES
                                                (@ScryfallCardId,
                                                @Name,
                                                @FullUrlWebSite,
                                                @IsCardSent
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
                        Value = false,
                    });


                    cmd.ExecuteNonQuery();

                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

            }
        }

        /// <summary>
        /// Insert new log info
        /// </summary>
        /// <param name="methodName">name of the method</param>
        /// <param name="spoilName">name of the spoil(if any)</param>
        /// <param name="message">message of the log</param>
        /// <returns>ID of the saved log</returns>
        public static Int32 InsertLog(String methodName, String spoilName, String message)
        {
            Int32 result = -1;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
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

                    cmd.ExecuteNonQuery();
                    result = (Int32)cmd.LastInsertedId;
                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                return result;
            }

        }

        public static Int64 InsertChat(Chat chat)
        {
            Int64 ret = -1;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
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

                    cmd.ExecuteNonQuery();
                    ret = (Int64)cmd.LastInsertedId;
                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                return ret;
            }

        }
        #endregion
    }
}