namespace AzureBootCampTickets.Contracts.Services
{
    public interface IIdentiService
    {
        string GetUserId();
        string GetUserEmail();
    }
}