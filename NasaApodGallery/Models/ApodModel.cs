using System;

namespace NasaApodGallery.Models
{
    public class ApodModel
    {
      public int Id {get; set;}
        public DateTime ApodDate { get; set; } 
        public string Title { get; set; } 
        public string Explanation { get; set; } 
        public string Url { get; set; } 
        public string MediaType { get; set; } 
        public string ServiceVersion { get; set; } 
       public DateTime SavedAt {get;set;} 
    }
}