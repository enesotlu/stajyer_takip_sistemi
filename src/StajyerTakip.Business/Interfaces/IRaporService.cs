using StajyerTakip.Business.Models;

namespace StajyerTakip.Business.Interfaces;

public interface IRaporService
{
    Task<RaporOzeti> GetOzetAsync();
}
