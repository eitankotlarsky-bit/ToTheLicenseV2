using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                                    "Source=..\\..\\..\\ViewModel\\DataBase\\Database11.accdb;Persist " +
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
            //this.command.CommandText = "SELECT *, tblPeople.id AS ID FROM (tblPeople INNER JOIN tblStudent ON tblPeople.id = tblStudent.id)";
            List<BaseEntity> list = new List<BaseEntity>();

            try
            {
                this.command.Connection = connection;
                this.connection.Open();

                this.reader = command.ExecuteReader();

                BaseEntity entity;

                while (this.reader.Read())
                {
                    entity = NewEntity();

                    this.CreateModel(entity);

                    list.Add(entity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\nSQL: " + command.CommandText);
            }
            finally
            {
                if (this.reader != null)
                    this.reader.Close();

                if (this.connection.State == ConnectionState.Open)
                    this.connection.Close();
            }

            return list;

        }


        //public virtual void Insert(BaseEntity entity)
        //{
        //    BaseEntity reqEntity = this.NewEntity();

        //    if (entity != null && entity.GetType() == reqEntity.GetType())
        //        this.inserted.Add(new ChangeEntity(this.CreateInsertSQL,entity));
        //}

        //public virtual void Update(BaseEntity entity)
        //{
        //    BaseEntity reqEntity = this.NewEntity();

        //    if (entity != null && entity.GetType() == reqEntity.GetType())
        //        this.updated.Add(new ChangeEntity(this.CreateUpdateSQL, entity));
        //}
        //public virtual void Delete(BaseEntity entity)
        //{
        //    BaseEntity reqEntity = this.NewEntity();

        //    if (entity != null && entity.GetType() == reqEntity.GetType())
        //        this.deleted.Add(new ChangeEntity(this.CreateDeleteSQL, entity));
        //}

        public abstract string CreateInsertSQL(BaseEntity entity);
        public abstract string CreateUpdateSQL(BaseEntity entity);
        public abstract string CreateDeleteSQL(BaseEntity entity);

        //public int SaveChanges()
        //{
        //    int records = 0;
        //    OleDbCommand cmd = new OleDbCommand();

        //    try
        //    {
        //        cmd.Connection = this.connection;                
        //        this.connection.Open();

        //        foreach (ChangeEntity item in this.inserted)
        //        {
        //            cmd.CommandText = item.CreateSQL(item.Entity);
        //            records += cmd.ExecuteNonQuery();

        //            cmd.CommandText = "Select @@Identity";
        //            item.Entity.Id = (int)cmd.ExecuteScalar();
        //        }

        //        foreach (ChangeEntity item in this.updated)
        //        {
        //            cmd.CommandText = item.CreateSQL(item.Entity);
        //            records += cmd.ExecuteNonQuery();
        //        }

        //        foreach (ChangeEntity item in this.deleted)
        //        {
        //            cmd.CommandText = item.CreateSQL(item.Entity);
        //            records += cmd.ExecuteNonQuery();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.Write(ex.Message + "\nSQL: " + cmd.CommandText);
        //    }
        //    finally
        //    {
        //        inserted.Clear();
        //        updated.Clear();
        //        deleted.Clear();

        //        if (this.connection.State == ConnectionState.Open)
        //            this.connection.Close();
        //    }

        //    return records;
        //}
    }
}
