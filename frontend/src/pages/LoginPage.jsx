import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogIn, User, Lock, AlertCircle, Shield, ShoppingCart } from 'lucide-react';

const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errorMsg, setErrorMsg] = useState('');
  const { login, loading } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrorMsg('');
    const res = await login(email, password);
    if (res.success) {
      navigate('/');
    } else {
      setErrorMsg(res.message);
    }
  };

  const handleFillDemo = (demoEmail, demoPwd) => {
    setEmail(demoEmail);
    setPassword(demoPwd);
  };

  return (
    <div className="container py-5">
      <div className="row justify-content-center">
        <div className="col-md-6 col-lg-5">
          <div className="card border-0 shadow-sm rounded-4 p-4 p-md-5 bg-white">
            <div className="text-center mb-4">
              <div className="bg-primary-subtle text-primary p-3 rounded-circle d-inline-flex mb-2">
                <LogIn size={28} />
              </div>
              <h3 className="fw-bold">Sign In to ShopSphere</h3>
              <p className="text-muted small">Access your shopping cart, orders, and admin controls</p>
            </div>

            {errorMsg && (
              <div className="alert alert-danger py-2 small d-flex align-items-center gap-2 mb-3">
                <AlertCircle size={16} /> {errorMsg}
              </div>
            )}

            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label className="form-label small fw-semibold text-muted">Email Address</label>
                <div className="input-group">
                  <span className="input-group-text bg-white">
                    <User size={16} className="text-muted" />
                  </span>
                  <input
                    type="email"
                    required
                    className="form-control"
                    placeholder="name@example.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                  />
                </div>
              </div>

              <div className="mb-4">
                <label className="form-label small fw-semibold text-muted">Password</label>
                <div className="input-group">
                  <span className="input-group-text bg-white">
                    <Lock size={16} className="text-muted" />
                  </span>
                  <input
                    type="password"
                    required
                    className="form-control"
                    placeholder="••••••••"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="btn btn-primary w-100 py-2 fw-semibold d-flex align-items-center justify-content-center gap-2"
              >
                {loading ? <span className="spinner-border spinner-border-sm" role="status"></span> : <LogIn size={18} />}
                Sign In
              </button>
            </form>

            <hr className="my-4 text-muted" />

            <div className="text-center">
              <small className="text-muted d-block mb-2">Quick Demo One-Click Fill:</small>
              <div className="d-flex justify-content-center gap-2">
                <button
                  type="button"
                  onClick={() => handleFillDemo('admin@shopsphere.com', 'Admin@123')}
                  className="btn btn-sm btn-outline-secondary d-flex align-items-center gap-1"
                >
                  <Shield size={14} /> Admin
                </button>
                <button
                  type="button"
                  onClick={() => handleFillDemo('customer@shopsphere.com', 'Customer@123')}
                  className="btn btn-sm btn-outline-secondary d-flex align-items-center gap-1"
                >
                  <ShoppingCart size={14} /> Customer
                </button>
              </div>
            </div>

            <div className="text-center mt-4">
              <span className="small text-muted">Don't have an account? </span>
              <Link to="/register" className="small fw-semibold text-decoration-none">
                Create one now
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
