import React from 'react';
import { Package } from 'lucide-react';

const Footer = () => {
  return (
    <footer className="bg-dark text-secondary py-4 mt-auto border-top border-secondary-subtle">
      <div className="container">
        <div className="row align-items-center">
          <div className="col-md-6 mb-3 mb-md-0">
            <h6 className="text-white d-flex align-items-center gap-2 mb-1">
              <Package size={18} className="text-warning" /> ShopSphere Platform
            </h6>
            <small className="text-muted">
              Full-Stack Modern E-Commerce Architecture: React Frontend + .NET 8 Web API + CQRS + EF Core & Redis.
            </small>
          </div>
          <div className="col-md-6 text-md-end">
            <span className="badge bg-primary me-1">React + Vite</span>
            <span className="badge bg-purple text-bg-secondary me-1">.NET 8 Web API</span>
            <span className="badge bg-danger me-1">Redis</span>
            <span className="badge bg-success">SQL Server</span>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
