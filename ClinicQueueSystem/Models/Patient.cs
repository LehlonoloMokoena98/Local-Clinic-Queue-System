namespace ClinicQueueSystem.Models
{
    /// <summary>
    /// Patient model - Represents a patient in the clinic queue system
    /// Contains all patient information, queue status, and priority indicators
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// Unique identifier for the patient record
        /// Primary key in the database
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Patient's full name (required field)
        /// Used for identification and display in queue
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Patient's age in years
        /// Used for senior patient auto-serve logic (65+ years)
        /// </summary>
        public int Age { get; set; }
        
        /// <summary>
        /// Emergency status flag
        /// True = Emergency patient (gets immediate attention/auto-served)
        /// False = Regular patient (follows normal queue order)
        /// </summary>
        public bool IsEmergency { get; set; }
        
        /// <summary>
        /// Timestamp when patient was registered
        /// Used for calculating wait times and daily statistics
        /// </summary>
        public DateTime RegistrationTime { get; set; }
        
        /// <summary>
        /// Sequential queue number assigned to patient
        /// Generated automatically, resets daily
        /// Used for queue ordering and patient identification
        /// </summary>
        public int QueueNumber { get; set; }
        
        /// <summary>
        /// Service completion status
        /// True = Patient has been served/completed consultation
        /// False = Patient is still waiting in queue
        /// </summary>
        public bool IsServed { get; set; }
        
        /// <summary>
        /// Optional password field (nullable)
        /// Not used in current admin-only registration system
        /// Kept for potential future patient self-registration feature
        /// </summary>
        public string? Password { get; set; }
    }
}