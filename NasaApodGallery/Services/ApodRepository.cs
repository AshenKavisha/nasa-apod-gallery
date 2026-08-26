using Microsoft.Data.SqlClient;
using NasaApodGallery.Models;

namespace NasaApodGallery.Services
{
   public class ApodRepository
   {
      private readonly string _connectionString;

      public ApodRepository(IConfiguration configuration)
      {
         _connectionString = configuration.GetConnectionString("DefaultConnection");
      }

      public void SaveApod(ApodDto apod)
{
    using var conn = new SqlConnection(_connectionString);
    conn.Open();

    // Step 1: check if this date already exists
    var checkCmd = new SqlCommand(
        "SELECT COUNT(*) FROM ApodEntries WHERE ApodDate = @ApodDate", conn);
    checkCmd.Parameters.AddWithValue("@ApodDate", apod.Date);

    int existingCount = (int)checkCmd.ExecuteScalar();

    if (existingCount > 0)
    {
        return;
    }

    var insertCmd = new SqlCommand(@"INSERT INTO ApodEntries (ApodDate, Title, Explanation, Url, MediaType, ServiceVersion, SavedAt) 
        VALUES (@ApodDate, @Title, @Explanation, @Url, @MediaType, @ServiceVersion, @SavedAt)", conn);

    insertCmd.Parameters.AddWithValue("@ApodDate", apod.Date);
    insertCmd.Parameters.AddWithValue("@Title", apod.Title);
    insertCmd.Parameters.AddWithValue("@Explanation", apod.Explanation);
    insertCmd.Parameters.AddWithValue("@Url", apod.Url);
    insertCmd.Parameters.AddWithValue("@MediaType", apod.MediaType);
    insertCmd.Parameters.AddWithValue("@ServiceVersion", (object)apod.ServiceVersion ?? DBNull.Value);
    insertCmd.Parameters.AddWithValue("@SavedAt", DateTime.UtcNow);

    insertCmd.ExecuteNonQuery();
}
   

   public List<ApodModel> GetAllApods()
      {
         var apods = new List<ApodModel>();

         using var conn = new SqlConnection(_connectionString);
         conn.Open();

         var selectCmd = new SqlCommand ("SELECT Id, ApodDate, Title, Explanation, Url, MediaType, ServiceVersion, SavedAt FROM ApodEntries ORDER BY ApodDate DESC", conn);
 
         using var reader = selectCmd.ExecuteReader();

         while (reader.Read())
         {
             var apod = new ApodModel
             {
                 Id = reader.GetInt32(reader.GetOrdinal("Id")),
               ApodDate = reader.GetDateTime(reader.GetOrdinal("ApodDate")),
               Title = reader.GetString(reader.GetOrdinal("Title")),
               Explanation = reader.GetString(reader.GetOrdinal("Explanation")),
               Url = reader.GetString(reader.GetOrdinal("Url")),
               MediaType = reader.GetString(reader.GetOrdinal("MediaType")),
               ServiceVersion = reader.IsDBNull(reader.GetOrdinal("ServiceVersion")) ? null : reader.GetString(reader.GetOrdinal("ServiceVersion")),
               SavedAt = reader.GetDateTime(reader.GetOrdinal("SavedAt"))
             };

             apods.Add(apod);
         }

         return apods;
      }

   }

}