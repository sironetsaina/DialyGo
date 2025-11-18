import React, { useState, useEffect } from "react";
import "./NurseDashboard.css";

/**
 * NurseDashboard.jsx
 * - Matches your NurseController endpoints exactly:
 *   GET  /api/Nurse/{id}                       -> nurse profile
 *   GET  /api/Nurse/patients                   -> list patients
 *   POST /api/Nurse/patients                   -> add patient
 *   PUT  /api/Nurse/patients/{id}              -> update patient
 *   GET  /api/Nurse/patients/{id}/treatment-summary
 *   GET  /api/Nurse/patients/{id}/health-details
 *
 * - No DELETE (removed by request)
 * - Adds "Patients Seen Today" by checking treatment-summary per patient
 */

const API_BASE = "http://localhost:5178/api/Nurse";

function NurseDashboard() {
  const [activeTab, setActiveTab] = useState("overview");
  const [nurse, setNurse] = useState(null);
  const [confirmed, setConfirmed] = useState(false);
  const [nurseIdInput, setNurseIdInput] = useState("");

  const [patients, setPatients] = useState([]);
  const [treatments, setTreatments] = useState([]); // for current selected patient
  const [healthDetails, setHealthDetails] = useState(null);

  const [patientsSeenToday, setPatientsSeenToday] = useState(0);
  const [loadingPatientsSeen, setLoadingPatientsSeen] = useState(false);

  const [message, setMessage] = useState(null); // {type: 'error'|'success', text: string}
  const [showAddModal, setShowAddModal] = useState(false);
  const [form, setForm] = useState({
    name: "",
    gender: "Female",
    dateOfBirth: "",
    phoneNumber: "",
    email: "",
    address: "",
    medicalHistory: "",
  });
  const [editPatient, setEditPatient] = useState(null);

  // helpers
  const showMessage = (type, text, timeout = 5000) => {
    setMessage({ type, text });
    if (timeout) setTimeout(() => setMessage(null), timeout);
  };
  const parseDateForInput = (d) => (d ? d.split("T")[0] : "");
  const todayStr = () => new Date().toISOString().split("T")[0];

  // --- Fetch nurse profile (GET /api/Nurse/{id})
  const fetchNurseProfile = async (id) => {
    try {
      const res = await fetch(`${API_BASE}/${id}`);
      if (!res.ok) throw new Error("Nurse not found");
      const data = await res.json();
      setNurse(data);
      showMessage("success", "Nurse confirmed");
      return true;
    } catch (err) {
      setNurse(null);
      showMessage("error", "⚠️ Nurse ID not found");
      return false;
    }
  };

  // --- Fetch patients (GET /api/Nurse/patients)
  const fetchPatients = async () => {
    try {
      const res = await fetch(`${API_BASE}/patients`);
      if (!res.ok) throw new Error("Failed to load patients");
      const data = await res.json();
      setPatients(data || []);
      return data || [];
    } catch {
      setPatients([]);
      showMessage("error", "⚠️ Failed to load patients.");
      return [];
    }
  };

  // --- Calculate Patients Seen Today
  // No central treatments endpoint exists: we call treatment-summary per patient.
  const calculatePatientsSeenToday = async (patientList) => {
    setPatientsSeenToday(0);
    setLoadingPatientsSeen(true);

    if (!patientList || patientList.length === 0) {
      setLoadingPatientsSeen(false);
      return;
    }

    try {
      // parallel fetch of treatment summaries
      const promises = patientList.map(async (p) => {
        try {
          const r = await fetch(`${API_BASE}/patients/${p.patientId}/treatment-summary`);
          if (!r.ok) return null;
          const arr = await r.json();
          return { patientId: p.patientId, treatments: arr || [] };
        } catch {
          return null;
        }
      });

      const results = await Promise.all(promises);
      const seenSet = new Set();

      results.forEach((res) => {
        if (!res || !res.treatments) return;
        const hadToday = res.treatments.some((t) => {
          const d = t.treatmentDate ? t.treatmentDate.split("T")[0] : "";
          return d === todayStr();
        });
        if (hadToday) seenSet.add(res.patientId);
      });

      setPatientsSeenToday(seenSet.size);
    } catch (err) {
      showMessage("error", "⚠️ Failed to compute patients seen today.");
    } finally {
      setLoadingPatientsSeen(false);
    }
  };

  // --- Treatment summary for a patient
  const fetchTreatmentSummary = async (patientId) => {
    setTreatments([]);
    setHealthDetails(null);
    try {
      const res = await fetch(`${API_BASE}/patients/${patientId}/treatment-summary`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setTreatments(data || []);
      // set focus to patients tab if not already
      setActiveTab("patients");
    } catch {
      showMessage("error", "⚠️ Failed to load treatment summary.");
    }
  };

  // --- Health details
  const fetchHealthDetails = async (patientId) => {
    setTreatments([]);
    setHealthDetails(null);
    try {
      const res = await fetch(`${API_BASE}/patients/${patientId}/health-details`);
      if (!res.ok) throw new Error();
      const data = await res.json();
      setHealthDetails(data);
      setActiveTab("patients");
    } catch {
      showMessage("error", "⚠️ Failed to load health details.");
    }
  };

  // --- Add patient (POST /api/Nurse/patients)
  const handleAddPatient = async (e) => {
    e.preventDefault();
    if (!form.name || !form.dateOfBirth) {
      showMessage("error", "Name and date of birth are required.");
      return;
    }

    const body = {
      name: form.name,
      gender: form.gender,
      dateOfBirth: parseDateForInput(form.dateOfBirth),
      phoneNumber: form.phoneNumber || null,
      email: form.email || null,
      address: form.address || null,
      medicalHistory: form.medicalHistory || null,
    };

    try {
      const res = await fetch(`${API_BASE}/patients`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Accept: "application/json" },
        body: JSON.stringify(body),
      });
      if (!res.ok) {
        const text = await res.text().catch(() => "");
        throw new Error(text || "Failed to add patient");
      }
      const created = await res.json();
      showMessage("success", `✅ Patient added (ID ${created.patientId})`);
      setShowAddModal(false);
      setForm({
        name: "",
        gender: "Female",
        dateOfBirth: "",
        phoneNumber: "",
        email: "",
        address: "",
        medicalHistory: "",
      });

      const list = await fetchPatients();
      calculatePatientsSeenToday(list);
    } catch (err) {
      showMessage("error", `❌ Failed to add patient. ${err.message || ""}`);
    }
  };

  // --- Update patient (PUT /api/Nurse/patients/{id})
  const handleUpdatePatient = async (patientId) => {
    if (!editPatient) return;
    if (!editPatient.name || !editPatient.dateOfBirth) {
      showMessage("error", "Name and date of birth are required.");
      return;
    }

    const payload = {
      name: editPatient.name,
      gender: editPatient.gender,
      dateOfBirth: parseDateForInput(editPatient.dateOfBirth),
      phoneNumber: editPatient.phoneNumber || null,
      email: editPatient.email || null,
      address: editPatient.address || null,
      medicalHistory: editPatient.medicalHistory || null,
    };

    try {
      const res = await fetch(`${API_BASE}/patients/${patientId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
      if (!res.ok) {
        const text = await res.text().catch(() => "");
        throw new Error(text || "Failed to update patient");
      }
      showMessage("success", "✅ Patient updated");
      setEditPatient(null);
      const list = await fetchPatients();
      calculatePatientsSeenToday(list);
    } catch (err) {
      showMessage("error", `❌ Failed to update patient. ${err.message || ""}`);
    }
  };

  // --- Confirm nurse id and load patients
  const handleConfirmNurseId = async (e) => {
    e.preventDefault();
    if (!nurseIdInput || isNaN(nurseIdInput)) {
      showMessage("error", "Enter a valid Nurse ID");
      return;
    }

    const ok = await fetchNurseProfile(nurseIdInput);
    if (ok) {
      setConfirmed(true);
      const list = await fetchPatients();
      calculatePatientsSeenToday(list);
    }
  };

  // --- Logout
  const handleLogout = () => {
    setNurse(null);
    setConfirmed(false);
    setNurseIdInput("");
    setPatients([]);
    setTreatments([]);
    setHealthDetails(null);
    setMessage(null);
    setPatientsSeenToday(0);
    setActiveTab("overview");
  };

  // --- change tab
  const changeTab = (tab) => {
    setActiveTab(tab);
    setMessage(null);
    setTreatments([]);
    setHealthDetails(null);
    if (tab === "patients") {
      fetchPatients().then((list) => calculatePatientsSeenToday(list));
    }
  };

  // auto fetch patients when confirmed (initial)
  useEffect(() => {
    if (!confirmed) return;
    (async () => {
      const list = await fetchPatients();
      calculatePatientsSeenToday(list);
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [confirmed]);

  // --- Render: show nurse-id form if not confirmed
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

  // --- Main dashboard
  return (
    <div className="nurse-dashboard">
      <aside className="sidebar">
        <h2>DialyGo Nurse</h2>
        <ul>
          <li className={activeTab === "overview" ? "active" : ""} onClick={() => changeTab("overview")}>Overview</li>
          <li className={activeTab === "profile" ? "active" : ""} onClick={() => changeTab("profile")}>Profile</li>
          <li className={activeTab === "patients" ? "active" : ""} onClick={() => changeTab("patients")}>Patients</li>
          <li className="logout-btn" onClick={handleLogout}>Logout</li>
        </ul>
      </aside>

      <main className="main-content">
        <header>
          <h1>Welcome, {nurse?.name ? nurse.name.split(" ")[0] : "Nurse"}</h1>
        </header>

        {message && <div className={`alert ${message.type}`}>{message.text}</div>}

        {/* Overview */}
        {activeTab === "overview" && (
          <section className="overview">
            <h2>Dashboard Overview</h2>
            <div className="cards">
              <div className="card">Total Patients <b>{patients.length}</b></div>
              <div className="card">Treatments Loaded <b>{treatments.length}</b></div>
              <div className="card">Patients Seen Today <b>{loadingPatientsSeen ? "..." : patientsSeenToday}</b></div>
            </div>
          </section>
        )}

        {/* Profile */}
        {activeTab === "profile" && nurse && (
          <section className="profile">
            <h2>My Profile</h2>
            <p><b>Name:</b> {nurse.name}</p>
            <p><b>Email:</b> {nurse.email}</p>
            <p><b>Phone:</b> {nurse.phoneNumber}</p>
          </section>
        )}

        {/* Patients */}
        {activeTab === "patients" && (
          <section className="patients-section">
            <h2>Patient Management</h2>

            <div className="patient-actions">
              <button onClick={() => setShowAddModal(true)}>Add New Patient</button>
              <button onClick={() => fetchPatients().then((list) => calculatePatientsSeenToday(list))}>Reload Patients</button>
            </div>

            {/* Add modal */}
            {showAddModal && (
              <div className="modal">
                <div className="modal-content">
                  <h3>Add New Patient</h3>
                  <form onSubmit={handleAddPatient} className="patient-form">
                    <input placeholder="Full Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
                    <select value={form.gender} onChange={(e) => setForm({ ...form, gender: e.target.value })}>
                      <option>Male</option><option>Female</option><option>Other</option>
                    </select>
                    <input type="date" value={form.dateOfBirth} onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })} required />
                    <input placeholder="Phone" value={form.phoneNumber} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} />
                    <input placeholder="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
                    <input placeholder="Address" value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
                    <textarea placeholder="Medical History" value={form.medicalHistory} onChange={(e) => setForm({ ...form, medicalHistory: e.target.value })} />
                    <div className="modal-actions">
                      <button type="submit">Add Patient</button>
                      <button type="button" onClick={() => setShowAddModal(false)}>Cancel</button>
                    </div>
                  </form>
                </div>
              </div>
            )}

            {/* Patients table */}
            {patients.length === 0 ? (
              <p>No patients yet. Click "Reload Patients" or "Add New Patient".</p>
            ) : (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>ID</th><th>Name</th><th>Gender</th><th>Phone</th><th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {patients.map((p) => (
                    <tr key={p.patientId}>
                      <td>{p.patientId}</td>
                      <td>{p.name}</td>
                      <td>{p.gender}</td>
                      <td>{p.phoneNumber}</td>
                      <td>
                        <button onClick={() => fetchTreatmentSummary(p.patientId)}>Treatment Summary</button>
                        <button onClick={() => fetchHealthDetails(p.patientId)}>Health Details</button>
                        <button onClick={() => setEditPatient({
                          patientId: p.patientId,
                          name: p.name || "",
                          gender: p.gender || "Female",
                          dateOfBirth: parseDateForInput(p.dateOfBirth || ""),
                          phoneNumber: p.phoneNumber || "",
                          email: p.email || "",
                          address: p.address || "",
                          medicalHistory: p.medicalHistory || ""
                        })}>Update</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {/* Edit modal */}
            {editPatient && (
              <div className="modal">
                <div className="modal-content">
                  <h3>Edit Patient</h3>
                  <input value={editPatient.name} onChange={(e) => setEditPatient({ ...editPatient, name: e.target.value })} />
                  <select value={editPatient.gender} onChange={(e) => setEditPatient({ ...editPatient, gender: e.target.value })}>
                    <option>Male</option><option>Female</option><option>Other</option>
                  </select>
                  <input type="date" value={parseDateForInput(editPatient.dateOfBirth)} onChange={(e) => setEditPatient({ ...editPatient, dateOfBirth: e.target.value })} />
                  <input placeholder="Phone" value={editPatient.phoneNumber} onChange={(e) => setEditPatient({ ...editPatient, phoneNumber: e.target.value })} />
                  <input placeholder="Email" value={editPatient.email} onChange={(e) => setEditPatient({ ...editPatient, email: e.target.value })} />
                  <input placeholder="Address" value={editPatient.address} onChange={(e) => setEditPatient({ ...editPatient, address: e.target.value })} />
                  <textarea placeholder="Medical History" value={editPatient.medicalHistory} onChange={(e) => setEditPatient({ ...editPatient, medicalHistory: e.target.value })} />
                  <div className="modal-actions">
                    <button onClick={() => handleUpdatePatient(editPatient.patientId)}>Save</button>
                    <button onClick={() => setEditPatient(null)}>Cancel</button>
                  </div>
                </div>
              </div>
            )}

            {/* Treatment Summary */}
            {treatments.length > 0 && (
              <div className="summary">
                <h3>Treatment Summary</h3>
                <table className="data-table">
                  <thead>
                    <tr><th>Date</th><th>Diagnosis</th><th>Details</th></tr>
                  </thead>
                  <tbody>
                    {treatments.map((t) => (
                      <tr key={t.treatmentId}>
                        <td>{parseDateForInput(t.treatmentDate)}</td>
                        <td>{t.diagnosis}</td>
                        <td>{t.treatmentDetails}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            {/* Health Details */}
            {healthDetails && (
              <div className="summary">
                <h3>Patient Health Details</h3>
                {/* controller returns object with Patient, TreatmentHistory, Appointments */}
                <p><b>Name:</b> {healthDetails.Patient?.name || healthDetails.patient?.name}</p>
                <p><b>Gender:</b> {healthDetails.Patient?.gender || healthDetails.patient?.gender}</p>
                <p><b>DOB:</b> {parseDateForInput(healthDetails.Patient?.dateOfBirth || healthDetails.patient?.dateOfBirth)}</p>
                <p><b>Medical History:</b> {healthDetails.Patient?.medicalHistory || healthDetails.patient?.medicalHistory}</p>

                <h4>Treatments</h4>
                <ul>
                  {(healthDetails.TreatmentHistory || healthDetails.treatmentHistory || []).map((t) => (
                    <li key={t.treatmentId}>{parseDateForInput(t.treatmentDate)} — {t.diagnosis}</li>
                  ))}
                </ul>

                <h4>Appointments</h4>
                <ul>
                  {(healthDetails.Appointments || healthDetails.appointments || []).map((a) => (
                    <li key={a.appointmentId}>{parseDateForInput(a.appointmentDate)} — {a.status}</li>
                  ))}
                </ul>
              </div>
            )}

          </section>
        )}

      </main>
    </div>
  );
}

export default NurseDashboard;
