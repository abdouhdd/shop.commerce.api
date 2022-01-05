using shop.commerce.api.services.Helpers;

namespace shop.commerce.api.services.Services.Impl
{
    public class BaseService 
    {
        public BaseService(MessagesHelper messagesHelper)
        {
            MessagesHelper = messagesHelper;
        }

        protected MessagesHelper MessagesHelper { get; }
    }
}
