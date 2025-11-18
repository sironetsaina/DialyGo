import React from "react";
import { Link } from "react-router-dom";
import "./HomePage.css";

export default function HomePage() {
  return (
    <div className="home-container">
      <header className="home-header">
        <h1>Welcome to DialyGo</h1>
        <p>Your mobile dialysis solution - bringing care closer to patients.</p>
      </header>

      <section className="home-about">
        <h2>About Us</h2>
        <p>
          DialyGo is a community-driven health platform designed to make dialysis
          accessible anywhere. We connect patients, doctors, and mobile dialysis
          units for efficient care scheduling and coordination.
        </p>
      </section>

      <div className="home-actions">
        <Link to="/login" className="btn">Login</Link>
      </div>

      <footer className="home-footer">
        <p>© 2025 DialyGo Health Solutions</p>
      </footer>
    </div>
  );
}
