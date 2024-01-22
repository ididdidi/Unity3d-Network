using System.Threading.Tasks;

namespace ru.ididdidi.Unity3D
{
    public interface IRequest
    {
        Task Send();
        void Cancel();
    }
}