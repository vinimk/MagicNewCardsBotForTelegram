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

        public static Int64 UpdateIsSent(SpoilItem spoil, Boolean isSent)
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
                    cmd.CommandText = @"UPDATE  SpoilItem 
                                        SET     IsCardSent =    @IsCardSent
                                        WHERE 
                                                Folder =        @Folder AND
                                                (CardUrl = @CardUrl OR CardUrl = @CardUrlAlt OR CardUrl = @CardUrlAlt2)";

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@IsCardSent",
                        DbType = DbType.Boolean,
                        Value = isSent,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Folder",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Folder,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrl",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 5) + ".jpg",
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt2",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 4)  + "1.jpg",
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

        private static Int64 UpdateTrysToGetFromWebsite(SpoilItem spoil, Int32 trys)
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
                    cmd.CommandText = @"UPDATE  SpoilItem 
                                        SET     TrysToGetFromWebsite =    @TrysToGetFromWebsite
                                        WHERE 
                                                Folder =        @Folder AND
                                                (CardUrl = @CardUrl OR CardUrl = @CardUrlAlt OR CardUrl = @CardUrlAlt2)";

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@TrysToGetFromWebsite",
                        DbType = DbType.Int32,
                        Value = trys,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Folder",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Folder,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrl",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 5) + ".jpg",
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt2",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 4)  + "1.jpg",
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

        #region Is In Methods

        public static Boolean IsSpoilInDatabase(SpoilItem spoil, Boolean isSent)
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
                                            FROM SpoilItem
                                            WHERE 
                                            Folder = @Folder AND
                                            (CardUrl = @CardUrl OR CardUrl = @CardUrlAlt OR CardUrl = @CardUrlAlt2) AND
                                            IsCardSent = @IsCardSent";

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Folder",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Folder,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrl",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@IsCardSent",
                        DbType = DbType.Boolean,
                        Value = isSent,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 5) + ".jpg",
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt2",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 4)  + "1.jpg",
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

        private static Int32 GetNumberOfTrysToGetFromWebsite(SpoilItem spoil)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                Int32 ret = -1;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT  TrysToGetFromWebsite
                                                    FROM SpoilItem
                                           WHERE 
                                            Folder = @Folder AND
                                            (CardUrl = @CardUrl OR CardUrl = @CardUrlAlt OR CardUrl = @CardUrlAlt2)";

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Folder",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Folder,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrl",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl,
                    });


                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@Folder",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Folder,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 5) + ".jpg",
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt2",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 4)  + "1.jpg",
                    });

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ret = reader.GetFieldValue<Int32>(0);
                        }
                    }
                }

                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                return ret;
            }
        }

        #endregion

        #region Insert Methods
        /// <summary>
        /// This method will check if the spoil is in the database, and if it did already tried to get this information
        /// </summary>
        /// <param name="spoil"></param>
        /// <returns>Number of times that it tryed to get the cards </returns>
        public static Int32 InsertSimpleSpoilAndOrAddCounter(SpoilItem spoil)
        {
            //if the spoil is in the database
            if (IsSpoilInDatabase(spoil, false))
            {
                //get the amount of times that it already tried to send it
                Int32 numberOfTrys = GetNumberOfTrysToGetFromWebsite(spoil);
                //adds one to it
                numberOfTrys++;
                //updates the amount in the database
                UpdateTrysToGetFromWebsite(spoil, numberOfTrys);
                //return the right value
                return numberOfTrys;
            }
            else
            {
                InsertSpoil(spoil);
                return 0;
            }
        }

        public static Int32 InsertOrUpdateSpoil(SpoilItem spoil)
        {
            if (IsSpoilInDatabase(spoil, false))
            {
                return UpdateSpoil(spoil);
            }
            else
            {
                return InsertSpoil(spoil);
            }
        }

        public static Int32 InsertSpoil(SpoilItem spoil)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                Int32 ret = -1;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
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

                    if (spoil.Toughness >= 0)
                    {
                        cmd.Parameters.Add(new MySqlParameter()
                        {
                            ParameterName = "@Toughness",
                            DbType = DbType.VarNumeric,
                            Value = spoil.Toughness,
                        });
                    }
                    else
                    {
                        cmd.Parameters.Add(new MySqlParameter()
                        {
                            ParameterName = "@Toughness",
                            DbType = DbType.VarNumeric,
                            Value = null,
                        });
                    }

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

                    cmd.ExecuteNonQuery();
                    ret = (Int32)cmd.LastInsertedId;
                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                return ret;
            }
        }

        private static Int32 UpdateSpoil(SpoilItem spoil)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                Int32 ret = -1;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE  SpoilItem
                                                SET 
                                                Folder =                        @Folder,
                                                Date =                          @Date,
                                                CardUrl =                       @CardUrl,
                                                Name =                          @Name,
                                                FullUrlWebSite =                @FullUrlWebSite,
                                                ManaCost =                      @ManaCost,
                                                Type =                          @Type,
                                                Text =                          @Text,
                                                Flavor =                        @Flavor,
                                                Illustrator =                   @Illustrator,
                                                Power =                         @Power,
                                                Toughness =                     @Toughness,
                                                ImageUrlWebSite =               @ImageUrlWebSite,
                                                AdditionalImageUrlWebSite =     @AdditionalImageUrlWebSite
                                                WHERE 
                                                Folder =                        @FolderWhere AND
                                                (CardUrl = @CardUrl OR CardUrl = @CardUrlAlt OR CardUrl = @CardUrlAlt2) AND
                                                IsCardSent =                    @IsCardSentWhere";

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@FolderWhere",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.Folder,
                    });


                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlWhere",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl,
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 5) + ".jpg",
                    });

                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@CardUrlAlt2",
                        DbType = DbType.StringFixedLength,
                        Value = spoil.CardUrl.Substring(0, spoil.CardUrl.Length - 4)  + "1.jpg",
                    });


                    cmd.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = "@IsCardSentWhere",
                        DbType = DbType.Boolean,
                        Value = false,
                    });


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

                    if (spoil.Toughness >= 0)
                    {
                        cmd.Parameters.Add(new MySqlParameter()
                        {
                            ParameterName = "@Toughness",
                            DbType = DbType.VarNumeric,
                            Value = spoil.Toughness,
                        });
                    }
                    else
                    {
                        cmd.Parameters.Add(new MySqlParameter()
                        {
                            ParameterName = "@Toughness",
                            DbType = DbType.VarNumeric,
                            Value = null,
                        });
                    }

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

                    cmd.ExecuteNonQuery();
                    ret = (Int32)cmd.LastInsertedId;
                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                return ret;
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
            #endregion
        }
    }
}