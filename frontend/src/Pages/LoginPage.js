import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import "./LoginPage.css";

function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");

    try {
      const response = await fetch("http://localhost:5178/api/Auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          username,
          password,
        }),
      });

      const data = await response.json();
      console.log("Login response:", data);

      if (!response.ok) {
        throw new Error(data.message || "Invalid credentials");
      }

      localStorage.setItem("user", JSON.stringify(data));

      switch (data.role) {
        case "Admin":
          navigate("/admin-dashboard");
          break;
        case "Doctor":
          navigate("/doctor-dashboard");
          break;
        case "Nurse":
          navigate("/nurse-dashboard");
          break;
        case "Patient":
          navigate("/patient-dashboard");
          break;
        default:
          navigate("/");
          break;
      }
    } catch (err) {
      console.error(err);
      setError("Login failed. Please check your username or password.");
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">

        {/* BACK BUTTON - TOP LEFT */}
        <Link to="/" className="back-btn top-left">← Back to Home</Link>

        <h2>DialyGo Login</h2>

        <form onSubmit={handleLogin}>
          <div className="form-group">
            <label>Username:</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label>Password:</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button type="submit" className="login-btn">
            Login
          </button>
        </form>

        {error && <p className="error-message">{error}</p>}
      </div>
    </div>
  );
}

export default LoginPage;
