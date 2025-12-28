using Microsoft.AspNetCore.Mvc;
using ClinicQueueSystem.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using ClinicQueueSystem.Models;
using Microsoft.AspNetCore.Http;

namespace ClinicQueueSystem.Controllers
{
    /// <summary>
    /// AdminController handles all administrative functions for the clinic queue system
    /// Including dashboard analytics, patient registration, and admin authentication
    /// </summary>
    public class AdminController : Controller
    {
        // Database context for accessing patient and admin data
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor - Dependency injection of database context
        /// </summary>
        /// <param name="context">Entity Framework database context</param>
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Dashboard action - Displays comprehensive statistics and analytics
        /// Shows today's patient data, wait times, emergency cases, and queue status
        /// </summary>
        /// <returns>Dashboard view with statistics</returns>
        public async Task<IActionResult> Dashboard()
        {
            // Get today's date for filtering patient data
            var today = DateTime.Today;

            // Retrieve all patients registered today from database
            var patientsToday = await _context.Patients
                .Where(p => p.RegistrationTime.Date == today)
                .ToListAsync();

            // Calculate key statistics for dashboard display
            var totalRegistered = patientsToday.Count;                    // Total patients registered today
            var servedToday = patientsToday.Count(p => p.IsServed);       // Patients who have been served
            var waitingInQueue = totalRegistered - servedToday;           // Patients still waiting
            var emergencyCount = patientsToday.Count(p => p.IsEmergency); // Emergency cases

            // Calculate average wait time for served patients
            var avgWaitTime = patientsToday
                .Where(p => p.IsServed)
                .Select(p => (DateTime.Now - p.RegistrationTime).TotalMinutes)
                .DefaultIfEmpty(0)
                .Average();

            // Find the peak hour (hour with most registrations)
            var peakHour = patientsToday
                .GroupBy(p => p.RegistrationTime.Hour)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            // Pass all statistics to the view using ViewBag
            ViewBag.TotalRegistered = totalRegistered;
            ViewBag.ServedToday = servedToday;
            ViewBag.WaitingInQueue = waitingInQueue;
            ViewBag.EmergencyCount = emergencyCount;
            ViewBag.AvgWaitTime = Math.Round(avgWaitTime, 2);
            ViewBag.PeakHour = peakHour;

            return View();
        }

        /// <summary>
        /// PatientsServedToday action - Displays list of all patients served today
        /// Used for reviewing completed consultations and daily service summary
        /// </summary>
        /// <returns>View with list of served patients</returns>
        public async Task<IActionResult> PatientsServedToday()
        {
            var today = DateTime.Today;

            // Query database for patients who have been served today
            var patientsServedToday = await _context.Patients
                .Where(p => p.IsServed && p.RegistrationTime.Date == today)
                .ToListAsync();

            return View(patientsServedToday);
        }

        /// <summary>
        /// Register GET action - Displays admin registration form
        /// Used for creating new admin accounts
        /// </summary>
        /// <returns>Admin registration view</returns>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Register POST action - Processes admin registration form submission
        /// Creates new admin account in the database
        /// </summary>
        /// <param name="admin">Admin model with registration details</param>
        /// <returns>Redirect to dashboard on success, or back to form on failure</returns>
        [HttpPost]
        public async Task<IActionResult> Register(Admin admin)
        {
            // Validate the admin model
            if (ModelState.IsValid)
            {
                // Add new admin to database
                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();
                
                // Redirect to dashboard after successful registration
                return RedirectToAction("Dashboard", "Admin");
            }

            // Return to registration form if validation fails
            return View(admin);
        }

        /// <summary>
        /// Login GET action - Displays admin login form
        /// Entry point for admin authentication
        /// </summary>
        /// <returns>Login view</returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Login POST action - Processes admin login credentials
        /// Authenticates admin and creates session
        /// </summary>
        /// <param name="username">Admin username</param>
        /// <param name="password">Admin password</param>
        /// <returns>Redirect to dashboard on success, or back to login with error</returns>
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Query database for admin with matching credentials
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

            if (admin != null)
            {
                // Create session for authenticated admin
                HttpContext.Session.SetString("AdminUsername", admin.Username);
                return RedirectToAction("Dashboard");
            }

            // Display error message for invalid credentials
            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        /// <summary>
        /// Logout action - Ends admin session and redirects to login
        /// Clears all session data for security
        /// </summary>
        /// <returns>Redirect to login page</returns>
        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// RegisterPatient GET action - Displays patient registration form
        /// Used by admin to register new patients with auto-serve options
        /// </summary>
        /// <returns>Patient registration view</returns>
        public IActionResult RegisterPatient()
        {
            return View();
        }

        /// <summary>
        /// RegisterPatient POST action - Processes patient registration with advanced features
        /// Implements auto-serve logic for emergency and senior patients
        /// Includes comprehensive error handling and validation
        /// </summary>
        /// <param name="patient">Patient model with registration details</param>
        /// <param name="serveImmediately">Optional parameter to manually mark patient as served</param>
        /// <returns>Redirect to dashboard with success/error message</returns>
        [HttpPost]
        public async Task<IActionResult> RegisterPatient(Patient patient, bool serveImmediately = false)
        {
            try
            {
                // Remove Password validation since it's not needed for admin registration
                ModelState.Remove("Password");
                
                if (ModelState.IsValid)
                {
                    // Set registration timestamp
                    patient.RegistrationTime = DateTime.Now;
                    patient.Password = null; // Password not required for admin-registered patients
                    
                    // Generate next queue number for today
                    var lastQueueNumber = await _context.Patients
                        .Where(p => p.RegistrationTime.Date == DateTime.Today)
                        .MaxAsync(p => (int?)p.QueueNumber) ?? 0;
                    
                    patient.QueueNumber = lastQueueNumber + 1;
                    
                    // SMART AUTO-SERVE LOGIC
                    // Automatically serve patients based on priority rules:
                    bool shouldAutoServe = patient.IsEmergency ||    // 1. Emergency patients (immediate attention)
                                         patient.Age >= 65 ||        // 2. Senior patients (65+ years old)
                                         serveImmediately;           // 3. Manually marked by admin
                    
                    patient.IsServed = shouldAutoServe;

                    // Save patient to database
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();

                    // Generate detailed success message with reason for auto-serving
                    string statusMessage = shouldAutoServe ? "served immediately" : "added to queue";
                    string reason = "";
                    
                    if (patient.IsEmergency)
                        reason = " (Emergency case - auto-served)";
                    else if (patient.Age >= 65)
                        reason = " (Senior patient - auto-served)";
                    else if (serveImmediately)
                        reason = " (Manually marked as served)";

                    // Set success message for dashboard display
                    TempData["SuccessMessage"] = $"Patient {patient.Name} registered successfully with Queue Number {patient.QueueNumber} and {statusMessage}{reason}";
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    // Handle validation errors - collect all error messages
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["ErrorMessage"] = $"Validation failed: {errors}";
                }
            }
            catch (Exception ex)
            {
                // Handle any database or system errors
                TempData["ErrorMessage"] = $"Error registering patient: {ex.Message}";
            }

            // Return to dashboard with error message if registration fails
            return RedirectToAction("Dashboard");
        }
    }
}
