import React from 'react';
import { Link } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { Trash2, Plus, Minus, ArrowLeft, ShieldCheck, ShoppingBag } from 'lucide-react';

const CartPage = () => {
  const { cart, updateQuantity, removeItem, clearCart } = useCart();

  if (!cart.items || cart.items.length === 0) {
    return (
      <div className="container py-5 text-center">
        <div className="card border-0 shadow-sm rounded-4 p-5 max-w-md mx-auto">
          <div className="py-4">
            <ShoppingBag size={56} className="text-muted mb-3" />
            <h3 className="fw-bold">Your Cart is Empty</h3>
            <p className="text-muted mb-4">Discover our catalog and add top-tier hardware to your cart.</p>
            <Link to="/" className="btn btn-primary px-4 py-2">
              <ArrowLeft size={16} className="me-2 mb-1" /> Start Shopping
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container py-4">
      <div className="row mb-4">
        <div className="col-12">
          <h2 className="fw-bold mb-1">Your Shopping Cart</h2>
          <p className="text-muted">Review items stored in your Redis distributed cart session.</p>
        </div>
      </div>

      <div className="row g-4">
        {/* Cart Items Table */}
        <div className="col-lg-8">
          <div className="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
            <div className="table-responsive">
              <table className="table align-middle mb-0">
                <thead className="table-light">
                  <tr>
                    <th className="ps-4">Product</th>
                    <th>Price</th>
                    <th style={{ width: '160px' }}>Quantity</th>
                    <th>Subtotal</th>
                    <th className="text-end pe-4">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {cart.items.map((item) => (
                    <tr key={item.productId}>
                      <td className="ps-4">
                        <div className="fw-bold text-dark">{item.productName}</div>
                        <small className="text-muted">ID: #{item.productId}</small>
                      </td>
                      <td>${item.unitPrice?.toFixed(2)}</td>
                      <td>
                        <div className="d-flex align-items-center gap-1">
                          <button
                            onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                            className="btn btn-sm btn-outline-secondary p-1"
                          >
                            <Minus size={14} />
                          </button>
                          <span className="px-3 fw-semibold">{item.quantity}</span>
                          <button
                            onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                            className="btn btn-sm btn-outline-secondary p-1"
                          >
                            <Plus size={14} />
                          </button>
                        </div>
                      </td>
                      <td className="fw-bold text-dark">${item.subTotal?.toFixed(2)}</td>
                      <td className="text-end pe-4">
                        <button
                          onClick={() => removeItem(item.productId)}
                          className="btn btn-sm btn-outline-danger p-1"
                          title="Remove item"
                        >
                          <Trash2 size={16} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="card-footer bg-white p-3 d-flex justify-content-between align-items-center">
              <Link to="/" className="btn btn-outline-secondary btn-sm d-flex align-items-center gap-1">
                <ArrowLeft size={16} /> Continue Shopping
              </Link>
              <button onClick={clearCart} className="btn btn-outline-danger btn-sm d-flex align-items-center gap-1">
                <Trash2 size={16} /> Clear Cart
              </button>
            </div>
          </div>
        </div>

        {/* Cart Summary */}
        <div className="col-lg-4">
          <div className="card border-0 shadow-sm rounded-4 p-4 bg-white">
            <h5 className="fw-bold mb-4">Summary</h5>

            <div className="d-flex justify-content-between mb-2">
              <span className="text-muted">Subtotal ({cart.totalItems} items)</span>
              <span className="fw-semibold">${cart.totalAmount?.toFixed(2)}</span>
            </div>
            <div className="d-flex justify-content-between mb-2">
              <span className="text-muted">Shipping</span>
              <span className="text-success fw-semibold">FREE</span>
            </div>
            <div className="d-flex justify-content-between mb-3">
              <span className="text-muted">Estimated Tax</span>
              <span className="fw-semibold">$0.00</span>
            </div>

            <hr />

            <div className="d-flex justify-content-between align-items-center mb-4">
              <span className="fs-5 fw-bold">Total</span>
              <span className="fs-4 fw-bold text-primary">${cart.totalAmount?.toFixed(2)}</span>
            </div>

            <Link to="/checkout" className="btn btn-warning w-100 py-3 fw-bold text-dark shadow-sm">
              Proceed to Checkout
            </Link>

            <div className="mt-3 text-center">
              <small className="text-muted d-flex align-items-center justify-content-center gap-1">
                <ShieldCheck size={16} className="text-success" />
                Atomic inventory deduction on checkout
              </small>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CartPage;
