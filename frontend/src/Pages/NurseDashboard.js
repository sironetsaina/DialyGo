import React, { useState, useEffect } from "react";
import "./NurseDashboard.css";

const API_BASE = "http://localhost:5178/api/Nurse";

export default function NurseDashboard() {
  const [activeTab, setActiveTab] = useState("overview");

  // Nurse auth
  const [nurse, setNurse] = useState(null);
  const [confirmed, setConfirmed] = useState(false);
  const [nurseIdInput, setNurseIdInput] = useState("");

  // Data
  const [patients, setPatients] = useState([]);
  const [patientsSeenToday, setPatientsSeenToday] = useState([]);
  const [appointments, setAppointments] = useState([]);
  const [treatments, setTreatments] = useState([]);

  // UI
  const [message, setMessage] = useState(null);
  const [showPatientModal, setShowPatientModal] = useState(false);

  // Patient modal
  const [selectedPatient, setSelectedPatient] = useState(null);
  const [editPatient, setEditPatient] = useState(null);
  const [isEditingDetails, setIsEditingDetails] = useState(false);
  const [activeSubTab, setActiveSubTab] = useState("treatments");

  const showMessage = (type, text, timeout = 4000) => {
    setMessage({ type, text });
    if (timeout) setTimeout(() => setMessage(null), timeout);
  };

  // -------------------- Nurse Confirmation --------------------
  const fetchNurseProfile = async (id) => {
    try {
      const res = await fetch(`${API_BASE}/${id}`);
      if (!res.ok) throw new Error("Nurse not found");
      const data = await res.json();
      setNurse(data);
      showMessage("success", "Nurse confirmed");
      return true;
    } catch {
      setNurse(null);
      showMessage("error", "⚠️ Nurse ID not found");
      return false;
    }
  };

  const handleConfirmNurseId = async (e) => {
    e?.preventDefault();
    if (!nurseIdInput || isNaN(nurseIdInput)) {
      showMessage("error", "Enter a valid Nurse ID");
      return;
    }
    const ok = await fetchNurseProfile(nurseIdInput);
    if (ok) {
      setConfirmed(true);
      fetchPatients();
      fetchPatientsSeenToday();
    }
  };

  // -------------------- Fetch Patients --------------------
  const fetchPatients = async () => {
    try {
      const res = await fetch(`${API_BASE}/patients`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setPatients(data || []);
    } catch {
      setPatients([]);
      showMessage("error", "⚠️ Failed to load patients.");
    }
  };

  // -------------------- Fetch Patients Seen Today --------------------
  const fetchPatientsSeenToday = async () => {
    try {
      const res = await fetch(`${API_BASE}/patients/seen-today`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setPatientsSeenToday(data || []);
    } catch {
      setPatientsSeenToday([]);
      showMessage("error", "⚠️ Failed to load patients seen today");
    }
  };

  // -------------------- Open / Close Patient Modal --------------------
  const openPatientModal = (patient) => {
    setSelectedPatient(patient);
    setEditPatient({ ...patient });
    setActiveSubTab("treatments");
    setShowPatientModal(true);
    fetchTreatmentSummary(patient.patientId);
    fetchPatientAppointments(patient.patientId);
  };

  const closePatientModal = () => {
    setShowPatientModal(false);
    setSelectedPatient(null);
    setTreatments([]);
    setAppointments([]);
    setEditPatient(null);
    setIsEditingDetails(false);
  };

  // -------------------- Fetch Treatment Summary --------------------
  const fetchTreatmentSummary = async (patientId) => {
    try {
      const res = await fetch(`${API_BASE}/patients/${patientId}/treatment-summary`);
      if (!res.ok) {
        setTreatments([]);
        return;
      }
      const data = await res.json();
      setTreatments(data || []);
    } catch {
      setTreatments([]);
      showMessage("error", "⚠️ Failed to load treatment summary.");
    }
  };

  // -------------------- Fetch Appointments --------------------
  const fetchPatientAppointments = async (patientId) => {
    try {
      const res = await fetch(`${API_BASE}/patients/${patientId}/appointments`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setAppointments((data || []).map(a => ({ ...a, editNotes: a.notes || "" })));
    } catch {
      setAppointments([]);
      showMessage("error", "⚠️ Failed to load appointments.");
    }
  };

  // -------------------- Handle Appointment Notes --------------------
  const handleAppointmentNoteChange = (appointmentId, value) => {
    setAppointments(prev => prev.map(a => a.appointmentId === appointmentId ? { ...a, editNotes: value } : a));
  };

  // -------------------- Save Notes --------------------
  const saveAppointmentNotes = async (appointmentId) => {
    const appointment = appointments.find(a => a.appointmentId === appointmentId);
    if (!appointment) return showMessage("error", "Appointment not found");

    try {
      const res = await fetch(`${API_BASE}/appointments/${appointmentId}/complete`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ notes: appointment.editNotes, nurseId: nurse.nurseId })
      });
      if (!res.ok) throw new Error(await res.text());
      showMessage("success", "✅ Appointment completed and SMS sent");
      fetchPatientsSeenToday();
      if (selectedPatient) fetchPatientAppointments(selectedPatient.patientId);
    } catch (err) {
      showMessage("error", `❌ ${err.message}`);
    }
  };

  // -------------------- Save Patient Details --------------------
  const savePatientDetails = async (patientId) => {
    try {
      const payload = {
        name: editPatient.name,
        gender: editPatient.gender,
        dateOfBirth: editPatient.dateOfBirth,
        phoneNumber: editPatient.phoneNumber,
        email: editPatient.email,
        address: editPatient.address,
        medicalHistory: editPatient.medicalHistory
      };
      const res = await fetch(`${API_BASE}/patients/${patientId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      });
      if (!res.ok) throw new Error(await res.text());
      showMessage("success", "✅ Patient details updated");

      // Refresh patients & seen today
      fetchPatients();
      fetchPatientsSeenToday();

      setSelectedPatient({ ...selectedPatient, ...payload });
      setIsEditingDetails(false);
    } catch (err) {
      showMessage("error", `❌ ${err.message}`);
    }
  };

  // -------------------- Add New Patient --------------------
  const addNewPatient = async (newPatient) => {
    try {
      const res = await fetch(`${API_BASE}/patients`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newPatient)
      });
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      showMessage("success", `✅ Patient ${data.name} added`);

      // Refresh patients & seen today
      fetchPatients();
      fetchPatientsSeenToday();
    } catch (err) {
      showMessage("error", `❌ ${err.message}`);
    }
  };

  const handleLogout = () => {
    setNurse(null);
    setConfirmed(false);
    setNurseIdInput("");
    setPatients([]);
    setPatientsSeenToday([]);
    setActiveTab("overview");
    closePatientModal();
  };

  // -------------------- Render --------------------
  if (!confirmed) {
    return (
      <div className="confirm-nurse-id">
        <h2>Confirm Your Nurse ID</h2>
        {message && <div className={`alert ${message.type}`}>{message.text}</div>}
        <form onSubmit={handleConfirmNurseId}>
          <input
            type="text"
            placeholder="Enter Nurse ID"
            value={nurseIdInput}
            onChange={(e) => setNurseIdInput(e.target.value)}
          />
          <button type="submit">Confirm ID</button>
        </form>
      </div>
    );
  }

  return (
    <div className="nurse-dashboard">
      <aside className="sidebar">
        <h2>DialyGo Nurse</h2>
        <ul>
          <li className={activeTab === "overview" ? "active" : ""} onClick={() => setActiveTab("overview")}>Overview</li>
          <li className={activeTab === "patients" ? "active" : ""} onClick={() => setActiveTab("patients")}>Patients</li>
          <li className={activeTab === "seen" ? "active" : ""} onClick={() => setActiveTab("seen")}>Patients Seen Today</li>
          <li className="logout-btn" onClick={handleLogout}>Logout</li>
        </ul>
      </aside>

      <main className="main-content">
        <header>
          <h1>Welcome, {nurse?.name?.split(" ")[0] || "Nurse"}</h1>
        </header>

        {message && <div className={`alert ${message.type}`}>{message.text}</div>}

        {/* Overview */}
        {activeTab === "overview" && (
          <div className="overview">
            <div className="cards">
              <div className="card">Total Patients: <b>{patients.length}</b></div>
              <div className="card">Patients Seen Today: <b>{patientsSeenToday.length}</b></div>
            </div>
            <button onClick={() => { fetchPatients(); fetchPatientsSeenToday(); }}>Refresh</button>
          </div>
        )}

        {/* Patients */}
        {activeTab === "patients" && (
          <div className="patients-section">
            <h2>Patient Management</h2>
            {patients.length === 0 ? <p>No patients found.</p> : (
              <table className="data-table">
                <thead>
                  <tr><th>ID</th><th>Name</th><th>Gender</th><th>Phone</th><th>Actions</th></tr>
                </thead>
                <tbody>
                  {patients.map(p => (
                    <tr key={p.patientId}>
                      <td>{p.patientId}</td>
                      <td>{p.name}</td>
                      <td>{p.gender}</td>
                      <td>{p.phoneNumber}</td>
                      <td><button onClick={() => openPatientModal(p)}>View</button></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}

        {/* Patients Seen Today */}
        {activeTab === "seen" && (
          <div className="patients-seen-today">
            <h2>Patients Seen Today</h2>
            {patientsSeenToday.length === 0 ? <p>No patients seen today.</p> : (
              <table className="data-table">
                <thead>
                  <tr><th>ID</th><th>Name</th><th>Phone</th></tr>
                </thead>
                <tbody>
                  {patientsSeenToday.map(p => (
                    <tr key={p.patientId}>
                      <td>{p.patientId}</td>
                      <td>{p.name}</td>
                      <td>{p.phoneNumber}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}

        {/* Patient Modal */}
        {showPatientModal && selectedPatient && (
          <div className="modal front">
            <div className="modal-content">
              <header style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <h3>{selectedPatient.name}</h3>
                <button className="close-btn" onClick={closePatientModal}>X</button>
              </header>

              <div className="patient-tabs">
                <button className={activeSubTab === "treatments" ? "active" : ""} onClick={() => setActiveSubTab("treatments")}>Treatments</button>
                <button className={activeSubTab === "appointments" ? "active" : ""} onClick={() => setActiveSubTab("appointments")}>Appointments</button>
                <button className={activeSubTab === "details" ? "active" : ""} onClick={() => setActiveSubTab("details")}>Patient Details</button>
              </div>

              <div className="tab-content">
                {/* Treatments */}
                {activeSubTab === "treatments" && (
                  <table className="data-table">
                    <thead>
                      <tr><th>Date</th><th>Diagnosis</th><th>Details</th></tr>
                    </thead>
                    <tbody>
                      {treatments.length === 0 ? <tr><td colSpan={3}>No treatments</td></tr> :
                        treatments.map((t, idx) => (
                          <tr key={idx}>
                            <td>{(t.treatmentDate || "").split("T")[0]}</td>
                            <td>{t.diagnosis}</td>
                            <td>{t.treatmentDetails}</td>
                          </tr>
                        ))}
                    </tbody>
                  </table>
                )}

                {/* Appointments */}
                {activeSubTab === "appointments" && (
                  <table className="data-table">
                    <thead>
                      <tr><th>Date</th><th>Status</th><th>Notes</th><th>Actions</th></tr>
                    </thead>
                    <tbody>
                      {appointments.length === 0 ? <tr><td colSpan={4}>No appointments</td></tr> :
                        appointments.map(a => (
                          <tr key={a.appointmentId}>
                            <td>{(a.appointmentDate || "").split("T")[0]}</td>
                            <td>{a.status}</td>
                            <td>
                              {(a.status === "Completed" || a.status === "Cancelled") ? a.notes || "-" :
                                <input
                                  type="text"
                                  value={a.editNotes || ""}
                                  onChange={(e) => handleAppointmentNoteChange(a.appointmentId, e.target.value)}
                                />}
                            </td>
                            <td>
                              {(a.status !== "Completed" && a.status !== "Cancelled") &&
                                <button onClick={() => saveAppointmentNotes(a.appointmentId)} disabled={!a.editNotes || !a.editNotes.trim()}>Mark Completed</button>}
                            </td>
                          </tr>
                        ))}
                    </tbody>
                  </table>
                )}

                {/* Patient Details */}
                {activeSubTab === "details" && editPatient && (
                  <div className="patient-details">
                    {!isEditingDetails ? (
                      <>
                        <p><b>Name:</b> {editPatient.name}</p>
                        <p><b>Gender:</b> {editPatient.gender}</p>
                        <p><b>DOB:</b> {(editPatient.dateOfBirth || "").split("T")[0]}</p>
                        <p><b>Phone:</b> {editPatient.phoneNumber}</p>
                        <p><b>Email:</b> {editPatient.email}</p>
                        <p><b>Address:</b> {editPatient.address}</p>
                        <p><b>Medical History:</b> {editPatient.medicalHistory}</p>
                        <button onClick={() => setIsEditingDetails(true)}>Edit</button>
                      </>
                    ) : (
                      <>
                        <input value={editPatient.name} onChange={e => setEditPatient({ ...editPatient, name: e.target.value })} />
                        <select value={editPatient.gender} onChange={e => setEditPatient({ ...editPatient, gender: e.target.value })}>
                          <option>Male</option><option>Female</option><option>Other</option>
                        </select>
                        <input type="date" value={(editPatient.dateOfBirth || "").split("T")[0]} onChange={e => setEditPatient({ ...editPatient, dateOfBirth: e.target.value })} />
                        <input placeholder="Phone" value={editPatient.phoneNumber} onChange={e => setEditPatient({ ...editPatient, phoneNumber: e.target.value })} />
                        <input placeholder="Email" value={editPatient.email} onChange={e => setEditPatient({ ...editPatient, email: e.target.value })} />
                        <input placeholder="Address" value={editPatient.address} onChange={e => setEditPatient({ ...editPatient, address: e.target.value })} />
                        <textarea placeholder="Medical History" value={editPatient.medicalHistory} onChange={e => setEditPatient({ ...editPatient, medicalHistory: e.target.value })} />
                        <button onClick={() => savePatientDetails(editPatient.patientId)}>Save</button>
                        <button onClick={() => setIsEditingDetails(false)}>Cancel</button>
                      </>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
