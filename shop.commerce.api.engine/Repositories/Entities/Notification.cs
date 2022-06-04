using shop.commerce.api.common;
//using System.ComponentModel.DataAnnotations.Schema;

namespace shop.commerce.api.infrastructure.Repositories.Entities
{
    //[NotMapped]
    public class Notification : Entity<string>
    {
        public int AdminId { get; set; }
        public DataNotification Data { get; set; }
        public bool IsViewed { get; set; }

        public override void BuildSearchTerms()
            => SearchTerms = "";
    }

    public class DataNotification
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string ClassName { get; set; }
        public string Icon { get; set; }
        public string Status { get; set; }
    }
}
