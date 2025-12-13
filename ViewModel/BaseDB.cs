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

        protected List<ChangeEntity> inserted;
        protected List<ChangeEntity> deleted;
        protected List<ChangeEntity> updated;

        protected BaseDB()
        {

            this.connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data " +
                        "Source=..\\..\\..\\ViewModel\\DataBase\\Database11 (1).accdb;Persist " +
                        "Security Info=True";

            this.connection = new OleDbConnection(this.connectionString);
            this.command = new OleDbCommand();
            this.command.Connection = this.connection;

            inserted = new List<ChangeEntity>();
            deleted = new List<ChangeEntity>();
            updated = new List<ChangeEntity>();
        }

        public List<BaseEntity> Select()
        {
            List<BaseEntity> list = new List<BaseEntity>();

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // כאן הייתה הבעיה של CommandText ריק - בפונקציות היורשות אנחנו חייבים להגדיר אותו
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
                // במקרה של שגיאה קריטית, אנחנו זורקים אותה החוצה כדי שה-UI יציג אותה
                throw ex;
            }
            finally
            {
                if (reader != null && !reader.IsClosed) reader.Close();
                if (connection.State == ConnectionState.Open) connection.Close();
            }

            return list;
        }

        // --- שאר הפונקציות ללא שינוי ---
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

        public int SaveChanges()
        {
            int records = 0;
            try
            {
                if (connection.State != ConnectionState.Open) connection.Open();

                foreach (var item in inserted)
                {
                    command.CommandText = item.CreateSQL(item.Entity);
                    records += command.ExecuteNonQuery();
                    
                    // שליפת ID אוטומטי
                    command.CommandText = "Select @@Identity";
                    try { item.Entity.Id = (int)command.ExecuteScalar(); } catch { }
                }

                foreach (var item in updated)
                {
                    command.CommandText = item.CreateSQL(item.Entity);
                    records += command.ExecuteNonQuery();
                }

                foreach (var item in deleted)
                {
                    command.CommandText = item.CreateSQL(item.Entity);
                    records += command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex; // זורק שגיאה ל-UI
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