import React, { useState, useEffect } from "react";
import "./PatientDashboard.css";

function PatientDashboard() {
  const [patientId, setPatientId] = useState("");
  const [patient, setPatient] = useState(null);
  const [appointments, setAppointments] = useState([]);
  const [availableTrucks, setAvailableTrucks] = useState([]);
  const [selectedTruck, setSelectedTruck] = useState("");
  const [appointmentDate, setAppointmentDate] = useState("");
  const [availableSlots, setAvailableSlots] = useState([]);
  const [selectedSlot, setSelectedSlot] = useState("");
  const [activeTab, setActiveTab] = useState("overview");
  const [notifications, setNotifications] = useState([]);
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const API_PATIENT = "http://localhost:5178/api/Patient";

  // -------------------- Helpers --------------------
  const showMsg = (msg) => {
    setMessage(msg);
    setTimeout(() => setMessage(""), 4000);
  };

  const handleLogout = () => {
    if (window.confirm("Log out?")) {
      localStorage.clear();
      window.location.href = "/";
    }
  };

  // -------------------- Fetch Notifications --------------------
  const loadNotifications = async (pid) => {
    try {
      const res = await fetch(`${API_PATIENT}/notifications/${pid}`);
      const data = await res.json();
      setNotifications(data.map((n) => n.message));
    } catch {
      console.log("Failed to load notifications");
    }
  };

  // -------------------- Load Trucks --------------------
  const loadAvailableTrucks = async () => {
    try {
      const res = await fetch(`${API_PATIENT}/trucks`);
      const data = await res.json();
      setAvailableTrucks(data);
    } catch {
      showMsg("Failed to load trucks.");
    }
  };

  // -------------------- Fetch Patient --------------------
  const handleFetchPatient = async () => {
    if (!patientId.trim()) {
      showMsg("Please enter your Patient ID.");
      return;
    }
    try {
      const res = await fetch(`${API_PATIENT}/${patientId}`);
      if (!res.ok) throw new Error();

      const data = await res.json();
      setPatient(data);
      setAppointments(data.appointmentID || []);

      // Load notifications separately
      loadNotifications(patientId);

      loadAvailableTrucks();
      showMsg("Patient data loaded successfully!");
    } catch {
      showMsg("Unable to find patient. Check your ID.");
    }
  };

  // -------------------- Load Available Slots --------------------
  useEffect(() => {
    const fetchSlots = async () => {
      if (!selectedTruck || !appointmentDate) return;
      try {
        const res = await fetch(
          `${API_PATIENT}/appointments/available/${selectedTruck}/${appointmentDate}`
        );
        if (!res.ok) throw new Error();
        const data = await res.json();
        setAvailableSlots(data);
        setSelectedSlot("");
      } catch {
        showMsg("Failed to load available slots.");
      }
    };
    fetchSlots();
  }, [selectedTruck, appointmentDate]);

  // -------------------- Book Appointment --------------------
  const handleBookAppointment = async () => {
    if (!selectedTruck || !appointmentDate || !selectedSlot) {
      showMsg("Please select a truck, date, and time slot.");
      return;
    }

    const [slotStart] = selectedSlot.split("-");
    const isoDate = new Date(`${appointmentDate}T${slotStart}:00`).toISOString();

    const payload = {
      patientId: parseInt(patientId),
      truckId: parseInt(selectedTruck),
      appointmentDate: isoDate,
    };

    try {
      setLoading(true);
      const res = await fetch(`${API_PATIENT}/appointments`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (!res.ok) {
        const errText = await res.text();
        throw new Error(errText || "Booking failed");
      }

      const data = await res.json();
      showMsg(`✅ ${data.message}`);

      await loadNotifications(patientId);
      await handleFetchPatient();
    } catch (error) {
      showMsg(`❌ Failed to book appointment: ${error.message}`);
    } finally {
      setLoading(false);
    }
  };

  // -------------------- Cancel Appointment --------------------
  const handleCancelAppointment = async (id) => {
    if (!window.confirm("Cancel this appointment?")) return;
    try {
      const res = await fetch(`${API_PATIENT}/appointments/cancel/${id}`, {
        method: "POST",
      });
      if (!res.ok) throw new Error();
      const data = await res.json();

      showMsg(`✅ ${data.message}`);

      await loadNotifications(patientId);
      await handleFetchPatient();
    } catch {
      showMsg("Failed to cancel appointment.");
    }
  };

  const getTruckInfo = (truckId) => {
    const truck = availableTrucks.find((t) => t.truckId === truckId);
    return truck ? `${truck.licensePlate} — ${truck.currentLocation}` : truckId;
  };

  // -------------------------------------------------------------
  // JSX
  // -------------------------------------------------------------
  return (
    <div className="patient-dashboard">
      <aside className="sidebar">
        <h2>DialyGo Patient</h2>
        <ul>
          {["overview", "profile", "book", "appointments", "notifications"].map(
            (tab) => (
              <li
                key={tab}
                className={activeTab === tab ? "active" : ""}
                onClick={() => setActiveTab(tab)}
              >
                {tab === "overview"
                  ? "Overview"
                  : tab === "profile"
                  ? "My Profile"
                  : tab === "book"
                  ? "Book Appointment"
                  : tab === "appointments"
                  ? "My Appointments"
                  : "Notifications"}
              </li>
            )
          )}
        </ul>
        <button className="logout-btn" onClick={handleLogout}>
          🚪 Logout
        </button>
      </aside>

      <main className="main-content">
        <header>
          <h1>Welcome to DialyGo 👋</h1>
          <p>Your mobile dialysis solution made easy.</p>
        </header>

        {message && <div className="alert">{message}</div>}

        {!patient ? (
          <div className="id-entry">
            <h2>Enter Your Patient ID</h2>
            <input
              type="text"
              placeholder="Patient ID"
              value={patientId}
              onChange={(e) => setPatientId(e.target.value)}
            />
            <button onClick={handleFetchPatient}>Access Dashboard</button>
          </div>
        ) : (
          <>
            {activeTab === "overview" && (
              <div className="overview">
                <h2>Dashboard Overview</h2>
                <p>Manage your appointments, profile, and notifications here.</p>
              </div>
            )}

            {activeTab === "profile" && (
              <div className="profile">
                <h2>My Profile</h2>
                <p><strong>Name:</strong> {patient.name}</p>
                <p><strong>Email:</strong> {patient.email}</p>
                <p><strong>Phone:</strong> {patient.phoneNumber}</p>
                <p><strong>Address:</strong> {patient.address}</p>
                <p><strong>Medical History:</strong> {patient.medicalHistory}</p>

                <h3>Treatment Records</h3>
                {patient.treatmentRecords?.length ? (
                  <table>
                    <thead>
                      <tr>
                        <th>Date</th>
                        <th>Diagnosis</th>
                        <th>Details</th>
                      </tr>
                    </thead>
                    <tbody>
                      {patient.treatmentRecords.map((t) => (
                        <tr key={t.treatmentId}>
                          <td>{new Date(t.treatmentDate).toLocaleDateString()}</td>
                          <td>{t.diagnosis}</td>
                          <td>{t.treatmentDetails}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ) : (
                  <p>No treatment records yet.</p>
                )}
              </div>
            )}

            {activeTab === "book" && (
              <div className="book-appointment">
                <h2>Book Appointment</h2>

                <select
                  value={selectedTruck}
                  onChange={(e) => setSelectedTruck(e.target.value)}
                >
                  <option value="">Select Truck</option>
                  {availableTrucks.map((truck) => (
                    <option key={truck.truckId} value={truck.truckId}>
                      {truck.licensePlate} — {truck.currentLocation}
                    </option>
                  ))}
                </select>

                <input
                  type="date"
                  value={appointmentDate}
                  onChange={(e) => setAppointmentDate(e.target.value)}
                />

                {availableSlots.length > 0 && (
                  <select
                    value={selectedSlot}
                    onChange={(e) => setSelectedSlot(e.target.value)}
                  >
                    <option value="">Select Time Slot</option>
                    {availableSlots.map((slot, idx) => (
                      <option key={idx} value={slot}>
                        {slot}
                      </option>
                    ))}
                  </select>
                )}

                <button
                  onClick={handleBookAppointment}
                  disabled={
                    !selectedTruck || !appointmentDate || !selectedSlot || loading
                  }
                >
                  {loading ? "Booking..." : "Confirm Booking"}
                </button>
              </div>
            )}

            {activeTab === "appointments" && (
              <div className="appointments">
                <h2>My Appointments</h2>

                {appointments.length > 0 ? (
                  <table>
                    <thead>
                      <tr>
                        <th>Truck</th>
                        <th>Time</th>
                        <th>Status</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {appointments.map((appt) => (
                        <tr key={appt.appointmentId}>
                          <td>{getTruckInfo(appt.truckId)}</td>
                          <td>{new Date(appt.appointmentDate).toLocaleString()}</td>
                          <td>{appt.status}</td>
                          <td>
                            {appt.status !== "Cancelled" && (
                              <button
                                className="delete-btn"
                                onClick={() => handleCancelAppointment(appt.appointmentId)}
                              >
                                🗑️ Cancel
                              </button>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ) : (
                  <p>No booked appointments yet.</p>
                )}
              </div>
            )}

            {activeTab === "notifications" && (
              <div className="notifications">
                <h2>Notifications</h2>
                {notifications.length > 0 ? (
                  <ul className="notifications-list">
                    {notifications.map((n, idx) => (
                      <li key={idx}>{n}</li>
                    ))}
                  </ul>
                ) : (
                  <p>No notifications yet.</p>
                )}
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
}

export default PatientDashboard;


