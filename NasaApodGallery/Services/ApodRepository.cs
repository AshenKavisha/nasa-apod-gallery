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

      public void SaveApod(ApodModel apod)
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

         insertCmd.Parameters.AddWithValue("@ApodDate", apod.ApodDate);
         insertCmd.Parameters.AddWithValue("@Title", apod.Title);
         insertCmd.Parameters.AddWithValue("@Explanation", apod.Explanation);
         insertCmd.Parameters.AddWithValue("@Url", apod.Url);
         insertCmd.Parameters.AddWithValue("@MediaType", apod.MediaType);
         insertCmd.Parameters.AddWithValue("@ServiceVersion", apod.ServiceVersion);
         insertCmd.Parameters.AddWithValue("@SavedAt", DateTime.UtcNow);

         insertCmd.ExecuteNonQuery();

      }
   }
}