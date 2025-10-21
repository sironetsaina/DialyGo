import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

function LoginPage() {
  const [Username, setUsername] = useState("");
  const [Password, setPassword] = useState("");
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
        Username: Username,
        Password: Password
      }),
    });

    const data = await response.json();
    console.log("Login response:", data);

    if (!response.ok) {
      throw new Error(data.message || "Invalid credentials");
    }

    localStorage.setItem("user", JSON.stringify(data));

    if (data.role === "Doctor") navigate("/doctor-dashboard");
    else if (data.role === "Admin") navigate("/admin-dashboard");
    else navigate("/");
  } catch (err) {
    console.error(err);
    setError("Login failed. Please check your username or password.");
  }
};

  return (
    <div className="login-container">
      <h2>DialyGo Login</h2>
      <form onSubmit={handleLogin}>
        <div>
          <label>Username:</label>
          <input
            type="text"
            value={Username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>
        <div>
          <label>Password:</label>
          <input
            type="Password"
            value={Password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <button type="submit">Login</button>
      </form>
      {error && <p style={{ color: "red" }}>{error}</p>}
    </div>
  );
}

export default LoginPage;
