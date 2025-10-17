import React, { useState } from "react";
import "./LoginPage.css";

export default function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const handleLogin = (e) => {
    e.preventDefault();
    console.log("Logging in:", { username, password });
    // TODO: integrate with your backend login endpoint
  };

  return (
    <div className="login-container">
      <h2>Login to DialyGo</h2>
      <form onSubmit={handleLogin}>
        <label>Username</label>
        <input
          type="text"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
        />

        <label>Password</label>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />

        <button type="submit" className="btn">Login</button>
      </form>
    </div>
  );
}
