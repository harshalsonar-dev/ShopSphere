import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import api from '../api/client';
import { ShieldCheck, CheckCircle2, AlertCircle, ArrowLeft } from 'lucide-react';

const CheckoutPage = () => {
  const { cart, refreshCart } = useCart();
  const { isAuthenticated, user } = useAuth();
  const navigate = useNavigate();

  const [placing, setPlacing] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');

  if (!isAuthenticated) {
    return (
      <div className="container py-5 text-center">
        <div className="card border-0 shadow-sm rounded-4 p-5 max-w-md mx-auto">
          <h3 className="fw-bold mb-3">Authentication Required</h3>
          <p className="text-muted mb-4">Please sign in or create an account to finalize your order.</p>
          <div className="d-flex justify-content-center gap-3">
            <Link to="/login" className="btn btn-primary px-4">Sign In</Link>
            <Link to="/register" className="btn btn-outline-secondary px-4">Register</Link>
          </div>
        </div>
      </div>
    );
  }

  if (!cart.items || cart.items.length === 0) {
    return (
      <div className="container py-5 text-center">
        <div className="card border-0 shadow-sm rounded-4 p-5 max-w-md mx-auto">
          <h3 className="fw-bold mb-3">Your Cart is Empty</h3>
          <p className="text-muted mb-4">Add products to your cart before proceeding to checkout.</p>
          <Link to="/" className="btn btn-primary px-4">Browse Catalog</Link>
        </div>
      </div>
    );
  }

  const handleConfirmOrder = async () => {
    setPlacing(true);
    setErrorMsg('');
    try {
      const response = await api.post('/orders/checkout');
      if (response.data?.success) {
        await refreshCart();
        navigate('/orders', {
          state: { successMessage: `Order #${response.data.data.id} placed successfully! Real-time inventory has been deducted.` }
        });
      } else {
        setErrorMsg(response.data?.message || 'Checkout failed');
      }
    } catch (err) {
      setErrorMsg(err.response?.data?.message || 'Error occurred during checkout. Please check available stock.');
    } finally {
      setPlacing(false);
    }
  };

  return (
    <div className="container py-4">
      <div className="row justify-content-center">
        <div className="col-lg-8">
          <div className="card border-0 shadow-sm rounded-4 p-4 p-md-5 bg-white">
            <h3 className="fw-bold mb-1">Confirm & Place Order</h3>
            <p className="text-muted mb-4">
              Review your purchase details. Inventory is deducted automatically in a SQL Server transaction.
            </p>

            {errorMsg && (
              <div className="alert alert-danger d-flex align-items-center gap-2 mb-4">
                <AlertCircle size={18} /> {errorMsg}
              </div>
            )}

            {/* Customer Details */}
            <div className="bg-light p-3 rounded-3 mb-4">
              <h6 className="fw-bold mb-2">Customer Account</h6>
              <div className="small text-muted"><strong>Name:</strong> {user?.fullName}</div>
              <div className="small text-muted"><strong>Email:</strong> {user?.email}</div>
            </div>

            {/* Items Summary */}
            <div className="table-responsive mb-4">
              <table className="table align-middle">
                <thead className="table-light">
                  <tr>
                    <th>Item</th>
                    <th>Unit Price</th>
                    <th>Qty</th>
                    <th className="text-end">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  {cart.items.map((item) => (
                    <tr key={item.productId}>
                      <td className="fw-semibold">{item.productName}</td>
                      <td>${item.unitPrice?.toFixed(2)}</td>
                      <td>{item.quantity}</td>
                      <td className="text-end fw-bold">${item.subTotal?.toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr>
                    <td colSpan="3" className="text-end fw-bold fs-5">Total:</td>
                    <td className="text-end fw-bold fs-5 text-primary">${cart.totalAmount?.toFixed(2)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>

            <div className="alert alert-info d-flex align-items-center gap-3 mb-4">
              <ShieldCheck size={28} className="text-info flex-shrink-0" />
              <small className="mb-0">
                <strong>ACID Transaction Guarantee:</strong> Products are validated in real-time against SQL Server inventory.
                If any item has insufficient stock, the transaction is safely rolled back.
              </small>
            </div>

            <div className="d-flex justify-content-between align-items-center">
              <Link to="/cart" className="btn btn-outline-secondary d-flex align-items-center gap-1">
                <ArrowLeft size={16} /> Back to Cart
              </Link>

              <button
                onClick={handleConfirmOrder}
                disabled={placing}
                className="btn btn-success px-4 py-2 fs-5 fw-bold shadow-sm d-flex align-items-center gap-2"
              >
                {placing ? (
                  <>
                    <span className="spinner-border spinner-border-sm" role="status"></span>
                    Processing Transaction...
                  </>
                ) : (
                  <>
                    <CheckCircle2 size={20} /> Place Order (${cart.totalAmount?.toFixed(2)})
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CheckoutPage;
