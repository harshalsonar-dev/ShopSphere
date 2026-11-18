import React, { useState, useEffect } from 'react';
import api from '../api/client';
import { Package, DollarSign, AlertTriangle, Plus, Pencil, Trash2, CheckCircle2, RotateCcw } from 'lucide-react';

const AdminDashboard = () => {
  const [activeTab, setActiveTab] = useState('products');
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [statusMsg, setStatusMsg] = useState('');

  // Form State for Create/Edit Product
  const [showForm, setShowForm] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [currentId, setCurrentId] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    price: '',
    stockQuantity: '',
    categoryId: '',
  });

  const fetchData = async () => {
    setLoading(true);
    try {
      const [prodRes, catRes, orderRes] = await Promise.all([
        api.get('/products', { params: { pageSize: 100 } }),
        api.get('/categories'),
        api.get('/orders'),
      ]);

      if (prodRes.data?.data?.items) setProducts(prodRes.data.data.items);
      if (catRes.data?.data) setCategories(catRes.data.data);
      if (orderRes.data?.data) setOrders(orderRes.data.data);
    } catch (err) {
      console.error('Error fetching admin data', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const totalRevenue = orders
    .filter((o) => o.orderStatusName !== 'Cancelled')
    .reduce((sum, o) => sum + (o.totalAmount || 0), 0);

  const lowStockCount = products.filter((p) => p.stockQuantity < 10).length;

  const handleOpenCreate = () => {
    setIsEditing(false);
    setCurrentId(null);
    setFormData({
      name: '',
      description: '',
      price: '',
      stockQuantity: '',
      categoryId: categories[0]?.id || '',
    });
    setShowForm(true);
  };

  const handleOpenEdit = (prod) => {
    setIsEditing(true);
    setCurrentId(prod.id);
    setFormData({
      name: prod.name,
      description: prod.description,
      price: prod.price,
      stockQuantity: prod.stockQuantity,
      categoryId: prod.categoryId,
    });
    setShowForm(true);
  };

  const handleSaveProduct = async (e) => {
    e.preventDefault();
    try {
      const payload = {
        name: formData.name,
        description: formData.description,
        price: parseFloat(formData.price),
        stockQuantity: parseInt(formData.stockQuantity, 10),
        categoryId: parseInt(formData.categoryId, 10),
      };

      if (isEditing) {
        await api.put(`/products/${currentId}`, { id: currentId, ...payload });
        setStatusMsg(`Product #${currentId} updated successfully.`);
      } else {
        await api.post('/products', payload);
        setStatusMsg(`New product "${formData.name}" created successfully.`);
      }

      setShowForm(false);
      fetchData();
    } catch (err) {
      alert(err.response?.data?.message || 'Error saving product');
    }
  };

  const handleDeleteProduct = async (id) => {
    if (!window.confirm(`Are you sure you want to delete Product #${id}?`)) return;
    try {
      await api.delete(`/products/${id}`);
      setStatusMsg(`Product #${id} deleted successfully.`);
      fetchData();
    } catch (err) {
      alert(err.response?.data?.message || 'Error deleting product');
    }
  };

  const handleUpdateOrderStatus = async (orderId, newStatus) => {
    try {
      await api.put(`/orders/${orderId}/status`, { status: parseInt(newStatus, 10) });
      setStatusMsg(`Order #${orderId} status updated successfully.`);
      fetchData();
    } catch (err) {
      alert(err.response?.data?.message || 'Error updating status');
    }
  };

  return (
    <div className="container py-4">
      <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <div>
          <h2 className="fw-bold mb-1">Admin Management Portal</h2>
          <p className="text-muted mb-0">Control product catalog, monitor inventory health, and manage fulfillment.</p>
        </div>
        <button onClick={handleOpenCreate} className="btn btn-primary d-flex align-items-center gap-1 shadow-sm">
          <Plus size={18} /> Add New Product
        </button>
      </div>

      {statusMsg && (
        <div className="alert alert-success alert-dismissible fade show shadow-sm mb-4">
          <CheckCircle2 size={18} className="me-2" /> {statusMsg}
          <button type="button" className="btn-close" onClick={() => setStatusMsg('')}></button>
        </div>
      )}

      {/* Metrics Cards */}
      <div className="row g-4 mb-4">
        <div className="col-md-4">
          <div className="card border-0 shadow-sm rounded-4 p-4 bg-white">
            <div className="d-flex align-items-center">
              <div className="bg-primary-subtle text-primary rounded-4 p-3 me-3">
                <Package size={28} />
              </div>
              <div>
                <span className="text-muted small fw-semibold">Total Products</span>
                <h3 className="fw-bold mb-0">{products.length}</h3>
              </div>
            </div>
          </div>
        </div>

        <div className="col-md-4">
          <div className="card border-0 shadow-sm rounded-4 p-4 bg-white">
            <div className="d-flex align-items-center">
              <div className="bg-success-subtle text-success rounded-4 p-3 me-3">
                <DollarSign size={28} />
              </div>
              <div>
                <span className="text-muted small fw-semibold">Total Revenue</span>
                <h3 className="fw-bold mb-0">${totalRevenue.toFixed(2)}</h3>
              </div>
            </div>
          </div>
        </div>

        <div className="col-md-4">
          <div className="card border-0 shadow-sm rounded-4 p-4 bg-white">
            <div className="d-flex align-items-center">
              <div className="bg-warning-subtle text-warning rounded-4 p-3 me-3">
                <AlertTriangle size={28} />
              </div>
              <div>
                <span className="text-muted small fw-semibold">Low Stock Items</span>
                <h3 className="fw-bold mb-0">{lowStockCount}</h3>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <ul className="nav nav-pills mb-4 bg-white p-2 rounded-4 shadow-sm d-inline-flex">
        <li className="nav-item">
          <button
            className={`nav-link rounded-3 px-4 ${activeTab === 'products' ? 'active' : ''}`}
            onClick={() => setActiveTab('products')}
          >
            Products Management ({products.length})
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link rounded-3 px-4 ${activeTab === 'orders' ? 'active' : ''}`}
            onClick={() => setActiveTab('orders')}
          >
            Customer Orders ({orders.length})
          </button>
        </li>
      </ul>

      {/* Product Form Modal / View */}
      {showForm && (
        <div className="card border-0 shadow-sm rounded-4 p-4 mb-4 bg-white">
          <h5 className="fw-bold mb-3">{isEditing ? `Edit Product #${currentId}` : 'Add New Product'}</h5>
          <form onSubmit={handleSaveProduct}>
            <div className="row g-3">
              <div className="col-md-6">
                <label className="form-label small fw-semibold">Product Name</label>
                <input
                  type="text"
                  required
                  className="form-control"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                />
              </div>

              <div className="col-md-6">
                <label className="form-label small fw-semibold">Category</label>
                <select
                  required
                  className="form-select"
                  value={formData.categoryId}
                  onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                >
                  <option value="">Select Category...</option>
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="col-md-6">
                <label className="form-label small fw-semibold">Price ($)</label>
                <input
                  type="number"
                  step="0.01"
                  required
                  className="form-control"
                  value={formData.price}
                  onChange={(e) => setFormData({ ...formData, price: e.target.value })}
                />
              </div>

              <div className="col-md-6">
                <label className="form-label small fw-semibold">Stock Quantity</label>
                <input
                  type="number"
                  required
                  className="form-control"
                  value={formData.stockQuantity}
                  onChange={(e) => setFormData({ ...formData, stockQuantity: e.target.value })}
                />
              </div>

              <div className="col-12">
                <label className="form-label small fw-semibold">Description</label>
                <textarea
                  rows="3"
                  className="form-control"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                ></textarea>
              </div>
            </div>

            <div className="d-flex justify-content-end gap-2 mt-4">
              <button type="button" onClick={() => setShowForm(false)} className="btn btn-outline-secondary">
                Cancel
              </button>
              <button type="submit" className="btn btn-primary px-4">
                {isEditing ? 'Save Changes' : 'Create Product'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Tab 1: Products */}
      {activeTab === 'products' && (
        <div className="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
          <div className="table-responsive">
            <table className="table table-hover align-middle mb-0">
              <thead className="table-light">
                <tr>
                  <th className="ps-4">Product Name</th>
                  <th>Category</th>
                  <th>Price</th>
                  <th>Stock Quantity</th>
                  <th className="text-end pe-4">Actions</th>
                </tr>
              </thead>
              <tbody>
                {products.map((p) => (
                  <tr key={p.id}>
                    <td className="ps-4">
                      <div className="fw-bold">{p.name}</div>
                      <small className="text-muted d-block text-truncate" style={{ maxWidth: '300px' }}>
                        {p.description}
                      </small>
                    </td>
                    <td>
                      <span className="badge bg-light text-dark border">{p.categoryName}</span>
                    </td>
                    <td className="fw-bold">${p.price?.toFixed(2)}</td>
                    <td>
                      {p.stockQuantity <= 5 ? (
                        <span className="badge bg-danger rounded-pill px-3 py-1">Critical: {p.stockQuantity}</span>
                      ) : p.stockQuantity < 20 ? (
                        <span className="badge bg-warning text-dark rounded-pill px-3 py-1">Low: {p.stockQuantity}</span>
                      ) : (
                        <span className="badge bg-success-subtle text-success rounded-pill px-3 py-1">Healthy ({p.stockQuantity})</span>
                      )}
                    </td>
                    <td className="text-end pe-4">
                      <button
                        onClick={() => handleOpenEdit(p)}
                        className="btn btn-sm btn-outline-secondary me-2"
                        title="Edit"
                      >
                        <Pencil size={14} />
                      </button>
                      <button
                        onClick={() => handleDeleteProduct(p.id)}
                        className="btn btn-sm btn-outline-danger"
                        title="Delete"
                      >
                        <Trash2 size={14} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Tab 2: Orders */}
      {activeTab === 'orders' && (
        <div className="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
          <div className="table-responsive">
            <table className="table table-hover align-middle mb-0">
              <thead className="table-light">
                <tr>
                  <th className="ps-4">Order ID</th>
                  <th>Customer</th>
                  <th>Date</th>
                  <th>Total</th>
                  <th>Status</th>
                  <th className="text-end pe-4">Update Status</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((o) => (
                  <tr key={o.id}>
                    <td className="ps-4 fw-bold">#{o.id}</td>
                    <td>
                      <div className="fw-semibold">{o.customerName}</div>
                      <small className="text-muted">{o.customerEmail}</small>
                    </td>
                    <td>{new Date(o.createdAt).toLocaleDateString()}</td>
                    <td className="fw-bold">${o.totalAmount?.toFixed(2)}</td>
                    <td>
                      <span className="badge bg-primary rounded-pill px-3 py-1">{o.orderStatusName}</span>
                    </td>
                    <td className="text-end pe-4">
                      <select
                        className="form-select form-select-sm d-inline-block w-auto"
                        value={o.orderStatus}
                        onChange={(e) => handleUpdateOrderStatus(o.id, e.target.value)}
                      >
                        <option value="1">Pending</option>
                        <option value="2">Processing</option>
                        <option value="3">Shipped</option>
                        <option value="4">Delivered</option>
                        <option value="5">Cancelled</option>
                      </select>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminDashboard;
