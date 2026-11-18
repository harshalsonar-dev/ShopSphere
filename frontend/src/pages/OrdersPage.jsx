import React, { useState, useEffect } from 'react';
import { useLocation, Link } from 'react-router-dom';
import api from '../api/client';
import { Package, Clock, CheckCircle2, XCircle, Truck, RotateCcw, AlertTriangle } from 'lucide-react';

const OrdersPage = () => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState('');
  const location = useLocation();

  useEffect(() => {
    if (location.state?.successMessage) {
      setMessage(location.state.successMessage);
    }
  }, [location.state]);

  const fetchOrders = async () => {
    setLoading(true);
    try {
      const res = await api.get('/orders/my-orders');
      if (res.data?.data) {
        setOrders(res.data.data);
      }
    } catch (err) {
      console.error('Failed to fetch orders', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
  }, []);

  const handleCancelOrder = async (orderId) => {
    if (!window.confirm('Are you sure you want to cancel this order? Reserved stock will be restocked immediately.')) {
      return;
    }

    try {
      const res = await api.post(`/orders/${orderId}/cancel`);
      if (res.data?.success) {
        setMessage(`Order #${orderId} was cancelled and inventory restocked.`);
        fetchOrders();
      }
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to cancel order.');
    }
  };

  const getStatusBadge = (statusName) => {
    switch (statusName) {
      case 'Pending':
        return <span className="badge bg-warning text-dark px-3 py-1 rounded-pill">Pending</span>;
      case 'Processing':
        return <span className="badge bg-info text-dark px-3 py-1 rounded-pill">Processing</span>;
      case 'Shipped':
        return <span className="badge bg-primary text-white px-3 py-1 rounded-pill">Shipped</span>;
      case 'Delivered':
        return <span className="badge bg-success text-white px-3 py-1 rounded-pill">Delivered</span>;
      case 'Cancelled':
        return <span className="badge bg-danger text-white px-3 py-1 rounded-pill">Cancelled</span>;
      default:
        return <span className="badge bg-secondary text-white px-3 py-1 rounded-pill">{statusName}</span>;
    }
  };

  return (
    <div className="container py-4">
      <div className="row mb-4">
        <div className="col-12">
          <h2 className="fw-bold mb-1">My Orders & Tracking</h2>
          <p className="text-muted">Monitor delivery progression, items purchased, and order statuses.</p>
        </div>
      </div>

      {message && (
        <div className="alert alert-success alert-dismissible fade show shadow-sm mb-4" role="alert">
          <CheckCircle2 size={18} className="me-2" /> {message}
          <button type="button" className="btn-close" onClick={() => setMessage('')}></button>
        </div>
      )}

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status"></div>
          <p className="text-muted mt-2">Loading your orders...</p>
        </div>
      ) : orders.length === 0 ? (
        <div className="card border-0 shadow-sm rounded-4 p-5 text-center bg-white">
          <Package size={56} className="text-muted mb-3 mx-auto" />
          <h4 className="fw-bold">No Orders Placed Yet</h4>
          <p className="text-muted mb-4">You have not completed any purchases yet.</p>
          <div>
            <Link to="/" className="btn btn-primary px-4">Browse Products</Link>
          </div>
        </div>
      ) : (
        <div className="d-flex flex-column gap-4">
          {orders.map((order) => {
            const isPending = order.orderStatus === 1 || order.orderStatusName === 'Pending';
            const isProcessing = order.orderStatus === 2 || order.orderStatusName === 'Processing';
            const isShipped = order.orderStatus === 3 || order.orderStatusName === 'Shipped';
            const isDelivered = order.orderStatus === 4 || order.orderStatusName === 'Delivered';
            const isCancelled = order.orderStatus === 5 || order.orderStatusName === 'Cancelled';

            return (
              <div key={order.id} className="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
                <div className="card-header bg-white py-3 px-4 d-flex justify-content-between align-items-center flex-wrap gap-2">
                  <div>
                    <h5 className="fw-bold mb-0">Order #{order.id}</h5>
                    <small className="text-muted">
                      Placed on {new Date(order.createdAt).toLocaleDateString()} at{' '}
                      {new Date(order.createdAt).toLocaleTimeString()}
                    </small>
                  </div>
                  <div className="d-flex align-items-center gap-3">
                    {getStatusBadge(order.orderStatusName)}
                    {(isPending || isProcessing) && (
                      <button
                        onClick={() => handleCancelOrder(order.id)}
                        className="btn btn-outline-danger btn-sm"
                      >
                        Cancel Order
                      </button>
                    )}
                  </div>
                </div>

                <div className="card-body p-4">
                  {/* Status Progress */}
                  {!isCancelled && (
                    <div className="bg-light p-3 rounded-4 mb-4">
                      <div className="row text-center">
                        <div className="col-3">
                          <div className={`p-2 rounded-circle d-inline-block text-white mb-1 ${isPending || isProcessing || isShipped || isDelivered ? 'bg-primary' : 'bg-secondary'}`}>
                            <Clock size={18} />
                          </div>
                          <div className="small fw-semibold">Pending</div>
                        </div>
                        <div className="col-3">
                          <div className={`p-2 rounded-circle d-inline-block text-white mb-1 ${isProcessing || isShipped || isDelivered ? 'bg-primary' : 'bg-secondary'}`}>
                            <RotateCcw size={18} />
                          </div>
                          <div className="small fw-semibold">Processing</div>
                        </div>
                        <div className="col-3">
                          <div className={`p-2 rounded-circle d-inline-block text-white mb-1 ${isShipped || isDelivered ? 'bg-primary' : 'bg-secondary'}`}>
                            <Truck size={18} />
                          </div>
                          <div className="small fw-semibold">Shipped</div>
                        </div>
                        <div className="col-3">
                          <div className={`p-2 rounded-circle d-inline-block text-white mb-1 ${isDelivered ? 'bg-success' : 'bg-secondary'}`}>
                            <CheckCircle2 size={18} />
                          </div>
                          <div className="small fw-semibold">Delivered</div>
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Order Items Table */}
                  <div className="table-responsive">
                    <table className="table align-middle mb-0">
                      <thead className="table-light">
                        <tr>
                          <th>Item</th>
                          <th>Price</th>
                          <th>Qty</th>
                          <th className="text-end">Subtotal</th>
                        </tr>
                      </thead>
                      <tbody>
                        {order.items?.map((item) => (
                          <tr key={item.id || item.productId}>
                            <td className="fw-semibold">{item.productName}</td>
                            <td>${item.unitPrice?.toFixed(2)}</td>
                            <td>{item.quantity}</td>
                            <td className="text-end fw-bold">${item.subTotal?.toFixed(2)}</td>
                          </tr>
                        ))}
                      </tbody>
                      <tfoot>
                        <tr>
                          <td colSpan="3" className="text-end fw-bold">Total Paid:</td>
                          <td className="text-end fw-bold fs-5 text-primary">${order.totalAmount?.toFixed(2)}</td>
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

export default OrdersPage;
