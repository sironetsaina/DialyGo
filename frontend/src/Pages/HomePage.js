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
         DialyGo is an innovative healthcare coordination platform designed to bring dialysis services closer to those who need them. 
         By streamlining communication, scheduling, and service access, DialyGo enhances how patients and care providers connect. 
         Our technology ensures smooth, timely interactions while supporting efficient mobile service delivery helping create a more responsive and accessible healthcare experience.
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
