import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./NurseDashboard.css";

const API_BASE = "http://localhost:5178/api/Nurse";

export default function NurseDashboard() {
  const [activeTab, setActiveTab] = useState("overview");
  const [nurse, setNurse] = useState(null);
  const [confirmed, setConfirmed] = useState(false);
  const [nurseIdInput, setNurseIdInput] = useState("");

  const [patients, setPatients] = useState([]);
  const [patientsSeenByDate, setPatientsSeenByDate] = useState([]);
  const [appointments, setAppointments] = useState([]);
  const [treatments, setTreatments] = useState([]);

  const [message, setMessage] = useState(null);
  const [loading, setLoading] = useState(false);

  const [showPatientModal, setShowPatientModal] = useState(false);
  const [showAddPatientModal, setShowAddPatientModal] = useState(false);

  const [selectedPatient, setSelectedPatient] = useState(null);
  const [editPatient, setEditPatient] = useState(null);
  const [isEditingDetails, setIsEditingDetails] = useState(false);
  const [activeSubTab, setActiveSubTab] = useState("treatments");

  const [newPatient, setNewPatient] = useState({
    name: "",
    gender: "Male",
    dateOfBirth: "",
    phoneNumber: "",
    email: "",
    address: "",
    medicalHistory: ""
  });

  const [seenDate, setSeenDate] = useState(() => new Date().toISOString().split("T")[0]);

  const navigate = useNavigate();

  const showMessage = (type, text, timeout = 4000) => {
    setMessage({ type, text });
    if (timeout) setTimeout(() => setMessage(null), timeout);
  };

  const confirmNurse = async () => {
    if (!nurseIdInput) return showMessage("error", "Please enter Nurse ID");
    try {
      setLoading(true);
      const res = await fetch(`${API_BASE}/${nurseIdInput}`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setNurse(data);
      setConfirmed(true);
      setMessage(null);
      fetchAllPatients();
      fetchPatientsSeenByDate(seenDate);
    } catch {
      showMessage("error", "Nurse not found");
    } finally {
      setLoading(false);
    }
  };

  const handleConfirmNurseId = (e) => {
    e?.preventDefault();
    if (!nurseIdInput || isNaN(nurseIdInput)) return showMessage("error", "Enter valid Nurse ID");
    confirmNurse();
  };

  const fetchAllPatients = async () => {
    try {
      setLoading(true);
      const res = await fetch(`${API_BASE}/patients`);
      const data = await res.json();
      setPatients(data || []);
    } catch {
      setPatients([]);
    } finally {
      setLoading(false);
    }
  };

  const addNewPatient = async () => {
    try {
      const res = await fetch(`${API_BASE}/patients`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newPatient)
      });
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      showMessage("success", `Patient ${data.name} added`);
      fetchAllPatients();
      fetchPatientsSeenByDate(seenDate);
      setShowAddPatientModal(false);
      setNewPatient({ name: "", gender: "Male", dateOfBirth: "", phoneNumber: "", email: "", address: "", medicalHistory: "" });
    } catch (err) {
      showMessage("error", err.message);
    }
  };

  const savePatientDetails = async (patientId) => {
    try {
      const res = await fetch(`${API_BASE}/patients/${patientId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(editPatient)
      });
      if (!res.ok) throw new Error(await res.text());
      showMessage("success", "Patient updated");
      fetchAllPatients();
      fetchPatientsSeenByDate(seenDate);
      setIsEditingDetails(false);
    } catch (err) {
      showMessage("error", err.message);
    }
  };

  const fetchPatientsSeenByDate = async (date) => {
    try {
      setLoading(true);
      const res = await fetch(`${API_BASE}/patients/seen-on?date=${date}`);
      const data = await res.json();
      setPatientsSeenByDate(data || []);
    } catch {
      setPatientsSeenByDate([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (confirmed) fetchPatientsSeenByDate(seenDate);
  }, [seenDate]);

  const fetchTreatmentSummary = async (id) => {
    try {
      const res = await fetch(`${API_BASE}/patients/${id}/treatment-summary`);
      const data = await res.json();
      setTreatments(data || []);
    } catch {
      setTreatments([]);
    }
  };

  const fetchPatientAppointments = async (id) => {
    try {
      const res = await fetch(`${API_BASE}/patients/${id}/appointments`);
      const data = await res.json();
      setAppointments(data.map(a => ({ ...a, editNotes: a.notes || "" })));
    } catch {
      setAppointments([]);
    }
  };

  const handleAppointmentNoteChange = (id, val) => {
    setAppointments(prev => prev.map(a => a.appointmentId === id ? { ...a, editNotes: val } : a));
  };

  const saveAppointmentNotes = async (id) => {
    const a = appointments.find(x => x.appointmentId === id);
    if (!a) return showMessage("error", "Appointment not found");
    if (a.status === "Cancelled") return showMessage("error", "Cancelled appointments cannot be marked as complete");
    try {
      const res = await fetch(`${API_BASE}/appointments/${id}/complete`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ notes: a.editNotes })
      });
      if (!res.ok) throw new Error(await res.text());
      showMessage("success", "Appointment completed");
      fetchPatientAppointments(selectedPatient.patientId);
      fetchPatientsSeenByDate(seenDate);
    } catch (err) {
      showMessage("error", err.message);
    }
  };

  const openPatientModal = (p) => {
    setSelectedPatient(p);
    setEditPatient({ ...p });
    setIsEditingDetails(false);
    setActiveSubTab("treatments");
    setShowPatientModal(true);
    fetchTreatmentSummary(p.patientId);
    fetchPatientAppointments(p.patientId);
  };

  const closePatientModal = () => {
    setShowPatientModal(false);
    setSelectedPatient(null);
    setEditPatient(null);
    setAppointments([]);
    setTreatments([]);
  };

  const handleLogout = () => {
    setConfirmed(false);
    setNurse(null);
    setNurseIdInput("");
    setPatients([]);
    setPatientsSeenByDate([]);
    setAppointments([]);
    setTreatments([]);
    navigate("/");
  };

  return (
    <div className="nurse-dashboard">
      <aside className="sidebar">
        <h2>DialyGo Nurse</h2>
        <ul>
          <li className={activeTab === "overview" ? "active" : ""} onClick={() => setActiveTab("overview")}>Overview</li>
          <li className={activeTab === "patients" ? "active" : ""} onClick={() => setActiveTab("patients")}>Patients</li>
          <li className={activeTab === "seen" ? "active" : ""} onClick={() => setActiveTab("seen")}>Patients Seen</li>
          <li className="logout-btn" onClick={handleLogout}>Logout</li>
        </ul>
      </aside>

      <main className="main-content">
        <header>
          <h1>Welcome, {confirmed ? nurse?.name?.split(" ")[0] : "Nurse"}</h1>
        </header>

        {message && <div className={`alert ${message.type}`}>{message.text}</div>}

        {!confirmed && (
          <div className="id-request-box">
            <h2>Confirm Your Nurse ID</h2>
            <form onSubmit={handleConfirmNurseId}>
              <input type="text" placeholder="Enter Nurse ID" value={nurseIdInput} onChange={(e) => setNurseIdInput(e.target.value)} />
              <button type="submit">Confirm</button>
            </form>
          </div>
        )}

        {confirmed && (
          <>
            {activeTab === "overview" && (
              <div className="overview">
                <div className="cards">
                  <div className="card">Total Patients: <b>{patients.length}</b></div>
                  <div className="card">Seen on {seenDate}: <b>{patientsSeenByDate.length}</b></div>
                </div>
                <label>Patients seen date:</label>
                <input type="date" value={seenDate} onChange={(e) => setSeenDate(e.target.value)} />
              </div>
            )}

            {activeTab === "patients" && (
              <div className="patients-section">
                <h2>Patients</h2>
                <button onClick={() => setShowAddPatientModal(true)}>Add New Patient</button>
                {loading ? <p>Loading...</p> : patients.length === 0 ? <p>No patients available.</p> : (
                  <table className="data-table">
                    <thead><tr><th>ID</th><th>Name</th><th>Gender</th><th>Phone</th><th>Actions</th></tr></thead>
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

            {activeTab === "seen" && (
              <div className="patients-seen">
                <h2>Patients Seen</h2>
                <label>Date:</label>
                <input type="date" value={seenDate} onChange={(e) => setSeenDate(e.target.value)} />
                {loading ? <p>Loading...</p> : patientsSeenByDate.length === 0 ? <p>No patients seen on this date.</p> : (
                  <table className="data-table">
                    <thead><tr><th>ID</th><th>Name</th><th>Phone</th><th>Date</th></tr></thead>
                    <tbody>
                      {patientsSeenByDate.map(p => (
                        <tr key={p.patientId}>
                          <td>{p.patientId}</td>
                          <td>{p.name}</td>
                          <td>{p.phoneNumber}</td>
                          <td>{(p.appointmentDate || "").split("T")[0]}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            )}

            {showAddPatientModal && (
              <div className="modal front">
                <div className="modal-content">
                  <header>
                    <h3>Add New Patient</h3>
                    <button className="close-btn" onClick={() => setShowAddPatientModal(false)}>X</button>
                  </header>
                  <div className="form-group">
                    <input placeholder="Name" value={newPatient.name} onChange={(e) => setNewPatient({ ...newPatient, name: e.target.value })} />
                    <select value={newPatient.gender} onChange={(e) => setNewPatient({ ...newPatient, gender: e.target.value })}><option>Male</option><option>Female</option><option>Other</option></select>
                    <input type="date" value={newPatient.dateOfBirth} onChange={(e) => setNewPatient({ ...newPatient, dateOfBirth: e.target.value })} />
                    <input placeholder="Phone" value={newPatient.phoneNumber} onChange={(e) => setNewPatient({ ...newPatient, phoneNumber: e.target.value })} />
                    <input placeholder="Email" value={newPatient.email} onChange={(e) => setNewPatient({ ...newPatient, email: e.target.value })} />
                    <input placeholder="Address" value={newPatient.address} onChange={(e) => setNewPatient({ ...newPatient, address: e.target.value })} />
                    <textarea placeholder="Medical History" value={newPatient.medicalHistory} onChange={(e) => setNewPatient({ ...newPatient, medicalHistory: e.target.value })} />
                    <button onClick={addNewPatient}>Add Patient</button>
                  </div>
                </div>
              </div>
            )}

            {showPatientModal && selectedPatient && (
              <div className="modal front">
                <div className="modal-content">
                  <header>
                    <h3>{selectedPatient.name}</h3>
                    <button className="close-btn" onClick={closePatientModal}>X</button>
                  </header>
                  <div className="patient-tabs">
                    <button className={activeSubTab === "treatments" ? "active" : ""} onClick={() => setActiveSubTab("treatments")}>Treatments</button>
                    <button className={activeSubTab === "appointments" ? "active" : ""} onClick={() => setActiveSubTab("appointments")}>Appointments</button>
                    <button className={activeSubTab === "details" ? "active" : ""} onClick={() => setActiveSubTab("details")}>Details</button>
                  </div>
                  <div className="tab-content">
                    {activeSubTab === "treatments" && (
                      <table className="data-table">
                        <thead><tr><th>Date</th><th>Diagnosis</th><th>Details</th></tr></thead>
                        <tbody>
                          {treatments.length === 0 ? <tr><td colSpan="3">No treatment records</td></tr> : treatments.map((t, i) => (
                            <tr key={i}><td>{t.treatmentDate?.split("T")[0]}</td><td>{t.diagnosis}</td><td>{t.treatmentDetails}</td></tr>
                          ))}
                        </tbody>
                      </table>
                    )}

                    {activeSubTab === "appointments" && (
                      <table className="data-table">
                        <thead><tr><th>Date</th><th>Status</th><th>Notes</th><th>Actions</th></tr></thead>
                        <tbody>
                          {appointments.length === 0 ? <tr><td colSpan="4">No appointments</td></tr> : appointments.map((a) => (
                            <tr key={a.appointmentId}><td>{a.appointmentDate?.split("T")[0]}</td><td>{a.status}</td><td>{a.status === "Completed" || a.status === "Cancelled" ? (a.notes || "-") : (<input type="text" value={a.editNotes} onChange={(e) => handleAppointmentNoteChange(a.appointmentId, e.target.value)} />)}</td><td>{a.status === "Completed" || a.status === "Cancelled" ? "-" : (<button disabled={!a.editNotes || !a.editNotes.trim()} onClick={() => saveAppointmentNotes(a.appointmentId)}>Mark Completed</button>)}</td></tr>
                          ))}
                        </tbody>
                      </table>
                    )}

                    {activeSubTab === "details" && (
                      <div className="patient-details">
                        {!isEditingDetails ? (
                          <>
                            <p><b>Name:</b> {editPatient.name}</p>
                            <p><b>Gender:</b> {editPatient.gender}</p>
                            <p><b>DOB:</b> {editPatient.dateOfBirth?.split("T")[0]}</p>
                            <p><b>Phone:</b> {editPatient.phoneNumber}</p>
                            <p><b>Email:</b> {editPatient.email}</p>
                            <p><b>Address:</b> {editPatient.address}</p>
                            <p><b>Medical History:</b> {editPatient.medicalHistory}</p>
                            <button onClick={() => setIsEditingDetails(true)}>Edit</button>
                          </>
                        ) : (
                          <>
                            <input value={editPatient.name} onChange={(e) => setEditPatient({ ...editPatient, name: e.target.value })} />
                            <select value={editPatient.gender} onChange={(e) => setEditPatient({ ...editPatient, gender: e.target.value })}><option>Male</option><option>Female</option><option>Other</option></select>
                            <input type="date" value={editPatient.dateOfBirth?.split("T")[0]} onChange={(e) => setEditPatient({ ...editPatient, dateOfBirth: e.target.value })} />
                            <input value={editPatient.phoneNumber} onChange={(e) => setEditPatient({ ...editPatient, phoneNumber: e.target.value })} />
                            <input value={editPatient.email} onChange={(e) => setEditPatient({ ...editPatient, email: e.target.value })} />
                            <input value={editPatient.address} onChange={(e) => setEditPatient({ ...editPatient, address: e.target.value })} />
                            <textarea value={editPatient.medicalHistory} onChange={(e) => setEditPatient({ ...editPatient, medicalHistory: e.target.value })} />
                            <button onClick={() => savePatientDetails(selectedPatient.patientId)}>Save</button>
                            <button onClick={() => setIsEditingDetails(false)}>Cancel</button>
                          </>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
}
