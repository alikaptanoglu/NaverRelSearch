using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Naver.SearchAd;

namespace Winner
{
    public class SQLite
    {
        private SQLiteConnection dbConnection;
        private const String DbFile = "NaverRelateSearch.sqlite";
        private static SQLite sqLite = new SQLite();
        
        public static SQLite GetInstance()
        {
            return sqLite;
        }

        public void CreateSQLiteDB(string name = DbFile)
        {
            if (!System.IO.File.Exists(DbFile))
            {
                SQLiteConnection.CreateFile(name);
            }
        }

        internal List<Configuration> SelectAllConfigurationByOwner(string owner)
        {
            ConnectionToDB();

            List<Configuration> Configs = new List<Configuration>();
            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from Configuration where owner = '{1}'", Configuration.Column, owner));

            int index = 0;
            while (reader.Read())
            {
                Configuration Config = new Configuration();

                Config.key = (string)reader.GetValue(index++);
                Config.value= (string)reader.GetValue(index++);
                Config.description = (string)reader.GetValue(index++);
                Config.owner = (string)reader.GetValue(index++);
                Configs.Add(Config);
                index = 0;
            }

            DisconnectionToDB();

            return Configs;
        }

        internal List<LogicItem> SelectAllLogicItemsByLogicId(string id)
        {
            ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from {1} where logicId = {2} order by sequence asc", LogicItem.Column, LogicItem.TableName, id));
            List<LogicItem> logicItems = LogicItem.MakeResultSet(reader);

            DisconnectionToDB();

            return logicItems;
        }

        internal List<LogicInput> SelectAllLogicInputsByLogicId(string id)
        {
            ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from {1} where logicId = {2}", LogicInput.Column, LogicInput.TableName, id));
            List<LogicInput> logicInputs = LogicInput.MakeResultSet(reader);

            DisconnectionToDB();

            return logicInputs;
        }

        internal List<RankingModel> SelectAllRanking()
        {
            ConnectionToDB();
            
            SQLiteDataReader reader = SelectExecuteSQL( string.Format("select * from ranking", RankingModel.Column));
            List <RankingModel> rankings = RankingModel.MakeResultSet(reader);

            DisconnectionToDB();
            return rankings;
        }

        public void ConnectionToDB()
        {
            if (dbConnection == null)
            {
                dbConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", DbFile));
            }
            
            dbConnection.Open();
        }

        public bool ExistAccount(string account, string password)
        {
            ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select * from account where account = '{0}' and password = '{1}'", account, password));

            int count = 0;
            while (reader.Read())
            {
                count++;
            }

            DisconnectionToDB();

            return count > 0 ? true : false;
        }

        private SQLiteDataReader SelectExecuteSQL(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            return command.ExecuteReader();
        }

        public void DisconnectionToDB()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }

        public void CreateTable(string tableName = null)
        {
            ConnectionToDB();

            ExecuteSQL("create table if not exists Configuration (key varchar(256), value varchar(4000))");

            DisconnectionToDB();
        }

