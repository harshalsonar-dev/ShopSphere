import React, { useState, useEffect } from 'react';
import api from '../api/client';
import ProductCard from '../components/ProductCard';
import { Search, Filter, RotateCcw, ChevronLeft, ChevronRight, Sparkles } from 'lucide-react';

const HomePage = () => {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);

  // Filters state
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');
  const [pageIndex, setPageIndex] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Fetch Categories
  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const res = await api.get('/categories');
        if (res.data?.data) {
          setCategories(res.data.data);
        }
      } catch (err) {
        console.warn('Failed to load categories', err);
      }
    };
    fetchCategories();
  }, []);

  // Fetch Products with active filters
  const fetchProducts = async (page = 1) => {
    setLoading(true);
    try {
      const params = {
        pageIndex: page,
        pageSize: 6,
      };
      if (searchTerm.trim()) params.searchTerm = searchTerm.trim();
      if (categoryId) params.categoryId = categoryId;
      if (minPrice !== '') params.minPrice = minPrice;
      if (maxPrice !== '') params.maxPrice = maxPrice;

      const res = await api.get('/products', { params });
      if (res.data?.data) {
        setProducts(res.data.data.items || []);
        setTotalPages(res.data.data.totalPages || 1);
        setTotalCount(res.data.data.totalCount || 0);
        setPageIndex(page);
      }
    } catch (err) {
      console.error('Error fetching products', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts(1);
  }, [categoryId]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    fetchProducts(1);
  };

  const handleResetFilters = () => {
    setSearchTerm('');
    setCategoryId('');
    setMinPrice('');
    setMaxPrice('');
    setTimeout(() => fetchProducts(1), 0);
  };

  return (
    <div className="container py-4">
      {/* Hero Banner */}
      <div
        className="p-4 p-md-5 rounded-4 text-white shadow-sm mb-4"
        style={{ background: 'linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%)' }}
      >
        <div className="row align-items-center">
          <div className="col-lg-8">
            <span className="badge bg-warning text-dark mb-2 px-3 py-1 fw-semibold">
              <Sparkles size={14} className="me-1 mb-1" /> Ultra-Fast Redis Powered
            </span>
            <h1 className="display-6 fw-bold mb-2">ShopSphere High-Tech Marketplace</h1>
            <p className="lead text-white-50 mb-0">
              Explore dynamic product filtering, real-time inventory management, and instant checkout.
            </p>
          </div>
          <div className="col-lg-4 text-lg-end mt-3 mt-lg-0">
            <div className="bg-white bg-opacity-25 p-3 rounded-4 backdrop-blur d-inline-block text-start">
              <div className="small text-white-50">Active Inventory</div>
              <div className="fs-3 fw-bold">{totalCount} Products</div>
            </div>
          </div>
        </div>
      </div>

      <div className="row g-4">
        {/* Filters Sidebar */}
        <div className="col-lg-3">
          <div className="card border-0 shadow-sm rounded-4 p-4 sticky-top" style={{ top: '6rem' }}>
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="fw-bold mb-0 d-flex align-items-center gap-2">
                <Filter size={18} className="text-primary" /> Filters
              </h5>
              <button onClick={handleResetFilters} className="btn btn-sm btn-link text-decoration-none text-muted p-0 d-flex align-items-center gap-1">
                <RotateCcw size={14} /> Reset
              </button>
            </div>

            <form onSubmit={handleSearchSubmit}>
              {/* Keyword Search */}
              <div className="mb-3">
                <label className="form-label small fw-semibold text-muted">Search Keywords</label>
                <div className="input-group">
                  <span className="input-group-text bg-white border-end-0">
                    <Search size={16} className="text-muted" />
                  </span>
                  <input
                    type="text"
                    className="form-control border-start-0 ps-0"
                    placeholder="e.g. 4K, Wireless, Mouse"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                </div>
              </div>

              {/* Category Selector */}
              <div className="mb-3">
                <label className="form-label small fw-semibold text-muted">Category</label>
                <select
                  className="form-select"
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                >
                  <option value="">All Categories</option>
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </div>

              {/* Price Range */}
              <div className="mb-4">
                <label className="form-label small fw-semibold text-muted">Price Range ($)</label>
                <div className="row g-2">
                  <div className="col-6">
                    <input
                      type="number"
                      step="0.01"
                      className="form-control form-control-sm"
                      placeholder="Min"
                      value={minPrice}
                      onChange={(e) => setMinPrice(e.target.value)}
                    />
                  </div>
                  <div className="col-6">
                    <input
                      type="number"
                      step="0.01"
                      className="form-control form-control-sm"
                      placeholder="Max"
                      value={maxPrice}
                      onChange={(e) => setMaxPrice(e.target.value)}
                    />
                  </div>
                </div>
              </div>

              <button type="submit" className="btn btn-primary w-100 fw-semibold py-2">
                Apply Filters
              </button>
            </form>
          </div>
        </div>

        {/* Product Catalog Grid */}
        <div className="col-lg-9">
          {loading ? (
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading products...</span>
              </div>
              <p className="text-muted mt-2">Loading products from SQL Server / Redis cache...</p>
            </div>
          ) : products.length === 0 ? (
            <div className="card border-0 shadow-sm rounded-4 p-5 text-center">
              <h4 className="fw-bold">No Products Found</h4>
              <p className="text-muted">No products matched your search or filter bounds.</p>
              <div>
                <button onClick={handleResetFilters} className="btn btn-outline-primary px-4">
                  Clear All Filters
                </button>
              </div>
            </div>
          ) : (
            <>
              <div className="row row-cols-1 row-cols-md-2 row-cols-xl-3 g-4">
                {products.map((product) => (
                  <div className="col" key={product.id}>
                    <ProductCard product={product} />
                  </div>
                ))}
              </div>

              {/* Pagination Controls */}
              {totalPages > 1 && (
                <div className="d-flex justify-content-center mt-5">
                  <nav>
                    <ul className="pagination shadow-sm">
                      <li className={`page-item ${pageIndex <= 1 ? 'disabled' : ''}`}>
                        <button
                          className="page-link d-flex align-items-center gap-1"
                          onClick={() => fetchProducts(pageIndex - 1)}
                          disabled={pageIndex <= 1}
                        >
                          <ChevronLeft size={16} /> Prev
                        </button>
                      </li>
                      {[...Array(totalPages)].map((_, i) => (
                        <li
                          key={i + 1}
                          className={`page-item ${pageIndex === i + 1 ? 'active' : ''}`}
                        >
                          <button
                            className="page-link"
                            onClick={() => fetchProducts(i + 1)}
                          >
                            {i + 1}
                          </button>
                        </li>
                      ))}
                      <li className={`page-item ${pageIndex >= totalPages ? 'disabled' : ''}`}>
                        <button
                          className="page-link d-flex align-items-center gap-1"
                          onClick={() => fetchProducts(pageIndex + 1)}
                          disabled={pageIndex >= totalPages}
                        >
                          Next <ChevronRight size={16} />
                        </button>
                      </li>
                    </ul>
                  </nav>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default HomePage;
