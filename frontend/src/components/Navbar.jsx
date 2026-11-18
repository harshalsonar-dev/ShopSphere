import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { ShoppingCart, ShieldCheck, User, LogOut, Package, Grid } from 'lucide-react';

const Navbar = () => {
  const { user, isAuthenticated, isAdmin, logout } = useAuth();
  const { cart } = useCart();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark sticky-top shadow-sm py-3">
      <div className="container">
        <Link to="/" className="navbar-brand d-flex align-items-center fw-bold fs-4 text-white text-decoration-none">
          <span className="bg-warning text-dark rounded-3 px-2 py-1 me-2 fs-5">
            <Package size={22} className="mb-1" />
          </span>
          <span>Shop<span className="text-warning">Sphere</span></span>
        </Link>

        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#frontendNavbar"
        >
          <span className="navbar-toggler-icon"></span>
        </button>

        <div className="collapse navbar-collapse" id="frontendNavbar">
          <ul className="navbar-nav me-auto mb-2 mb-lg-0 ms-lg-3">
            <li className="nav-item">
              <Link to="/" className="nav-link text-light d-flex align-items-center gap-1">
                <Grid size={18} /> Products Catalog
              </Link>
            </li>
            {isAdmin && (
              <li className="nav-item">
                <Link to="/admin" className="nav-link text-warning fw-semibold d-flex align-items-center gap-1">
                  <ShieldCheck size={18} /> Admin Portal
                </Link>
              </li>
            )}
          </ul>

          <div className="d-flex align-items-center gap-3">
            {/* Cart Button */}
            <Link to="/cart" className="btn btn-outline-warning position-relative px-3 py-2 d-flex align-items-center gap-2">
              <ShoppingCart size={20} />
              <span className="d-none d-sm-inline">Cart</span>
              {cart.totalItems > 0 && (
                <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                  {cart.totalItems}
                </span>
              )}
            </Link>

            {/* User Profile / Auth State */}
            {isAuthenticated ? (
              <div className="dropdown">
                <button
                  className="btn btn-outline-light dropdown-toggle d-flex align-items-center gap-2"
                  type="button"
                  data-bs-toggle="dropdown"
                >
                  <User size={18} />
                  <span className="small">{user?.fullName || user?.email}</span>
                  <span className="badge bg-secondary">{user?.role}</span>
                </button>
                <ul className="dropdown-menu dropdown-menu-end shadow">
                  <li>
                    <Link to="/orders" className="dropdown-item d-flex align-items-center gap-2">
                      <Package size={16} /> My Orders
                    </Link>
                  </li>
                  {isAdmin && (
                    <li>
                      <Link to="/admin" className="dropdown-item d-flex align-items-center gap-2 text-warning">
                        <ShieldCheck size={16} /> Admin Dashboard
                      </Link>
                    </li>
                  )}
                  <li><hr className="dropdown-divider" /></li>
                  <li>
                    <button onClick={handleLogout} className="dropdown-item text-danger d-flex align-items-center gap-2">
                      <LogOut size={16} /> Sign Out
                    </button>
                  </li>
                </ul>
              </div>
            ) : (
              <div className="d-flex gap-2">
                <Link to="/login" className="btn btn-outline-light btn-sm px-3">
                  Sign In
                </Link>
                <Link to="/register" className="btn btn-warning btn-sm px-3 fw-semibold text-dark">
                  Register
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
