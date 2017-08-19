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

        internal Slot SelectSlotBySlotId(string slotId)
        {
            SQLiteConnection conn = ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from {1} where oid = '{2}'", Slot.Column, Slot.TableName, slotId), conn);
            List<Slot> slots = Slot.MakeResultSet(reader);

            DisconnectionToDB(conn);
            return slots[0];
        }

        internal List<Configuration> SelectAllConfigurationByOwner(string owner)
        {
            SQLiteConnection conn = ConnectionToDB();

            List<Configuration> Configs = new List<Configuration>();
            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from Configuration where owner = '{1}'", Configuration.Column, owner), conn);

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

            DisconnectionToDB(conn);

            return Configs;
        }

        internal List<LogicItem> SelectAllLogicItemsByLogicId(string id)
        {
            SQLiteConnection conn = ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from {1} where logicId = '{2}' order by sequence asc", LogicItem.Column, LogicItem.TableName, id), conn);
            List<LogicItem> logicItems = LogicItem.MakeResultSet(reader);

            DisconnectionToDB(conn);

            return logicItems;
        }

        internal List<LogicInput> SelectAllLogicInputsByLogicId(string id)
        {
            SQLiteConnection conn = ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from {1} where logicId = '{2}'", LogicInput.Column, LogicInput.TableName, id), conn);
            List<LogicInput> logicInputs = LogicInput.MakeResultSet(reader);

            DisconnectionToDB(conn);

            return logicInputs;
        }

        internal List<LogicItem> SelectAllLogicItemsBySlotId(string slotId)
        {
            SQLiteConnection conn = ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from {1} l, {2} s where  l.logicid = s.logicid and s.oid = '{3}' order by sequence asc;", LogicItem.GetPrepixColumn("l"), LogicItem.TableName, Slot.TableName, slotId), conn);
            List<LogicItem> logicItems = LogicItem.MakeResultSet(reader);

            DisconnectionToDB( conn);

            return logicItems;
        }

        private void DisconnectionToDB(SQLiteConnection conn)
        {
            if (conn != null)
            {
                conn.Close();
            }
        }

        internal List<RankingModel> SelectAllRanking()
        {
            SQLiteConnection conn = ConnectionToDB();
            
            SQLiteDataReader reader = SelectExecuteSQL( string.Format("select * from ranking", RankingModel.Column), conn);
            List <RankingModel> rankings = RankingModel.MakeResultSet(reader);

            DisconnectionToDB(conn);
            return rankings;
        }

        public SQLiteConnection ConnectionToDB()
        {            
            SQLiteConnection dbConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3;Pooling=False;Max Pool Size=100;", DbFile));
            dbConnection.Open();
            return dbConnection;            
        }

        public bool ExistAccount(string account, string password)
        {
            SQLiteConnection conn = ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select * from account where account = '{0}' and password = '{1}'", account, password), conn);

            int count = 0;
            while (reader.Read())
            {
                count++;
            }

            DisconnectionToDB(conn);

            return count > 0 ? true : false;
        }

        private SQLiteDataReader SelectExecuteSQL(string sql, SQLiteConnection conn)
        {
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            return command.ExecuteReader();
        }


        public void CreateTable(string tableName = null)
        {
            SQLiteConnection conn = ConnectionToDB();

            ExecuteSQL("create table if not exists Configuration (key varchar(256), value varchar(4000))", conn);

            DisconnectionToDB(conn);
        }

        public void InsetAllConfiguration(List<Configuration> configs)
        {
            SQLiteConnection conn = ConnectionToDB();

            using (var command = new SQLiteCommand(conn))
            {
                using (var transaction = conn.BeginTransaction())
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

            DisconnectionToDB(conn);
        }

        internal void DeleteLogicInputByLogicId(string logicId)
        {
            SQLiteConnection conn = ConnectionToDB();
            ExecuteSQL(string.Format("delete from {0} where LogicId = '{1}'", LogicInput.TableName, logicId), conn);
            DisconnectionToDB(conn);
        }

        internal void InsertAllLogicInpts(List<LogicInput> logicInputs)
        {
            SQLiteConnection conn = ConnectionToDB();

            using (var command = new SQLiteCommand(conn))
            {
                using (var transaction = conn.BeginTransaction())
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

            DisconnectionToDB(conn);
        }

        internal void InsertAllLogicItems(List<LogicItem> logicItems)
        {
            SQLiteConnection conn = ConnectionToDB();

            using (var command = new SQLiteCommand(conn))
            {
                using (var transaction = conn.BeginTransaction())
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

            DisconnectionToDB(conn);
        }

        internal void DeleteLogicItemByLogicId(string logicId)
        {
            SQLiteConnection conn = ConnectionToDB();
            ExecuteSQL(string.Format("delete from {0} where LogicId = '{1}'", LogicItem.TableName, logicId), conn);
            DisconnectionToDB(conn);
        }

        public void DeleteAllConfigurationByOwner(string owner)
        {
            SQLiteConnection conn = ConnectionToDB();
            ExecuteSQL(string.Format("delete from Configuration where Owner = '{0}'", owner), conn);
            DisconnectionToDB(conn);
        }

        public void ExecuteSQL( string sql, SQLiteConnection conn)
        {
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void InsertSlot(Slot slot)
        {
            SQLiteConnection conn = ConnectionToDB();            
            ExecuteSQL(string.Format("insert into Slot (" + Slot.Column + ") values ("+ Slot.Values + ")", CommonUtils.MakeArray(slot)), conn);                
            DisconnectionToDB(conn);
            
        }

        public void DeleteAllTables( string tableName)
        {
            SQLiteConnection conn = ConnectionToDB();
            ExecuteSQL( string.Format("delete from {0}", tableName), conn);
            DisconnectionToDB(conn);
        }

        public void InsertAllSlots(List<Slot> slots)
        {
            SQLiteConnection conn = ConnectionToDB();

            using (var command = new SQLiteCommand(conn))
            {
                using (var transaction = conn.BeginTransaction())
                {
                    // 100,000 inserts
                    for (var i = 0; i < slots.Count; i++)
                    {
                        Slot slot = slots.ElementAt(i);                                                
                        command.CommandText =
                            string.Format( "insert into  Slot ("+ Slot.Column + ") values ("+ Slot.Values+")", CommonUtils.MakeArray(slot));
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            DisconnectionToDB(conn);
        }

        public void InsertAllRankings(List<RankingModel> rankingModels)
        {
            SQLiteConnection conn = ConnectionToDB();

            using (var command = new SQLiteCommand(conn))
            {
                using (var transaction = conn.BeginTransaction())
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

            DisconnectionToDB(conn);
        }

        public List<Logic> SelectAllLogics()
        {
            SQLiteConnection conn = ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from {1} order by createdAt asc",Logic.Column, Logic.TableName), conn);
            List<Logic> logics = Logic.MakeResultSet(reader);

            DisconnectionToDB(conn);
            return logics;
        }

        public List<Logic> SelectLogicByType( string type)
        {
            SQLiteConnection conn = ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from {1} where type = '{2}' order by createdAt asc", Logic.Column, Logic.TableName, type), conn);
            List<Logic> logics = Logic.MakeResultSet(reader);

            DisconnectionToDB(conn);
            return logics;
        }

        public List<Slot> SelectAllSlots()
        {
            SQLiteConnection conn = ConnectionToDB();
            List<Slot> Slots = new List<Slot>();
            SQLiteDataReader reader = SelectExecuteSQL( string.Format( "select {0} from slot", Slot.Column), conn);

            
            while( reader.Read())
            {
                Slot slot = new Slot();                
                slot.OID = (string)reader["OID"];
                slot.logicName = (string)reader["LogicName"];
                slot.createdAt = (string)reader["CreatedAt"];
                slot.toCount = (string)reader["ToCount"];
                slot.currCount = (string)reader["CurrCount"];
                slot.rank = (string)reader["Rank"];
                slot.description = (string)reader["Description"];

                Slots.Add(slot);
                
            }
            
            DisconnectionToDB(conn);

            return Slots;
        }
        
        internal void InsertLogic(Logic logic)
        {
            SQLiteConnection conn = ConnectionToDB();
            ExecuteSQL(string.Format("insert into "+ Logic.TableName + " values (" + Logic.Values + ")", CommonUtils.MakeArray( logic)), conn);
            DisconnectionToDB(conn);
        }

        internal void DeleteLogicByLogicId(string logicId)
        {
            SQLiteConnection conn = ConnectionToDB();
            ExecuteSQL(string.Format("delete from {0} where id = '{1}'", Logic.TableName, logicId), conn);
            DisconnectionToDB(conn);
        }

        internal void DeleteSlot(string slotId)
        {
            SQLiteConnection conn = ConnectionToDB();
            ExecuteSQL(string.Format("delete from {0} where oid = '{1}'", Slot.TableName, slotId), conn);
            DisconnectionToDB(conn);
        }

        internal Logic SelectLogicById(string id)
        {
            SQLiteConnection conn = ConnectionToDB();

            SQLiteDataReader reader = SelectExecuteSQL(string.Format("select {0} from {1} where id = '{2}'", Logic.Column, Logic.TableName, id), conn);
            List<Logic> logics = Logic.MakeResultSet(reader);

            DisconnectionToDB(conn);
            return logics[0];
        }

        internal void UpdateSlot(Slot slot)
        {
            SQLiteConnection conn = ConnectionToDB();
            ExecuteSQL(string.Format("update Slot Set logicName = '{0}', ToCount = '{1}', Description = '{2}' where oid = '{3}'", slot.logicName, slot.toCount, slot.description, slot.OID), conn);
            DisconnectionToDB(conn);
        }
    }
}

