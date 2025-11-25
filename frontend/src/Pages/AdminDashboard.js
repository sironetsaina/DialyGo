import React, { useState, useEffect } from "react";
import "./AdminDashboard.css";


function AdminDashboard() {
  const [activeTab, setActiveTab] = useState("overview");
  const [data, setData] = useState([]);
  const [editItem, setEditItem] = useState(null);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [newItem, setNewItem] = useState({});
  const [searchTerm, setSearchTerm] = useState("");
  const [message, setMessage] = useState("");
  const [overviewMetrics, setOverviewMetrics] = useState({
    totalDoctors: 0,
    totalNurses: 0,
    totalPatients: 0,
    totalTrucks: 0,
    availableTrucks: 0,
    fullyBookedTrucks: 0,
  });
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(false);

  const API_ADMIN = process.env.REACT_APP_API_ADMIN || "http://localhost:5178/api/Admin";

  
  const showMsg = (msg, ms = 4000) => {
    setMessage(msg);
    setTimeout(() => setMessage(""), ms);
  };

  const handleBack = () => window.history.back();
  const handleLogout = () => {
    if (window.confirm("Are you sure you want to log out?")) {
      localStorage.clear();
      window.location.href = "/";
    }
  };

  const getIdField = () => {
    switch (activeTab) {
      case "doctors":
        return "doctorId";
      case "nurses":
        return "nurseId";
      case "patients":
        return "patientId";
      case "users":
        return "userId"; 
      case "trucks":
        return "truckId";
      default:
        return "id";
    }
  };

  const normalizeBookingsCount = (truck) => {
    return truck.bookingsCount ?? truck.BookingsCount ?? 0;
  };

  const computeTruckMetrics = (trucks) => {
    let fullyBooked = 0;
    let available = 0;
    trucks.forEach((truck) => {
      const bookings = normalizeBookingsCount(truck);
      const capacity = truck.capacity ?? truck.Capacity ?? 0;
      if (capacity > 0 && bookings >= capacity) fullyBooked++;
      else available++;
    });
    return { fullyBooked, available };
  };

  // Generic GET helper
  const fetchJson = async (url, opts) => {
    const res = await fetch(url, opts);
    if (!res.ok) {
      const text = await res.text().catch(() => "");
      throw new Error(text || res.statusText);
    }
    // catch endpoints returning objects and empty on delete
    try {
      return await res.json();
    } catch {
      return null;
    }
  };

  const loadOverviewMetrics = async () => {
    try {
      setLoading(true);
      const [metricsRes, trucksRes] = await Promise.all([
        fetch(`${API_ADMIN}/metrics`),
        fetch(`${API_ADMIN}/trucks`),
      ]);

      const metrics = metricsRes.ok ? await metricsRes.json() : {};
      const trucks = trucksRes.ok ? await trucksRes.json() : [];

      const { fullyBooked, available } = computeTruckMetrics(trucks);

      setOverviewMetrics({
        totalDoctors: metrics.totalDoctors || 0,
        totalNurses: metrics.totalNurses || 0,
        totalPatients: metrics.totalPatients || 0,
        totalTrucks: Array.isArray(trucks) ? trucks.length : metrics.totalTrucks || 0,
        fullyBookedTrucks: fullyBooked,
        availableTrucks: available,
      });
    } catch (error) {
      console.error("Failed to load metrics:", error);
      showMsg("Failed to load metrics");
    } finally {
      setLoading(false);
    }
  };

  const loadNotifications = async () => {
    try {
      const res = await fetch(`${API_ADMIN}/notifications`);
      if (res.ok) setNotifications(await res.json());
      else setNotifications([]);
    } catch (err) {
      // Fallback sample notifications
      setNotifications([
        "Truck 2 is fully booked today",
        "Appointment #123 cancelled by patient",
        "Truck 3 route delayed",
      ]);
    }
  };

  const loadData = async (tab) => {
    if (tab === "overview") return;
    try {
      setLoading(true);
      const res = await fetch(`${API_ADMIN}/${tab}`);
      if (!res.ok) throw new Error();
      const result = await res.json();
      setData(Array.isArray(result) ? result : [result]);
    } catch (err) {
      console.error("Load data error", err);
      showMsg("Failed to load data");
      setData([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (activeTab === "overview") {
      loadOverviewMetrics();
      loadNotifications();
    } else {
      loadData(activeTab);
    }
    // Reset modals and forms when switching tabs
    setIsAddModalOpen(false);
    setEditItem(null);
    setNewItem({});
  }, [activeTab]);

  const handleDelete = async (id) => {
    if (!window.confirm("Delete this record?")) return;
    try {
      await fetchJson(`${API_ADMIN}/${activeTab}/${id}`, { method: "DELETE" });
      showMsg("Deleted successfully!");
      await loadData(activeTab);
    } catch (err) {
      console.error(err);
      showMsg("Delete failed.");
    }
  };

  const preparePayloadForTab = (payload, tab) => {
    // Ensure numeric fields are numbers and strip id fields as necessary
    const p = { ...payload };
    if (tab === "users") {
      // Backend expects UserCreateDto (no userId in body)
      delete p.userId;
      delete p.id;
      if (p.roleId !== undefined) p.roleId = Number(p.roleId);
      if (p.relatedId !== undefined && p.relatedId !== null && p.relatedId !== "") p.relatedId = Number(p.relatedId);
      if (p.isActive === undefined) p.isActive = true;
    }

    if (tab === "trucks") {
      delete p.truckId;
      if (p.capacity !== undefined) p.capacity = Number(p.capacity);
      if (p.bookingsCount !== undefined) p.bookingsCount = Number(p.bookingsCount);
    }

    if (tab === "doctors" || tab === "nurses") {
      delete p.doctorId;
      delete p.nurseId;
      delete p.id;
    }

    return p;
  };

  const handleUpdate = async (id, tab) => {
    try {
      const payload = preparePayloadForTab(editItem, tab);
      await fetchJson(`${API_ADMIN}/${tab}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
      showMsg("Updated successfully!");
      setEditItem(null);
      await loadData(tab);
    } catch (err) {
      console.error(err);
      showMsg("Update failed.");
    }
  };

  const handleAdd = async (tab) => {
    if (tab === "patients") {
      showMsg("Patients can only be viewed.");
      return;
    }
    try {
      const payload = preparePayloadForTab(newItem, tab);
      await fetchJson(`${API_ADMIN}/${tab}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
      showMsg("Added successfully!");
      setNewItem({});
      setIsAddModalOpen(false);
      await loadData(tab);
    } catch (err) {
      console.error(err);
      showMsg("Failed to add new record.");
    }
  };

  const idField = getIdField();

  const filteredData = data.filter((item) => {
    const term = searchTerm.toLowerCase();
    const idVal = (item[idField] ?? item[idField.charAt(0).toUpperCase() + idField.slice(1)])?.toString() ?? "";
    return (
      idVal.toLowerCase().includes(term) ||
      (item.name ?? item.Name ?? "").toString().toLowerCase().includes(term) ||
      (item.username ?? "").toLowerCase().includes(term)
    );
  });

  const handleChangeIn = (objSetter) => (key, value) => objSetter((prev) => ({ ...prev, [key]: value }));

  const renderFormFields = (item, setItem) => {
    const handleChange = handleChangeIn(setItem);

    switch (activeTab) {
      case "doctors":
      case "nurses":
        return (
          <>
            <input placeholder="Name" value={item.name || item.Name || ""} onChange={(e) => handleChange("name", e.target.value)} />
            <input placeholder="Email" value={item.email || item.Email || ""} onChange={(e) => handleChange("email", e.target.value)} />
            {activeTab === "doctors" && (
              <input placeholder="Specialization" value={item.specialization || item.Specialization || ""} onChange={(e) => handleChange("specialization", e.target.value)} />
            )}
            <input placeholder="Phone Number" value={item.phoneNumber || item.PhoneNumber || ""} onChange={(e) => handleChange("phoneNumber", e.target.value)} />
          </>
        );

      case "users":
        return (
          <>
            <input placeholder="Username" value={item.username || item.Username || ""} onChange={(e) => handleChange("username", e.target.value)} />

            <input placeholder="Password" type="password" value={item.password || item.Password || ""} onChange={(e) => handleChange("password", e.target.value)} />

            <input placeholder="Role ID" type="number" value={item.roleId ?? item.RoleId ?? ""} onChange={(e) => handleChange("roleId", Number(e.target.value))} />

            <input placeholder="Related ID" type="number" value={item.relatedId ?? item.RelatedId ?? ""} onChange={(e) => handleChange("relatedId", e.target.value === "" ? null : Number(e.target.value))} />

            <label style={{ display: "flex", alignItems: "center", gap: "8px" }}>
              <input type="checkbox" checked={item.isActive ?? item.IsActive ?? true} onChange={(e) => handleChange("isActive", e.target.checked)} /> Active
            </label>
          </>
        );

      case "trucks":
        return (
          <>
            <input placeholder="License Plate" value={item.licensePlate || item.LicensePlate || ""} onChange={(e) => handleChange("licensePlate", e.target.value)} />
            <input placeholder="Current Location" value={item.currentLocation || item.CurrentLocation || ""} onChange={(e) => handleChange("currentLocation", e.target.value)} />
            <input placeholder="Capacity" type="number" value={item.capacity ?? item.Capacity ?? ""} onChange={(e) => handleChange("capacity", Number(e.target.value))} />
            <input placeholder="Bookings Count" type="number" value={item.bookingsCount ?? item.BookingsCount ?? 0} onChange={(e) => handleChange("bookingsCount", Number(e.target.value))} />
          </>
        );

      default:
        return <p>No editable fields for this category.</p>;
    }
  };

  const renderOverview = () => (
    <div className="overview-container">
      <h2>System Metrics Summary</h2>
      <div className="metrics-grid">
        <div className="metric-card"><h3>Doctors</h3><p className="metric-value">{overviewMetrics.totalDoctors}</p></div>
        <div className="metric-card"><h3>Nurses</h3><p className="metric-value">{overviewMetrics.totalNurses}</p></div>
        <div className="metric-card"><h3>Patients</h3><p className="metric-value">{overviewMetrics.totalPatients}</p></div>
        <div className="metric-card"><h3>Total Trucks</h3><p className="metric-value">{overviewMetrics.totalTrucks}</p></div>
        <div className="metric-card"><h3>Available Trucks</h3><p className="metric-value">{overviewMetrics.availableTrucks}</p></div>
        <div className="metric-card"><h3>Fully Booked Trucks</h3><p className="metric-value">{overviewMetrics.fullyBookedTrucks}</p></div>
      </div>

      <div className="notifications-section">
        <h3>Alerts & Notifications</h3>
        <ul>{notifications.map((note, i) => <li key={i}>{note}</li>)}</ul>
      </div>
    </div>
  );

  return (
    <div className="admin-dashboard">
      <aside className="sidebar">
        <h2>DialyGo Admin</h2>
        <ul>
          {["overview", "doctors", "nurses", "patients", "users", "trucks"].map((tab) => (
            <li key={tab} className={activeTab === tab ? "active" : ""} onClick={() => setActiveTab(tab)}>
              {tab.charAt(0).toUpperCase() + tab.slice(1)}
            </li>
          ))}
        </ul>
        <button className="logout-btn" onClick={handleLogout}>Logout</button>
      </aside>

      <main className="main-content">
        <button className="back-arrow" onClick={handleBack}>← Back</button>
        <header>
          <h1>Welcome, Admin</h1>
          <p>Manage users, staff, patients, and trucks here.</p>
        </header>

        {message && <div className="alert">{message}</div>}

        {activeTab === "overview" ? (
          renderOverview()
        ) : (
          <>
            <div className="top-controls">
              <input
                type="text"
                placeholder={`Search by name or ${idField}...`}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />

              {activeTab !== "patients" && (
                <button className="add-btn" onClick={() => { setIsAddModalOpen(true); setNewItem({}); }}>
                  Add New
                </button>
              )}
            </div>

            <div className="data-table">
              <table>
                <thead>
                  <tr>
                    {data[0] && Object.keys(data[0]).map((key) => <th key={key}>{key}</th>)}
                    {activeTab === "trucks" && <th>Status</th>}
                    <th>{activeTab === "patients" ? "Phone Number" : "Actions"}</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    <tr><td colSpan="100%">Loading...</td></tr>
                  ) : filteredData.length > 0 ? (
                    filteredData.map((item) => {
                      const keyVal = item[idField] ?? item[idField.charAt(0).toUpperCase() + idField.slice(1)];
                      return (
                        <tr key={keyVal ?? JSON.stringify(item)}>
                          {Object.keys(item).map((k, i) => <td key={i}>{String(item[k])}</td>)}
                          {activeTab === "trucks" && (
                            <td>
                              {(normalizeBookingsCount(item) >= (item.capacity ?? item.Capacity ?? 0)) ? (
                                <span style={{ color: "red", fontWeight: "bold" }}>Fully Booked</span>
                              ) : (
                                <span style={{ color: "green", fontWeight: "bold" }}>Available</span>
                              )}
                            </td>
                          )}
                          <td>
                            {activeTab === "patients" ? (
                              item.phoneNumber ?? item.PhoneNumber ?? ""
                            ) : (
                              <>
                                <button className="edit-btn" onClick={() => setEditItem(item)}>
                                  Edit
                                </button>
                                <button className="delete-btn" onClick={() => handleDelete(item[idField] ?? item[idField.charAt(0).toUpperCase() + idField.slice(1)])}>
                                  Delete
                                </button>
                              </>
                            )}
                          </td>
                        </tr>
                      );
                    })
                  ) : (
                    <tr><td colSpan="100%">No records found</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </>
        )}

        {/* Edit Modal */}
        {editItem && (
          <div className="modal-overlay" onClick={() => setEditItem(null)}>
            <div className="modal" onClick={(e) => e.stopPropagation()}>
              <h3>Edit {activeTab.slice(0, -1)}</h3>
              {renderFormFields(editItem, setEditItem)}

              <div style={{ marginTop: "20px", display: "flex", gap: "10px" }}>
                <button onClick={() => handleUpdate(editItem[idField] ?? editItem[idField.charAt(0).toUpperCase() + idField.slice(1)], activeTab)}>Save</button>
                <button className="cancel-btn" onClick={() => setEditItem(null)}>Cancel</button>
              </div>
            </div>
          </div>
        )}

        {/* Add Modal */}
        {isAddModalOpen && (
          <div className="modal-overlay" onClick={() => setIsAddModalOpen(false)}>
            <div className="modal" onClick={(e) => e.stopPropagation()}>
              <h3>Add New {activeTab.slice(0, -1)}</h3>
              {renderFormFields(newItem, setNewItem)}

              <div style={{ marginTop: "20px", display: "flex", gap: "10px" }}>
                <button onClick={() => handleAdd(activeTab)}>Save</button>
                <button className="cancel-btn" onClick={() => setIsAddModalOpen(false)}>Cancel</button>
              </div>
            </div>
          </div>
        )}

      </main>
    </div>
  );
}

export default AdminDashboard;
