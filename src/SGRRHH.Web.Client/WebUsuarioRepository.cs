using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebUsuarioRepository : WebFirestoreRepository<Usuario>, IUsuarioRepository
{
    public WebUsuarioRepository(FirebaseJsInterop firebase) 
        : base(firebase, "users") // Nota: La colección se llama "users" en Firestore
    {
    }

    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "username", "==", username);
        return results.Select(MapToEntity).FirstOrDefault();
    }

    public async Task<bool> ExistsUsernameAsync(string username)
    {
        var user = await GetByUsernameAsync(username);
        return user != null;
    }
}
