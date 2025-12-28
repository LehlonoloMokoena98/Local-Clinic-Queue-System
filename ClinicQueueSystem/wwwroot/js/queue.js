// ====================================================================
// REAL-TIME QUEUE UPDATES WITH SIGNALR
// ====================================================================
// This JavaScript file handles real-time communication between the server and client
// Automatically updates the patient queue when changes occur (new patients, served patients)

// Establish SignalR connection to the server hub
// This creates a persistent WebSocket connection for real-time updates
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/queueHub")  // Connect to the QueueHub endpoint defined in Program.cs
    .build();

// ====================================================================
// EVENT HANDLERS
// ====================================================================

// Listen for "QueueUpdated" events from the server
// This event is triggered whenever:
// - A new patient is registered
// - A patient is marked as served
// - Any queue-related change occurs
connection.on("QueueUpdated", () => {
    // When queue update is received, fetch the latest queue data
    // This makes an AJAX call to get updated queue without full page reload
    fetch('/Patient/GetQueue')
        .then(response => response.text())    // Get HTML response
        .then(html => {
            // Update the queue display area with new data
            // This replaces the content of the element with id "queueList"
            document.getElementById("queueList").innerHTML = html;
        });
});

// ====================================================================
// CONNECTION STARTUP
// ====================================================================

// Start the SignalR connection
// Once connected, the client can receive real-time updates from the server
connection.start().then(() => {
    console.log("Connected to QueueHub - Real-time updates enabled");
}).catch(err => {
    console.error("SignalR connection failed:", err);
});
