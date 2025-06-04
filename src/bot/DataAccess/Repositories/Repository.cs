using TelegramForwardly.DataAccess.Context;

namespace TelegramForwardly.DataAccess.Repositories
{
    public abstract class Repository(ForwardlyContext context)
    {
        protected readonly ForwardlyContext context = context;
    }
}
