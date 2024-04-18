using WebTeam.Data;

namespace WebTeam.Services
{
    public interface ICommentsService
    {
        Task Add(Comment comment);
    }
}
