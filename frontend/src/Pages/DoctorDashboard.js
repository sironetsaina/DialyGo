import React, { useState, useEffect } from "react";
import "./DoctorDashboard.css";

function DoctorDashboard() {
  const [doctorId, setDoctorId] = useState("");
  const [confirmedDoctor, setConfirmedDoctor] = useState(null);
  const [activeTab, setActiveTab] = useState("overview");

  const [patientId, setPatientId] = useState("");
  const [patient, setPatient] = useState(null);
  const [treatments, setTreatments] = useState([]);
  const [patientsToday, setPatientsToday] = useState([]);
  const [patientsSeenOnDate, setPatientsSeenOnDate] = useState([]);
  const [historyDate, setHistoryDate] = useState("");

  const [newTreatment, setNewTreatment] = useState({
    diagnosis: "",
    treatmentDetails: ""
  });

  const [editingTreatmentId, setEditingTreatmentId] = useState(null);
  const [editingTreatment, setEditingTreatment] = useState({
    diagnosis: "",
    treatmentDetails: ""
  });

  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const API_BASE = "http://localhost:5178/api";

  // ---------------- CONFIRM DOCTOR ----------------
  const confirmDoctor = async () => {
    if (!doctorId) return setMessage("Please enter Doctor ID");

    try {
      setLoading(true);
      const res = await fetch(`${API_BASE}/Doctor/${doctorId}`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setConfirmedDoctor(data);
      fetchPatientsSeenToday();
      setMessage("");
    } catch {
      setMessage("Doctor not found.");
    } finally {
      setLoading(false);
    }
  };

  // ---------------- FETCH PATIENT ----------------
  const fetchPatient = async () => {
    if (!patientId) return setMessage("Enter patient ID");

    try {
      setLoading(true);
      const res = await fetch(`${API_BASE}/Doctor/patients/${patientId}`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setPatient(data);
      fetchTreatments(patientId);
      setMessage("");
    } catch {
      setMessage("Patient not found.");
    } finally {
      setLoading(false);
    }
  };

  // ---------------- FETCH TREATMENTS ----------------
  const fetchTreatments = async (id) => {
    try {
      const res = await fetch(`${API_BASE}/Doctor/patients/${id}/treatments`);
      if (res.ok) {
        const data = await res.json();
        setTreatments(data);
      }
    } catch {}
  };

  // ---------------- FETCH PATIENTS SEEN TODAY ----------------
  const fetchPatientsSeenToday = async () => {
    try {
      const today = new Date().toISOString().split("T")[0]; // YYYY-MM-DD
      setHistoryDate(today);

      const res = await fetch(`${API_BASE}/Doctor/patients-seen-today`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setPatientsToday(data || []);
    } catch {
      setPatientsToday([]);
      setMessage("⚠️ Failed to load patients seen today");
    }
  };

  // ---------------- FETCH PATIENTS SEEN ON DATE ----------------
  const fetchSeenOnDate = async (date) => {
    try {
      const res = await fetch(`${API_BASE}/Doctor/patients-seen?date=${date}`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setPatientsSeenOnDate(data || []);
      setHistoryDate(date);
      setMessage("");
    } catch {
      setPatientsSeenOnDate([]);
      setMessage("⚠️ Failed to load history for the date");
    }
  };

  // ---------------- ADD TREATMENT ----------------
  const addTreatment = async () => {
    if (!newTreatment.diagnosis || !newTreatment.treatmentDetails) {
      return setMessage("Diagnosis and treatment are required.");
    }

    try {
      const res = await fetch(`${API_BASE}/Doctor/patients/${patientId}/update`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newTreatment)
      });
      const result = await res.text();
      if (!res.ok) throw new Error(result);

      setMessage("✅ Treatment and diagnosis saved.");
      setNewTreatment({ diagnosis: "", treatmentDetails: "" });
      fetchTreatments(patientId);
      fetchPatientsSeenToday();
    } catch (err) {
      setMessage("❌ Failed to save treatment.");
      console.error(err);
    }
  };

  // ---------------- UPDATE EXISTING TREATMENT ----------------
  const updateTreatment = async (treatmentId) => {
    if (!editingTreatment.diagnosis || !editingTreatment.treatmentDetails) {
      return setMessage("All fields are required.");
    }

    try {
      const res = await fetch(`${API_BASE}/Doctor/treatment/${treatmentId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(editingTreatment)
      });
      const result = await res.text();
      if (!res.ok) throw new Error(result);

      setEditingTreatmentId(null);
      fetchTreatments(patientId);
      setMessage("✅ Treatment updated.");
    } catch {
      setMessage("❌ Failed to update treatment.");
    }
  };

  // ---------------- BACK ----------------
  const backToSearch = () => {
    setPatient(null);
    setPatientId("");
    setTreatments([]);
    setMessage("");
  };

  // ---------------- LOGOUT ----------------
  const logout = () => {
    setConfirmedDoctor(null);
    setDoctorId("");
    setPatient(null);
    setActiveTab("overview");
  };

  // ---------------- HELPER TABLE COMPONENT ----------------
  const PatientsTable = ({ data }) => {
    if (data.length === 0) return <p>No patients found</p>;
    return (
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Diagnosis</th>
            <th>Date</th>
          </tr>
        </thead>
        <tbody>
          {data.map((p, i) => (
            <tr key={i}>
              <td>{p.patientId}</td>
              <td>{p.name}</td>
              <td>{p.diagnosis}</td>
              <td>{new Date(p.treatmentDate).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  };

  return (
    <div className="doctor-dashboard">
      <aside className="sidebar">
        <h2>DialyGo Doctor</h2>
        <ul>
          <li onClick={() => setActiveTab("overview")}>Overview</li>
          <li onClick={() => setActiveTab("patients")}>Patient Records</li>
          <li onClick={() => setActiveTab("seen")}>Patients Seen</li>
          {confirmedDoctor && <li onClick={logout}>Logout</li>}
        </ul>
      </aside>

      <main className="main-content">
        {!confirmedDoctor ? (
          <div className="confirm-doctor">
            <h2>Doctor Login</h2>
            <input
              type="number"
              placeholder="Enter Doctor ID"
              value={doctorId}
              onChange={(e) => setDoctorId(e.target.value)}
            />
            <button onClick={confirmDoctor}>
              {loading ? "Loading..." : "Login"}
            </button>
            {message && <p className="alert">{message}</p>}
          </div>
        ) : (
          <>
            <header className="doctor-header">
              <h2>Welcome, Dr. {confirmedDoctor.name}</h2>
              <p>{confirmedDoctor.specialization}</p>
            </header>

            {message && <div className="alert">{message}</div>}

            {/* OVERVIEW */}
            {activeTab === "overview" && (
              <div className="overview">
                <div className="card">
                  <h3>Doctor Profile</h3>
                  <p><b>Email:</b> {confirmedDoctor.email}</p>
                  <p><b>Phone:</b> {confirmedDoctor.phoneNumber}</p>
                </div>

                <div className="card">
                  <h3>Patients Seen Today</h3>
                  <h1>{patientsToday.length}</h1>
                </div>
              </div>
            )}

            {/* PATIENT RECORDS */}
            {activeTab === "patients" && (
              <div className="patients-section">
                {!patient ? (
                  <div className="search-box">
                    <h3>Search Patient</h3>
                    <input
                      type="number"
                      value={patientId}
                      onChange={(e) => setPatientId(e.target.value)}
                      placeholder="Patient ID"
                    />
                    <button onClick={fetchPatient}>Search</button>
                  </div>
                ) : (
                  <>
                    <button className="back-button" onClick={backToSearch}>← Back</button>

                    <div className="card patient-details">
                      <h3>Patient Details</h3>
                      <p><b>Name:</b> {patient.name}</p>
                      <p><b>Gender:</b> {patient.gender}</p>
                      <p><b>DOB:</b> {new Date(patient.dateOfBirth).toLocaleDateString()}</p>
                      <p><b>Phone:</b> {patient.phoneNumber}</p>
                    </div>

                    <div className="card">
                      <h3>Treatment History</h3>
                      {treatments.length === 0 ? (
                        <p>No treatments yet</p>
                      ) : (
                        <table>
                          <thead>
                            <tr>
                              <th>Date</th>
                              <th>Diagnosis</th>
                              <th>Treatment</th>
                              <th>Action</th>
                            </tr>
                          </thead>
                          <tbody>
                            {treatments.map((t) => (
                              <tr key={t.treatmentId}>
                                <td>{new Date(t.treatmentDate).toLocaleString()}</td>
                                <td>
                                  {editingTreatmentId === t.treatmentId ? (
                                    <input
                                      value={editingTreatment.diagnosis}
                                      onChange={(e) =>
                                        setEditingTreatment({ ...editingTreatment, diagnosis: e.target.value })
                                      }
                                    />
                                  ) : t.diagnosis}
                                </td>
                                <td>
                                  {editingTreatmentId === t.treatmentId ? (
                                    <textarea
                                      value={editingTreatment.treatmentDetails}
                                      onChange={(e) =>
                                        setEditingTreatment({ ...editingTreatment, treatmentDetails: e.target.value })
                                      }
                                    />
                                  ) : t.treatmentDetails}
                                </td>
                                <td>
                                  {editingTreatmentId === t.treatmentId ? (
                                    <>
                                      <button onClick={() => updateTreatment(t.treatmentId)}>Save</button>
                                      <button onClick={() => setEditingTreatmentId(null)}>Cancel</button>
                                    </>
                                  ) : (
                                    <button onClick={() => {
                                      setEditingTreatmentId(t.treatmentId);
                                      setEditingTreatment({ diagnosis: t.diagnosis, treatmentDetails: t.treatmentDetails });
                                    }}>Edit</button>
                                  )}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      )}
                    </div>

                    <div className="card add-treatment">
                      <h3>Add New Treatment</h3>
                      <input
                        type="text"
                        placeholder="Diagnosis"
                        value={newTreatment.diagnosis}
                        onChange={(e) => setNewTreatment({ ...newTreatment, diagnosis: e.target.value })}
                      />
                      <textarea
                        placeholder="Treatment Details"
                        value={newTreatment.treatmentDetails}
                        onChange={(e) => setNewTreatment({ ...newTreatment, treatmentDetails: e.target.value })}
                      />
                      <button onClick={addTreatment}>Add Treatment</button>
                    </div>
                  </>
                )}
              </div>
            )}

            {/* PATIENTS SEEN */}
            {activeTab === "seen" && (
              <div className="card">
                <h3>Patients Seen</h3>
                <input
                  type="date"
                  value={historyDate}
                  onChange={(e) => fetchSeenOnDate(e.target.value)}
                />
                <PatientsTable data={historyDate === new Date().toISOString().split("T")[0] ? patientsToday : patientsSeenOnDate} />
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
}

export default DoctorDashboard;
