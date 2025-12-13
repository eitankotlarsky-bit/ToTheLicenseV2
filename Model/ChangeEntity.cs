using System;

namespace Model
{
    public delegate string CreateSql(BaseEntity entity);

    public class ChangeEntity
    {
        private BaseEntity entity;
        private CreateSql createSQL;

        public ChangeEntity(CreateSql createSql, BaseEntity entity)
        {
            this.createSQL = createSql;
            this.entity = entity;
        }

        public BaseEntity Entity { get => entity; set => entity = value; }
        public CreateSql CreateSQL { get => createSQL; set => createSQL = value; }
    }
}