        public void InsetAllConfiguration(List<Configuration> configs)
        {
            ConnectionToDB();

            using (var command = new SQLiteCommand(dbConnection))
            {
                using (var transaction = dbConnection.BeginTransaction())
                {
                    // 100,000 inserts                
                    for (var i = 0; i < configs.Count; i++)
                    {
                        Configuration config = configs.ElementAt(i);

                        object[] param = { config.key, config.value, config.description, config.owner};
                        command.CommandText =
                            string.Format("insert into  Configuration (" + Configuration.Column + ") values (" + Configuration.Values + ")", param);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            DisconnectionToDB();
        }

        internal void DeleteLogicInputByLogicId(string logicId)
        {
            ConnectionToDB();
            ExecuteSQL(string.Format("delete from {0} where LogicId = '{1}'", LogicInput.TableName, logicId));
            DisconnectionToDB();
        }

        internal void InsertAllLogicInpts(List<LogicInput> logicInputs)
        {
            ConnectionToDB();

            using (var command = new SQLiteCommand(dbConnection))
            {
                using (var transaction = dbConnection.BeginTransaction())
                {
                    // 100,000 inserts                
                    for (var i = 0; i < logicInputs.Count; i++)
                    {
                        LogicInput logicInput = logicInputs.ElementAt(i);                                                
                        command.CommandText =
                            string.Format("insert into "+ LogicInput.TableName +" (" + LogicInput.Column + ") values (" + LogicInput.Values + ")", CommonUtils.MakeArray(logicInput));
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            DisconnectionToDB();
        }

        internal void InsertAllLogicItems(List<LogicItem> logicItems)
        {
            ConnectionToDB();

            using (var command = new SQLiteCommand(dbConnection))
            {
                using (var transaction = dbConnection.BeginTransaction())
                {
                    // 100,000 inserts                
                    for (var i = 0; i < logicItems.Count; i++)
                    {
                        LogicItem logicItem = logicItems.ElementAt(i);
                        command.CommandText =
                            string.Format("insert into " + LogicItem.TableName + " (" + LogicItem.Column + ") values (" + LogicItem.Values + ")", CommonUtils.MakeArray( logicItem));
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            DisconnectionToDB();
        }

        internal void DeleteAllLogicItems(string logicId)
        {
            ConnectionToDB();
            ExecuteSQL(string.Format("delete from {0} where LogicId = '{1}'", LogicItem.TableName, logicId));
            DisconnectionToDB();
        }

        public void DeleteAllConfigurationByOwner(string owner)
        {
            ConnectionToDB();
            ExecuteSQL(string.Format("delete from Configuration where Owner = '{0}'", owner));
            DisconnectionToDB();
        }

        public void ExecuteSQL( string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
        }

        public void InsertSlot(Slot slot)
        {
            ConnectionToDB();
            object[] param = { slot.OID, slot.category, slot.search, slot.nxSearch, slot.toCount, slot.currCount, slot.View, slot.initRank, slot.currRank, slot.createdAt, slot.agent, slot.browser};
            ExecuteSQL(string.Format("insert into Slot values ("+ Slot.Values + ")", param));                
            DisconnectionToDB();
            
        }

        public void DeleteAllTables( string tableName)
        {
            ConnectionToDB();
            ExecuteSQL( string.Format("delete from {0}", tableName));
            DisconnectionToDB();
        }

        public void InsertAllSlots(List<Slot> slots)
        {
            ConnectionToDB();

            using (var command = new SQLiteCommand(dbConnection))
            {
                using (var transaction = dbConnection.BeginTransaction())
                {
                    // 100,000 inserts
                    for (var i = 0; i < slots.Count; i++)
                    {
                        Slot slot = slots.ElementAt(i);
                        object[] param = { slot.OID, slot.category, slot.search, slot.nxSearch, slot.toCount, slot.currCount, slot.View, slot.initRank, slot.currRank, slot.createdAt, slot.agent, slot.browser };
                        command.CommandText =
                            string.Format( "insert into  Slot ("+ Slot.Column + ") values ("+ Slot.Values+")", param);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            DisconnectionToDB();
        }

        public void InsertAllRankings(List<RankingModel> rankingModels)
        {
            ConnectionToDB();

            using (var command = new SQLiteCommand(dbConnection))
            {
                using (var transaction = dbConnection.BeginTransaction())
                {
                    // 100,000 inserts
                    for (var i = 0; i < rankingModels.Count; i++)
                    {
                        RankingModel rankingModel = rankingModels.ElementAt(i);
                                            
                        command.CommandText =
                            string.Format("insert into  "+ RankingModel.TableName +" (" + RankingModel.Column + ") values (" + RankingModel.Values + ")", CommonUtils.MakeArray(rankingModel));
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            DisconnectionToDB();
        }

        public List<Logic> SelectAllLogics()
        {
            ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select * from " + Logic.TableName, Logic.Column));
            List<Logic> logics = Logic.MakeResultSet(reader);

            DisconnectionToDB();
            return logics;
        }

        public List<Slot> SelectAllSlots()
        {
            ConnectionToDB();
            List<Slot> Slots = new List<Slot>();
            SQLiteDataReader reader = SelectExecuteSQL( string.Format( "select {0} from slot", Slot.Column));

            
            while( reader.Read())
            {
                Slot slot = new Slot();                
                slot.OID = (string)reader["OID"];
                slot.category = (string)reader["Category"];
                slot.search = (string)reader["Search"];
                slot.nxSearch = (string)reader["NxSearch"];
                slot.toCount = (string)reader["ToCount"];
                slot.currCount = (string)reader["CurrCount"];
                slot.View = (string)reader["View"];
                slot.initRank = (string)reader["InitRank"];
                slot.currRank = (string)reader["CurrRank"];
                slot.createdAt = (string)reader["CreatedAt"];
                slot.agent = (string)reader["Agent"];
                slot.browser = (string)reader["Browser"];

                Slots.Add(slot);
                
            }
            
            DisconnectionToDB();

            return Slots;
        }
    }
}
