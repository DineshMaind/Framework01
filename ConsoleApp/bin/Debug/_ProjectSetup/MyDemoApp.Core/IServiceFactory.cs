namespace MyDemoApp.Core
{
    public interface IServiceFactory
    {
        IRepository GetDatabaseObject();
    }
}