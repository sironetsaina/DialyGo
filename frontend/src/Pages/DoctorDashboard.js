import React, { useState } from "react";
import "./DoctorDashboard.css";

function DoctorDashboard() {
  const [doctorId, setDoctorId] = useState("");
  const [confirmedDoctor, setConfirmedDoctor] = useState(null);
  const [activeTab, setActiveTab] = useState("overview");
  const [patientId, setPatientId] = useState("");
  const [patient, setPatient] = useState(null);
  const [treatments, setTreatments] = useState([]);
  const [patientsToday, setPatientsToday] = useState([]);
  const [newTreatment, setNewTreatment] = useState({ Diagnosis: "", TreatmentDetails: "" });
  const [message, setMessage] = useState("");
  const [editingTreatmentId, setEditingTreatmentId] = useState(null);
  const [editingTreatment, setEditingTreatment] = useState({ Diagnosis: "", TreatmentDetails: "" });

  const API_BASE = "http://localhost:5178/api";

  const confirmDoctor = async () => {
    try {
      const res = await fetch(`${API_BASE}/Doctor/${doctorId}`);
      if (!res.ok) throw new Error("Doctor not found");
      const data = await res.json();
      setConfirmedDoctor(data);
      fetchPatientsToday();
      setMessage("");
    } catch {
      setMessage("Doctor ID not found. Please try again.");
    }
  };

  const fetchPatient = async () => {
    if (!patientId) {
      setMessage("⚠️ Please enter a Patient ID");
      return;
    }
    try {
      const res = await fetch(`${API_BASE}/Doctor/patients/${patientId}`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setPatient(data);
      fetchTreatments(patientId);
      setMessage("");
    } catch {
      setMessage("Patient not found.");
    }
  };

  const fetchTreatments = async (id) => {
    const res = await fetch(`${API_BASE}/Doctor/patients/${id}/treatments`);
    if (res.ok) setTreatments(await res.json());
  };

  const addTreatment = async () => {
    if (!newTreatment.Diagnosis || !newTreatment.TreatmentDetails) {
      setMessage("Please fill in both Diagnosis and Treatment Details.");
      return;
    }
    try {
      const res = await fetch(`${API_BASE}/Doctor/patients/${patientId}/treatment`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          diagnosis: newTreatment.Diagnosis,
          treatmentDetails: newTreatment.TreatmentDetails,
        }),
      });
      if (!res.ok) throw new Error();
      setMessage("Treatment added successfully!");
      setNewTreatment({ Diagnosis: "", TreatmentDetails: "" });
      fetchTreatments(patientId);
    } catch {
      setMessage("Failed to add treatment.");
    }
  };

  const updateTreatment = async (treatmentId) => {
    if (!editingTreatment.Diagnosis || !editingTreatment.TreatmentDetails) {
      setMessage("Please fill in both Diagnosis and Treatment Details.");
      return;
    }
    try {
      const res = await fetch(`${API_BASE}/Doctor/treatment/${treatmentId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          diagnosis: editingTreatment.Diagnosis,
          treatmentDetails: editingTreatment.TreatmentDetails,
        }),
      });
      if (!res.ok) throw new Error();
      setMessage("Treatment updated successfully!");
      setEditingTreatmentId(null);
      fetchTreatments(patientId);
    } catch {
      setMessage("Failed to update treatment.");
    }
  };

  const updateDiagnosis = async () => {
    try {
      const res = await fetch(`${API_BASE}/Doctor/patients/${patientId}/diagnosis`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(patient.medicalHistory),
      });
      if (!res.ok) throw new Error();
      setMessage("Diagnosis updated successfully!");
    } catch {
      setMessage("Failed to update diagnosis.");
    }
  };

  const fetchPatientsToday = async () => {
    const res = await fetch(`${API_BASE}/Doctor/patients-seen-today`);
    if (res.ok) setPatientsToday(await res.json());
  };

  const backToSearch = () => {
    setPatient(null);
    setPatientId("");
    setTreatments([]);
    setMessage("");
  };

  const logout = () => {
    setConfirmedDoctor(null);
    setActiveTab("overview");
    setPatient(null);
    setPatientId("");
    setTreatments([]);
    setPatientsToday([]);
    setMessage("");
  };

  return (
    <div className="doctor-dashboard">
      <aside className="sidebar">
        <h2>DialyGo Doctor</h2>
        <ul>
          <li className={activeTab === "overview" ? "active" : ""} onClick={() => setActiveTab("overview")}>Overview</li>
          <li className={activeTab === "patients" ? "active" : ""} onClick={() => setActiveTab("patients")}>Patient Records</li>
          <li className={activeTab === "seen" ? "active" : ""} onClick={() => setActiveTab("seen")}>Patients Seen Today</li>
          {confirmedDoctor && <li onClick={logout}>Logout</li>}
        </ul>
      </aside>

      <main className="main-content">
        {!confirmedDoctor ? (
          <div className="confirm-doctor">
            <h2>Confirm Doctor ID</h2>
            <input
              type="number"
              placeholder="Enter your Doctor ID"
              value={doctorId}
              onChange={(e) => setDoctorId(e.target.value)}
            />
            <button onClick={confirmDoctor}>Confirm</button>
            {message && <p className="alert">{message}</p>}
          </div>
        ) : (
          <>
            <header className="header">
              <h1>Welcome, Dr. {confirmedDoctor.name}</h1>
              <p>Specialization: {confirmedDoctor.specialization}</p>
              <p>Email: {confirmedDoctor.email} | Phone: {confirmedDoctor.phoneNumber}</p>
            </header>

            {message && <div className="alert">{message}</div>}

            {/* Overview */}
            {activeTab === "overview" && (
              <section className="overview">
                <div className="card">
                  <h3>Doctor Profile</h3>
                  <p><b>Name:</b> {confirmedDoctor.name}</p>
                  <p><b>Specialization:</b> {confirmedDoctor.specialization}</p>
                  <p><b>Email:</b> {confirmedDoctor.email}</p>
                  <p><b>Phone:</b> {confirmedDoctor.phoneNumber}</p>
                </div>
                <div className="card">
                  <h3>Patients Seen Today</h3>
                  <p>{patientsToday.length}</p>
                </div>
              </section>
            )}

            {/* Patients */}
            {activeTab === "patients" && (
              <section className="patients-section">
                {!patient ? (
                  <>
                    <h2>Search Patient</h2>
                    <div className="search-box">
                      <input
                        type="number"
                        placeholder="Enter Patient ID"
                        value={patientId}
                        onChange={(e) => setPatientId(e.target.value)}
                      />
                      <button onClick={fetchPatient}>Search</button>
                    </div>
                  </>
                ) : (
                  <>
                    <button className="back-button" onClick={backToSearch}>← Back to Search</button>

                    <div className="patient-details card">
                      <h3>Patient Details</h3>
                      <form className="patient-form">
                        <label><b>Name:</b> {patient.name}</label>
                        <label><b>Gender:</b> {patient.gender}</label>
                        <label><b>Date of Birth:</b> {new Date(patient.dateOfBirth).toLocaleDateString()}</label>
                        <label><b>Phone:</b> {patient.phoneNumber}</label>
                        <label><b>Email:</b> {patient.email}</label>
                        <label><b>Address:</b> {patient.address}</label>
                        <label>
                          <b>Diagnosis:</b>
                          <input
                            type="text"
                            value={patient.medicalHistory || ""}
                            onChange={(e) => setPatient({ ...patient, medicalHistory: e.target.value })}
                          />
                        </label>
                        <button type="button" onClick={updateDiagnosis}>Update Diagnosis</button>
                      </form>
                    </div>

                    {treatments.length > 0 && (
                      <div className="treatments card">
                        <h3>Treatment History</h3>
                        <table>
                          <thead>
                            <tr>
                              <th>Date</th>
                              <th>Diagnosis</th>
                              <th>Details</th>
                              <th>Actions</th>
                            </tr>
                          </thead>
                          <tbody>
                            {treatments.map((t) => (
                              <tr key={t.treatmentId}>
                                <td>{new Date(t.treatmentDate).toLocaleString()}</td>
                                <td>
                                  {editingTreatmentId === t.treatmentId ? (
                                    <input
                                      type="text"
                                      value={editingTreatment.Diagnosis}
                                      onChange={(e) =>
                                        setEditingTreatment({ ...editingTreatment, Diagnosis: e.target.value })
                                      }
                                    />
                                  ) : (
                                    t.diagnosis
                                  )}
                                </td>
                                <td>
                                  {editingTreatmentId === t.treatmentId ? (
                                    <textarea
                                      value={editingTreatment.TreatmentDetails}
                                      onChange={(e) =>
                                        setEditingTreatment({ ...editingTreatment, TreatmentDetails: e.target.value })
                                      }
                                    />
                                  ) : (
                                    t.treatmentDetails
                                  )}
                                </td>
                                <td>
                                  {editingTreatmentId === t.treatmentId ? (
                                    <>
                                      <button onClick={() => updateTreatment(t.treatmentId)}>Save</button>
                                      <button onClick={() => setEditingTreatmentId(null)}>Cancel</button>
                                    </>
                                  ) : (
                                    <button
                                      onClick={() => {
                                        setEditingTreatmentId(t.treatmentId);
                                        setEditingTreatment({ Diagnosis: t.diagnosis, TreatmentDetails: t.treatmentDetails });
                                      }}
                                    >
                                      Edit
                                    </button>
                                  )}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    )}

                    <div className="add-treatment card">
                      <h3>Add Treatment</h3>
                      <form className="add-treatment-form">
                        <label>
                          Diagnosis:
                          <input
                            type="text"
                            placeholder="Diagnosis"
                            value={newTreatment.Diagnosis}
                            onChange={(e) => setNewTreatment({ ...newTreatment, Diagnosis: e.target.value })}
                          />
                        </label>
                        <label>
                          Treatment Details:
                          <textarea
                            placeholder="Treatment Details"
                            value={newTreatment.TreatmentDetails}
                            onChange={(e) => setNewTreatment({ ...newTreatment, TreatmentDetails: e.target.value })}
                          />
                        </label>
                        <button
                          type="button"
                          onClick={addTreatment}
                          disabled={!newTreatment.Diagnosis || !newTreatment.TreatmentDetails}
                        >
                          Add Treatment
                        </button>
                      </form>
                    </div>
                  </>
                )}
              </section>
            )}

            {/* Patients Seen Today */}
            {activeTab === "seen" && (
              <section className="card">
                <h2>Patients Seen Today</h2>
                {patientsToday.length > 0 ? (
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
                      {patientsToday.map((p, i) => (
                        <tr key={i}>
                          <td>{p.patientId}</td>
                          <td>{p.name}</td>
                          <td>{p.diagnosis}</td>
                          <td>{new Date(p.treatmentDate).toLocaleString()}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ) : (
                  <p>No patients seen today.</p>
                )}
              </section>
            )}
          </>
        )}
      </main>
    </div>
  );
}

export default DoctorDashboard;
