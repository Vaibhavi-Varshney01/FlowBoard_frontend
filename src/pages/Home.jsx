import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { ArrowRight, Layout, Users, Zap, Shield, BarChart2, Globe, User } from 'lucide-react';
import './Home.css';

const Home = () => {
  const { isAuthenticated } = useSelector((state) => state.auth);
  const navigate = useNavigate();

  return (
    <div className="home-container">
      <div className="hero-section">
        <div className="hero-badge">✨ Project Management Reimagined</div>
        <h1 className="hero-title">
          Organise Work. <br/>
          <span className="text-gradient">Collaborate Seamlessly.</span>
        </h1>
        <p className="hero-subtitle">
          FlowBoard is the ultimate Kanban task management platform. Plan projects, 
          assign tasks, and deliver faster with your team.
        </p>
        <div className="hero-actions">
          {isAuthenticated ? (
            <Link to="/dashboard" className="btn btn-primary btn-large">
              Go to Dashboard <ArrowRight size={20} />
            </Link>
          ) : (
            <>
              <Link 
                to="/login" 
                className="btn btn-primary btn-large"
                onClick={() => localStorage.setItem('pendingRole', 'ADMIN')}
              >
                <Shield size={20} /> Login as Admin
              </Link>
              <Link 
                to="/login" 
                className="btn btn-secondary btn-large"
                onClick={() => localStorage.removeItem('pendingRole')}
              >
                <User size={20} /> Login as User
              </Link>
            </>
          )}
        </div>
      </div>

      <div className="features-grid">
        <div className="glass-panel feature-card">
          <div className="feature-icon"><Layout /></div>
          <h3>Visual Boards</h3>
          <p>Organize tasks with customizable drag-and-drop Kanban boards.</p>
        </div>
        <div className="glass-panel feature-card">
          <div className="feature-icon"><Users /></div>
          <h3>Team Workspaces</h3>
          <p>Collaborate with your team securely across multiple workspaces.</p>
        </div>
        <div className="glass-panel feature-card">
          <div className="feature-icon"><Zap /></div>
          <h3>Real-time Sync</h3>
          <p>See changes instantly with live updates and notifications.</p>
        </div>
        <div className="glass-panel feature-card">
          <div className="feature-icon"><Shield /></div>
          <h3>Secure by Default</h3>
          <p>JWT-based auth keeps your workspace private and protected.</p>
        </div>
        <div className="glass-panel feature-card">
          <div className="feature-icon"><BarChart2 /></div>
          <h3>Progress Tracking</h3>
          <p>Track card movement, due dates, and team velocity effortlessly.</p>
        </div>
        <div className="glass-panel feature-card">
          <div className="feature-icon"><Globe /></div>
          <h3>API-First Design</h3>
          <p>Built on a microservices architecture with a centralized API gateway.</p>
        </div>
      </div>
    </div>
  );
};

export default Home;
