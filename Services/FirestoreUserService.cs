using Google.Cloud.Firestore;

namespace ImbUserManagment2.Services
{
    public class FirestoreUserService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreUserService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var snapshot = await _firestoreDb
                .Collection("userDetails") 
                .WhereEqualTo("email", email)
                .GetSnapshotAsync();

            return snapshot.Documents.Any();
        }
    }
}