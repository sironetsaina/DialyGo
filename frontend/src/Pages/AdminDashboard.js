import React, { useState, useEffect } from "react";
import "./AdminDashboard.css";

function AdminDashboard() {
  const [activeTab, setActiveTab] = useState("overview");
  const [data, setData] = useState([]);
  const [editItem, setEditItem] = useState(null);
  const [newItem, setNewItem] = useState({});
  const [searchTerm, setSearchTerm] = useState("");
  const [message, setMessage] = useState("");
  const [showAddForm, setShowAddForm] = useState(false);
  const [overviewMetrics, setOverviewMetrics] = useState({
    totalDoctors: 0,
    totalNurses: 0,
    totalPatients: 0,
    totalTrucks: 0,
    availableTrucks: 0,
    fullyBookedTrucks: 0,
  });
  const [notifications, setNotifications] = useState([]);

  const API_ADMIN = "http://localhost:5178/api/Admin";

  const showMsg = (msg) => {
    setMessage(msg);
    setTimeout(() => setMessage(""), 4000);
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
      case "doctors": return "doctorId";
      case "nurses": return "nurseId";
      case "patients": return "patientId";
      case "users": return "userId";
      case "trucks": return "truckId";
      default: return "id";
    }
  };

  const computeTruckMetrics = (trucks) => {
    let fullyBooked = 0;
    let available = 0;
    trucks.forEach(truck => {
      if (truck.bookingsCount >= truck.capacity) fullyBooked++;
      else available++;
    });
    return { fullyBooked, available };
  };

  const loadOverviewMetrics = async () => {
    try {
      const [metricsRes, trucksRes] = await Promise.all([
        fetch(`${API_ADMIN}/metrics`),
        fetch(`${API_ADMIN}/trucks`)
      ]);

      const metrics = metricsRes.ok ? await metricsRes.json() : {};
      const trucks = trucksRes.ok ? await trucksRes.json() : [];

      const { fullyBooked, available } = computeTruckMetrics(trucks);

      setOverviewMetrics({
        totalDoctors: metrics.totalDoctors || 0,
        totalNurses: metrics.totalNurses || 0,
        totalPatients: metrics.totalPatients || 0,
        totalTrucks: trucks.length,
        fullyBookedTrucks: fullyBooked,
        availableTrucks: available,
      });
    } catch (error) {
      console.error("Failed to load metrics:", error);
    }
  };

  const loadNotifications = async () => {
    try {
      const res = await fetch(`${API_ADMIN}/notifications`);
      if (res.ok) setNotifications(await res.json());
    } catch {
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
      const res = await fetch(`${API_ADMIN}/${tab}`);
      if (!res.ok) throw new Error();
      const result = await res.json();
      setData(Array.isArray(result) ? result : [result]);
    } catch {
      showMsg("X Failed to load data");
    }
  };

  useEffect(() => {
    if (activeTab === "overview") {
      loadOverviewMetrics();
      loadNotifications();
    } else {
      loadData(activeTab);
    }
    setShowAddForm(false);
    setEditItem(null);
  }, [activeTab]);

  const handleDelete = async (id) => {
    if (!window.confirm("Delete this record?")) return;
    try {
      const res = await fetch(`${API_ADMIN}/${activeTab}/${id}`, { method: "DELETE" });
      if (!res.ok) throw new Error();
      showMsg("Deleted successfully!");
      loadData(activeTab);
    } catch {
      showMsg("Delete failed.");
    }
  };

  const handleUpdate = async (id, tab) => {
    try {
      const res = await fetch(`${API_ADMIN}/${tab}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(editItem),
      });
      if (!res.ok) throw new Error();
      showMsg("Updated successfully!");
      setEditItem(null);
      loadData(tab);
    } catch {
      showMsg("Update failed.");
    }
  };

  const handleAdd = async () => {
    if (activeTab === "patients") {
      showMsg("Patients can only be viewed.");
      return;
    }
    try {
      const res = await fetch(`${API_ADMIN}/${activeTab}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newItem),
      });
      if (!res.ok) throw new Error();
      showMsg("Added successfully!");
      setNewItem({});
      setShowAddForm(false);
      loadData(activeTab);
    } catch {
      showMsg("Failed to add new record.");
    }
  };

  const filteredData = data.filter((item) => {
    const idField = getIdField();
    const term = searchTerm.toLowerCase();
    return (
      item[idField]?.toString().toLowerCase().includes(term) ||
      item.name?.toLowerCase().includes(term)
    );
  });

  const renderFormFields = (item, setItem) => {
    const handleChange = (key, value) => setItem({ ...item, [key]: value });
    switch (activeTab) {
      case "doctors":
      case "nurses":
        return (
          <>
            <input placeholder="Name" value={item.name || ""} onChange={(e) => handleChange("name", e.target.value)} />
            <input placeholder="Email" value={item.email || ""} onChange={(e) => handleChange("email", e.target.value)} />
            {activeTab === "doctors" && <input placeholder="Specialization" value={item.specialization || ""} onChange={(e) => handleChange("specialization", e.target.value)} />}
            <input placeholder="Phone Number" value={item.phoneNumber || ""} onChange={(e) => handleChange("phoneNumber", e.target.value)} />
          </>
        );
      case "users":
        return (
          <>
            <input placeholder="Username" value={item.username || ""} onChange={(e) => handleChange("username", e.target.value)} />
            <input placeholder="Password" type="password" value={item.password || ""} onChange={(e) => handleChange("password", e.target.value)} />
            <input placeholder="Role ID" value={item.roleId || ""} onChange={(e) => handleChange("roleId", e.target.value)} />
            <input placeholder="Related ID" value={item.relatedId || ""} onChange={(e) => handleChange("relatedId", e.target.value)} />
          </>
        );
      case "trucks":
        return (
          <>
            <input placeholder="License Plate" value={item.licensePlate || ""} onChange={(e) => handleChange("licensePlate", e.target.value)} />
            <input placeholder="Current Location" value={item.currentLocation || ""} onChange={(e) => handleChange("currentLocation", e.target.value)} />
            <input placeholder="Capacity" type="number" value={item.capacity || ""} onChange={(e) => handleChange("capacity", e.target.value)} />
            <input placeholder="Bookings Count" type="number" value={item.bookingsCount || 0} onChange={(e) => handleChange("bookingsCount", e.target.value)} />
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
                placeholder={`Search by name or ${getIdField()}...`}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
              {activeTab !== "patients" && (
                <button className="add-btn" onClick={() => setShowAddForm(!showAddForm)}>
                  {showAddForm ? "Cancel" : "Add New"}
                </button>
              )}
            </div>

            {showAddForm && activeTab !== "patients" && (
              <div className="form-card">
                <h3>Add New {activeTab.slice(0, -1)}</h3>
                {renderFormFields(newItem, setNewItem)}
                <button onClick={handleAdd}>Save</button>
              </div>
            )}

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
                  {filteredData.length > 0 ? (
                    filteredData.map((item) => (
                      <tr key={item[getIdField()]}>
                        {Object.keys(item).map((key, i) => <td key={i}>{item[key]?.toString()}</td>)}
                        {activeTab === "trucks" && (
                          <td>
                            {item.bookingsCount >= item.capacity ? (
                              <span style={{ color: "red", fontWeight: "bold" }}>Fully Booked</span>
                            ) : (
                              <span style={{ color: "green", fontWeight: "bold" }}>Available</span>
                            )}
                          </td>
                        )}
                        <td>
                          {activeTab === "patients" ? (
                            item.phoneNumber
                          ) : (
                            <>
                              <button
                                className="edit-btn"
                                onClick={() => setEditItem(item)}
                                disabled={activeTab === "trucks" && item.bookingsCount >= item.capacity}
                              >
                                Edit
                              </button>
                              <button className="delete-btn" onClick={() => handleDelete(item[getIdField()])}>
                                Delete
                              </button>
                            </>
                          )}
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr><td colSpan="100%">No records found</td></tr>
                  )}
                </tbody>
              </table>
            </div>

            {editItem && (
              <div className="form-card">
                <h3>Edit {activeTab.slice(0, -1)}</h3>
                {renderFormFields(editItem, setEditItem)}
                <button onClick={() => handleUpdate(editItem[getIdField()], activeTab)}>Save Changes</button>
                <button className="cancel-btn" onClick={() => setEditItem(null)}>Cancel</button>
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
}

export default AdminDashboard;
