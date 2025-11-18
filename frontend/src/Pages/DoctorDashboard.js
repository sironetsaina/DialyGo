import React, { useState, useEffect } from "react";
import "./DoctorDashboard.css";

function DoctorDashboard() {
  const [doctorId, setDoctorId] = useState("");
  const [confirmedDoctor, setConfirmedDoctor] = useState(null);
  const [activeTab, setActiveTab] = useState("overview");
  const [patientId, setPatientId] = useState("");
  const [patient, setPatient] = useState(null);
  const [treatments, setTreatments] = useState([]);
  const [truck, setTruck] = useState(null);
  const [patientsToday, setPatientsToday] = useState([]);
  const [newTreatment, setNewTreatment] = useState({ Diagnosis: "", TreatmentDetails: "" });
  const [message, setMessage] = useState("");

  const API_BASE = "http://localhost:5178/api";

  const confirmDoctor = async () => {
    try {
      const res = await fetch(`${API_BASE}/Admin/doctors/${doctorId}`);
      if (!res.ok) throw new Error("Doctor not found");
      const data = await res.json();
      setConfirmedDoctor(data);

      // Fetch truck & patients seen today
      fetchAssignedTruck();
      fetchPatientsToday();
      setMessage("");
    } catch {
      setMessage(" Doctor ID not found. Please try again.");
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
      setMessage(" Patient not found.");
    }
  };

  const fetchTreatments = async (id) => {
    const res = await fetch(`${API_BASE}/Doctor/patients/${id}/treatments`);
    if (res.ok) setTreatments(await res.json());
  };

  const addTreatment = async () => {
    try {
      const res = await fetch(`${API_BASE}/Doctor/patients/${patientId}/treatment`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newTreatment),
      });
      if (!res.ok) throw new Error();
      setMessage("Treatment added successfully!");
      fetchTreatments(patientId);
      setNewTreatment({ Diagnosis: "", TreatmentDetails: "" });
    } catch {
      setMessage("Failed to add treatment.");
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
      setMessage(" Diagnosis updated successfully!");
    } catch {
      setMessage("Failed to update diagnosis.");
    }
  };

  const fetchAssignedTruck = async () => {
    const res = await fetch(`${API_BASE}/Doctor/assigned-truck/${doctorId}`);
    if (res.ok) setTruck(await res.json());
  };

  const fetchPatientsToday = async () => {
    const res = await fetch(`${API_BASE}/Doctor/patients-seen-today`);
    if (res.ok) setPatientsToday(await res.json());
  };

  return (
    <div className="doctor-dashboard">
      <aside className="sidebar">
        <h2>DialyGo Doctor</h2>
        <ul>
          <li className={activeTab === "overview" ? "active" : ""} onClick={() => setActiveTab("overview")}> Overview</li>
          <li className={activeTab === "patients" ? "active" : ""} onClick={() => setActiveTab("patients")}>Patient Records</li>
          <li className={activeTab === "truck" ? "active" : ""} onClick={() => setActiveTab("truck")}> Assigned Truck</li>
          <li className={activeTab === "seen" ? "active" : ""} onClick={() => setActiveTab("seen")}> Patients Seen Today</li>
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
              <h1>Welcome, Dr. {confirmedDoctor.name} </h1>
              <p>Specialization: {confirmedDoctor.specialization}</p>
            </header>

            {message && <div className="alert">{message}</div>}

            {/* Overview */}
            {activeTab === "overview" && (
              <section className="overview">
                <div className="card">
                  <h3>Assigned Truck</h3>
                  {truck ? (
                    <p>
                      {truck.licensePlate} — {truck.currentLocation}
                    </p>
                  ) : (
                    <p>No truck assigned yet.</p>
                  )}
                </div>
                <div className="card">
                  <h3> Patients Seen Today</h3>
                  <p>{patientsToday.length}</p>
                </div>
              </section>
            )}

            {/* Patients */}
            {activeTab === "patients" && (
              <section className="patients-section">
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

                {patient && (
                  <div className="patient-details">
                    <h3>{patient.name}</h3>
                    <p><b>Gender:</b> {patient.gender}</p>
                    <p><b>Email:</b> {patient.email}</p>
                    <p><b>Diagnosis:</b>
                      <input
                        type="text"
                        value={patient.medicalHistory || ""}
                        onChange={(e) =>
                          setPatient({ ...patient, medicalHistory: e.target.value })
                        }
                      />
                      <button onClick={updateDiagnosis}>Update Diagnosis</button>
                    </p>
                  </div>
                )}

                {treatments.length > 0 && (
                  <div className="treatments">
                    <h3>Treatment History</h3>
                    <table>
                      <thead>
                        <tr>
                          <th>Date</th>
                          <th>Diagnosis</th>
                          <th>Details</th>
                        </tr>
                      </thead>
                      <tbody>
                        {treatments.map((t) => (
                          <tr key={t.treatmentId}>
                            <td>{new Date(t.treatmentDate).toLocaleString()}</td>
                            <td>{t.diagnosis}</td>
                            <td>{t.treatmentDetails}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}

                <div className="add-treatment">
                  <h3>Add Treatment</h3>
                  <input
                    type="text"
                    placeholder="Diagnosis"
                    value={newTreatment.Diagnosis}
                    onChange={(e) =>
                      setNewTreatment({ ...newTreatment, Diagnosis: e.target.value })
                    }
                  />
                  <textarea
                    placeholder="Treatment Details"
                    value={newTreatment.TreatmentDetails}
                    onChange={(e) =>
                      setNewTreatment({
                        ...newTreatment,
                        TreatmentDetails: e.target.value,
                      })
                    }
                  />
                  <button onClick={addTreatment}>Add Treatment</button>
                </div>
              </section>
            )}

            {/* Assigned Truck */}
            {activeTab === "truck" && (
              <section>
                <h2> Assigned Truck</h2>
                {truck ? (
                  <div className="card">
                    <p><b>Plate:</b> {truck.licensePlate}</p>
                    <p><b>Location:</b> {truck.currentLocation}</p>
                    <p><b>Capacity:</b> {truck.capacity}</p>
                  </div>
                ) : (
                  <p>No truck assigned yet.</p>
                )}
              </section>
            )}

            {/* Patients Seen Today */}
            {activeTab === "seen" && (
              <section>
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
