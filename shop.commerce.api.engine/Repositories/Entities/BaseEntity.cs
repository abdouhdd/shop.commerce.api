using System;

namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    public class BaseEntity
    {
        public DateTime InsertDate { get; set; }
        public DateTime LastUpdate { get; set; }

        public BaseEntity()
        {
            InsertDate = DateTime.UtcNow;
            LastUpdate = DateTime.UtcNow;
        }
    }
}
