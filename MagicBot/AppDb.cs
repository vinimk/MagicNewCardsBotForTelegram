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
    public class Database : IDisposable
    {

        private MySqlConnection _connection;

        #region Constructor
        public Database(String connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
        #endregion

        #region Get Methods
        public async Task<List<SpoilItem>> GetAllSpoils()
        {
            List<SpoilItem> retList = new List<SpoilItem>();
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
            using (MySqlCommand cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT  SpoilItemId,
                                            ifNull(Status,''),
                                            ifNull(Folder,''),
                                            ifNull(Date,''),
                                            ifNull(Message,''),
                                            ifNull(CardUrl,'')
                                            FROM SpoilItem";

                using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        SpoilItem spoil = new SpoilItem();
                        spoil.SpoilItemId = await reader.GetFieldValueAsync<Int32>(0);
                        spoil.Status = await reader.GetFieldValueAsync<String>(1);
                        spoil.Folder = await reader.GetFieldValueAsync<String>(2);
                        spoil.Date = Convert.ToDateTime(await reader.GetFieldValueAsync<String>(3));
                        spoil.Message = await reader.GetFieldValueAsync<String>(4);
                        spoil.CardUrl = await reader.GetFieldValueAsync<String>(5);
                        retList.Add(spoil);
                    }
                }
            }
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
            return retList;
        }

        public async Task<List<Chat>> GetAllChats()
        {
            List<Chat> retList = new List<Chat>();
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
            using (MySqlCommand cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT  ChatId,
                                            ifNull(Title,''),
                                            ifNull(FirstName,''),
                                            ifNull(Type,'')
                                            FROM Chat 
                                            WHERE IsDeleted = FALSE";

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
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
            return retList;
        }

        #endregion

        #region Insert Methods
        public async Task<Int32> InsertSpoil(SpoilItem spoil)
        {
            Int32 ret = -1;
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
            using (MySqlCommand cmd = _connection.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO SpoilItem
                                            (Status,
                                            Folder,
                                            Date,
                                            Message,
                                            CardUrl)
                                    VALUES
                                            (@Status,
                                            @Folder,
                                            @Date,
                                            @Message,
                                            @CardUrl)";

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@Status",
                    DbType = DbType.StringFixedLength,
                    Value = spoil.Status,
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
                    ParameterName = "@Message",
                    DbType = DbType.StringFixedLength,
                    Value = spoil.Message,
                });

                cmd.Parameters.Add(new MySqlParameter()
                {
                    ParameterName = "@CardUrl",
                    DbType = DbType.StringFixedLength,
                    Value = spoil.CardUrl,
                });

                await cmd.ExecuteNonQueryAsync();
                ret = (Int32)cmd.LastInsertedId;
            }
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
            return ret;
        }

        public async Task<Int64> InsertChat(Chat chat)
        {
            Int64 ret = -1;
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
            using (MySqlCommand cmd = _connection.CreateCommand())
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
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
            return ret;
        }
        #endregion
    }
}