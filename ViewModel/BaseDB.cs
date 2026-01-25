using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using Model;

namespace ViewModel
{
    public abstract class BaseDB
    {
        private string connectionString;
        protected OleDbConnection connection;
        protected OleDbCommand command;
        protected OleDbDataReader reader;

        protected abstract BaseEntity NewEntity();
        protected abstract void CreateModel(BaseEntity entity);

        // רשימות לניהול השינויים (כמו אצל המורה)
        protected List<ChangeEntity> inserted = new List<ChangeEntity>();
        protected List<ChangeEntity> deleted = new List<ChangeEntity>();
        protected List<ChangeEntity> updated = new List<ChangeEntity>();

        protected BaseDB()
        {
            // נתיב חכם שעובד גם במחשב אחר (בודק איפה הקובץ נמצא)
            this.connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data " +
                                    "Source=..\\..\\..\\ViewModel\\DataBase\\Database11 (1).accdb;Persist " +
                                    "Security Info=True";

            this.connection = new OleDbConnection(this.connectionString);
            this.command = new OleDbCommand();
            this.command.Connection = this.connection;
        }

        public List<BaseEntity> Select()
        {
            List<BaseEntity> list = new List<BaseEntity>();
            try
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                this.command.Connection = this.connection;
                this.reader = command.ExecuteReader();

                while (this.reader.Read())
                {
                    BaseEntity entity = NewEntity();
                    this.CreateModel(entity);
                    list.Add(entity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Select: " + ex.Message);
            }
            finally
            {
                if (reader != null && !reader.IsClosed) reader.Close();
                if (connection.State == ConnectionState.Open) connection.Close();
            }
            return list;
        }

        public virtual void Insert(BaseEntity entity)
        {
            if (entity != null) this.inserted.Add(new ChangeEntity(this.CreateInsertSQL, entity));
        }

        public virtual void Update(BaseEntity entity)
        {
            if (entity != null) this.updated.Add(new ChangeEntity(this.CreateUpdateSQL, entity));
        }

        public virtual void Delete(BaseEntity entity)
        {
            if (entity != null) this.deleted.Add(new ChangeEntity(this.CreateDeleteSQL, entity));
        }

        public abstract string CreateInsertSQL(BaseEntity entity);
        public abstract string CreateUpdateSQL(BaseEntity entity);
        public abstract string CreateDeleteSQL(BaseEntity entity);

        // === הלב של המערכת: שמירה עם טרנזקציה ===
        public int SaveChanges()
        {
            int records = 0;
            OleDbTransaction transaction = null;

            try
            {
                if (connection.State != ConnectionState.Open) connection.Open();

                // 1. התחלת טרנזקציה
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                // ביצוע INSERT
                foreach (var item in inserted)
                {
                    command.CommandText = item.CreateSQL(item.Entity);
                    records += command.ExecuteNonQuery();

                    // טריק חשוב: שולפים את ה-ID שנוצר הרגע
                    command.CommandText = "Select @@Identity";
                    try 
                    {
                        var newId = command.ExecuteScalar();
                        if (newId != null) 
                            item.Entity.Id = Convert.ToInt32(newId);
                    }
                    catch { }
                }

                // ביצוע UPDATE
                foreach (var item in updated)
                {
                    command.CommandText = item.CreateSQL(item.Entity);
                    records += command.ExecuteNonQuery();
                }

                // ביצוע DELETE
                foreach (var item in deleted)
                {
                    command.CommandText = item.CreateSQL(item.Entity);
                    records += command.ExecuteNonQuery();
                }

                // 2. אישור השינויים (Commit)
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // 3. ביטול במקרה של שגיאה (Rollback)
                if (transaction != null) transaction.Rollback();
                System.Diagnostics.Debug.WriteLine("Transaction Failed: " + ex.Message);
                records = 0; // מסמנים כישלון
                throw ex; // זורקים את השגיאה ל-UI כדי שתדע מה קרה
            }
            finally
            {
                inserted.Clear(); updated.Clear(); deleted.Clear();
                if (connection.State == ConnectionState.Open) connection.Close();
            }
            return records;
        }
    }
}