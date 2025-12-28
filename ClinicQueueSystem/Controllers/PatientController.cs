using ClinicQueueSystem.Data;
using ClinicQueueSystem.Models;
using ClinicQueueSystem.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ClinicQueueSystem.Controllers
{
    /// <summary>
    /// PatientController handles patient-related operations
    /// Including queue viewing, patient serving, and real-time updates via SignalR
    /// </summary>
    public class PatientController : Controller
    {
        // Database context for patient data access
        private readonly ApplicationDbContext _context;
        // SignalR hub for real-time queue updates
        private readonly IHubContext<QueueHub> _hubContext;

        /// <summary>
        /// Constructor - Dependency injection of database context and SignalR hub
        /// </summary>
        /// <param name="context">Entity Framework database context</param>
        /// <param name="hubContext">SignalR hub for real-time communication</param>
        public PatientController(ApplicationDbContext context, IHubContext<QueueHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Index action - Displays the live patient queue with intelligent sorting
        /// Shows current queue status with priority-based ordering
        /// </summary>
        /// <returns>View with prioritized patient queue</returns>
        public async Task<IActionResult> Index()
        {
            // Retrieve unserved patients with intelligent priority sorting:
            // 1. Emergency patients first (highest priority)
            // 2. Senior patients (65+) second 
            // 3. Regular patients by queue number (first-come-first-served)
            var queue = await _context.Patients
                .Where(p => !p.IsServed)                          // Only unserved patients
                .OrderByDescending(p => p.IsEmergency)            // Emergency patients first
                .ThenByDescending(p => p.Age >= 65)               // Then senior patients (65+)
                .ThenBy(p => p.QueueNumber)                       // Then by registration order
                .ToListAsync();

            return View(queue);
        }

        /// <summary>
        /// GetQueue action - Returns partial view for AJAX queue updates
        /// Used by SignalR for real-time queue refreshing without page reload
        /// </summary>
        /// <returns>Partial view with current queue data</returns>
        [HttpGet]
        public async Task<IActionResult> GetQueue()
        {
            // Same intelligent sorting as Index action
            var queue = await _context.Patients
                .Where(p => !p.IsServed)
                .OrderByDescending(p => p.IsEmergency)
                .ThenByDescending(p => p.Age >= 65)
                .ThenBy(p => p.QueueNumber)
                .ToListAsync();
            
            // Return partial view for AJAX updates
            return PartialView("_QueueList", queue);
        }

        /// <summary>
        /// Confirmation action - Displays patient registration confirmation
        /// Shows success message with queue number after registration
        /// </summary>
        /// <param name="id">Patient ID for confirmation display</param>
        /// <returns>Confirmation view with patient details</returns>
        public async Task<IActionResult> Confirmation(int id)
        {
            // Find patient by ID
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            // Create confirmation message with queue number
            var viewModel = new ConfirmationViewModel
            {
                Message = $"Patient {patient.Name} has been successfully registered with Queue Number {patient.QueueNumber}."
            };

            return View(viewModel);
        }

        /// <summary>
        /// ServePatient action - Marks a patient as served and updates queue
        /// Includes real-time notification to all connected clients via SignalR
        /// </summary>
        /// <param name="id">Patient ID to mark as served</param>
        /// <returns>Redirect to queue index with updated data</returns>
        [HttpPost]
        public async Task<IActionResult> ServePatient(int id)
        {
            // Find patient by ID
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            // Mark patient as served
            patient.IsServed = true;
            await _context.SaveChangesAsync();

            // REAL-TIME UPDATE: Notify all connected clients about queue change
            // This triggers automatic queue refresh on all browsers viewing the queue
            await _hubContext.Clients.All.SendAsync("QueueUpdated");

            // Redirect back to queue view
            return RedirectToAction(nameof(Index));
        }
    }
}
