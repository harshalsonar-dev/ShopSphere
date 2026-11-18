import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { UserPlus, User, Mail, Lock, AlertCircle, Shield } from 'lucide-react';

const RegisterPage = () => {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [role, setRole] = useState('Customer');
  const [errorMsg, setErrorMsg] = useState('');
  const { register, loading } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (password !== confirmPassword) {
      setErrorMsg('Passwords do not match');
      return;
    }
    setErrorMsg('');
    const res = await register(fullName, email, password, role);
    if (res.success) {
      navigate('/');
    } else {
      setErrorMsg(res.message);
    }
  };

  return (
    <div className="container py-5">
      <div className="row justify-content-center">
        <div className="col-md-6 col-lg-5">
          <div className="card border-0 shadow-sm rounded-4 p-4 p-md-5 bg-white">
            <div className="text-center mb-4">
              <div className="bg-warning-subtle text-warning-emphasis p-3 rounded-circle d-inline-flex mb-2">
                <UserPlus size={28} />
              </div>
              <h3 className="fw-bold">Create Account</h3>
              <p className="text-muted small">Join ShopSphere to buy tech or manage your store</p>
            </div>

            {errorMsg && (
              <div className="alert alert-danger py-2 small d-flex align-items-center gap-2 mb-3">
                <AlertCircle size={16} /> {errorMsg}
              </div>
            )}

            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label className="form-label small fw-semibold text-muted">Full Name</label>
                <div className="input-group">
                  <span className="input-group-text bg-white">
                    <User size={16} className="text-muted" />
                  </span>
                  <input
                    type="text"
                    required
                    className="form-control"
                    placeholder="John Doe"
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                  />
                </div>
              </div>

              <div className="mb-3">
                <label className="form-label small fw-semibold text-muted">Email Address</label>
                <div className="input-group">
                  <span className="input-group-text bg-white">
                    <Mail size={16} className="text-muted" />
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

              <div className="mb-3">
                <label className="form-label small fw-semibold text-muted">Account Role</label>
                <div className="input-group">
                  <span className="input-group-text bg-white">
                    <Shield size={16} className="text-muted" />
                  </span>
                  <select
                    className="form-select"
                    value={role}
                    onChange={(e) => setRole(e.target.value)}
                  >
                    <option value="Customer">Customer (Shop & Order)</option>
                    <option value="Admin">Admin (Full Management)</option>
                  </select>
                </div>
              </div>

              <div className="mb-3">
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

              <div className="mb-4">
                <label className="form-label small fw-semibold text-muted">Confirm Password</label>
                <div className="input-group">
                  <span className="input-group-text bg-white">
                    <Lock size={16} className="text-muted" />
                  </span>
                  <input
                    type="password"
                    required
                    className="form-control"
                    placeholder="••••••••"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="btn btn-warning w-100 py-2 fw-semibold text-dark d-flex align-items-center justify-content-center gap-2"
              >
                {loading ? <span className="spinner-border spinner-border-sm" role="status"></span> : <UserPlus size={18} />}
                Register Now
              </button>
            </form>

            <div className="text-center mt-4">
              <span className="small text-muted">Already have an account? </span>
              <Link to="/login" className="small fw-semibold text-decoration-none">
                Sign in here
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
