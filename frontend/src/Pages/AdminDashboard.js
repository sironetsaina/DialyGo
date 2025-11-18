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

  const API_ADMIN = "http://localhost:5178/api/Admin";

  const showMsg = (msg) => {
    setMessage(msg);
    setTimeout(() => setMessage(""), 4000);
  };

  const handleBack = () => {
    window.history.back(); // 🔙 go to the previous page
  };

  const handleLogout = () => {
    if (window.confirm("Are you sure you want to log out?")) {
      localStorage.clear();
      window.location.href = "/"; // redirect to homepage or login
    }
  };

  // Load data depending on tab
  const loadData = async (tab) => {
    let url = "";
    switch (tab) {
      case "doctors":
        url = `${API_ADMIN}/doctors`;
        break;
      case "nurses":
        url = `${API_ADMIN}/nurses`;
        break;
      case "patients":
        url = `${API_ADMIN}/patients`;
        break;
      case "users":
        url = `${API_ADMIN}/users`;
        break;
      case "trucks":
        url = `${API_ADMIN}/trucks`;
        break;
      default:
        setData([]);
        return;
    }

    try {
      const res = await fetch(url);
      if (!res.ok) throw new Error();
      const result = await res.json();
      setData(Array.isArray(result) ? result : [result]);
    } catch {
      showMsg("X Failed to load data");
    }
  };

  useEffect(() => {
    if (activeTab !== "overview") loadData(activeTab);
    setShowAddForm(false);
    setEditItem(null);
  }, [activeTab]);

  const handleDelete = async (id) => {
    if (!window.confirm("Delete this record?")) return;
    try {
      const res = await fetch(`${API_ADMIN}/${activeTab}/${id}`, {
        method: "DELETE",
      });
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
      showMsg(" Added successfully!");
      setNewItem({});
      setShowAddForm(false);
      loadData(activeTab);
    } catch {
      showMsg("Failed to add new record.");
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
        return (
          <>
            <input placeholder="Name" value={item.name || ""} onChange={(e) => handleChange("name", e.target.value)} />
            <input placeholder="Email" value={item.email || ""} onChange={(e) => handleChange("email", e.target.value)} />
            <input placeholder="Specialization" value={item.specialization || ""} onChange={(e) => handleChange("specialization", e.target.value)} />
            <input placeholder="Phone Number" value={item.phoneNumber || ""} onChange={(e) => handleChange("phoneNumber", e.target.value)} />
          </>
        );
      case "nurses":
        return (
          <>
            <input placeholder="Name" value={item.name || ""} onChange={(e) => handleChange("name", e.target.value)} />
            <input placeholder="Email" value={item.email || ""} onChange={(e) => handleChange("email", e.target.value)} />
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
          </>
        );
      default:
        return <p>No editable fields for this category.</p>;
    }
  };

  return (
    <div className="admin-dashboard">
      {/* Sidebar */}
      <aside className="sidebar">
        <h2>DialyGo Admin</h2>
        <ul>
          {["overview", "doctors", "nurses", "patients", "users", "trucks"].map((tab) => (
            <li
              key={tab}
              className={activeTab === tab ? "active" : ""}
              onClick={() => setActiveTab(tab)}
            >
              {tab === "overview"
                ? "Overview"
                : tab === "doctors"
                ? " Doctors"
                : tab === "nurses"
                ? " Nurses"
                : tab === "patients"
                ? " Patients"
                : tab === "users"
                ? " Users"
                : "Trucks"}
            </li>
          ))}
        </ul>
        <button className="logout-btn" onClick={handleLogout}>🚪 Logout</button>
      </aside>

      {/* Main Content */}
      <main className="main-content">
        {/* 🔙 Back Button */}
        <button className="back-arrow" onClick={handleBack}>← Back</button>

        <header>
          <h1>Welcome, Admin 👋</h1>
          <p>Manage users, staff, patients, and trucks here.</p>
        </header>

        {message && <div className="alert">{message}</div>}

        {activeTab === "overview" ? (
          <div className="overview">
            <h2>System Overview</h2>
            <p>Use the sidebar to manage DialyGo data.</p>
          </div>
        ) : (
          <>
            <div className="top-controls">
              <input
                type="text"
                placeholder={`🔍 Search by name or ${getIdField()}...`}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
              {activeTab !== "patients" && (
                <button className="add-btn" onClick={() => setShowAddForm(!showAddForm)}>
                  {showAddForm ? "➖ Cancel" : "➕ Add New"}
                </button>
              )}
            </div>

            {showAddForm && activeTab !== "patients" && (
              <div className="form-card">
                <h3>Add New {activeTab.slice(0, -1)}</h3>
                {renderFormFields(newItem, setNewItem)}
                <button onClick={handleAdd}> Save</button>
              </div>
            )}

            <div className="data-table">
              <table>
                <thead>
                  <tr>
                    {data[0] && Object.keys(data[0]).map((key) => (
                      <th key={key}>{key}</th>
                    ))}
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredData.length > 0 ? (
                    filteredData.map((item) => (
                      <tr key={item[getIdField()]}>
                        {Object.values(item).map((val, i) => (
                          <td key={i}>{val?.toString()}</td>
                        ))}
                        <td>
                          {activeTab !== "patients" && (
                            <>
                              <button className="edit-btn" onClick={() => setEditItem(item)}>
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
                    <tr>
                      <td colSpan="100%">No records found</td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            {editItem && (
              <div className="form-card">
                <h3>Edit {activeTab.slice(0, -1)}</h3>
                {renderFormFields(editItem, setEditItem)}
                <button onClick={() => handleUpdate(editItem[getIdField()], activeTab)}>
                  💾 Save Changes
                </button>
                <button className="cancel-btn" onClick={() => setEditItem(null)}>
                   Cancel
                </button>
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
}

export default AdminDashboard;
