import React, { useState } from 'react';
import { useCart } from '../context/CartContext';
import { ShoppingCart, Check, AlertCircle } from 'lucide-react';

const ProductCard = ({ product }) => {
  const { addToCart } = useCart();
  const [adding, setAdding] = useState(false);
  const [added, setAdded] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');

  const handleAdd = async () => {
    setAdding(true);
    setErrorMsg('');
    const res = await addToCart(product.id, 1);
    setAdding(false);
    if (res.success) {
      setAdded(true);
      setTimeout(() => setAdded(false), 2000);
    } else {
      setErrorMsg(res.message);
      setTimeout(() => setErrorMsg(''), 3000);
    }
  };

  const isOutOfStock = product.stockQuantity <= 0;

  return (
    <div className="card h-100 border-0 shadow-sm rounded-4 bg-white overflow-hidden transition-all">
      <div className="card-body d-flex flex-column p-4">
        <div className="d-flex justify-content-between align-items-start mb-2">
          <span className="badge bg-primary-subtle text-primary border border-primary-subtle rounded-pill px-3 py-1 small">
            {product.categoryName || 'General'}
          </span>
          {isOutOfStock ? (
            <span className="badge bg-danger-subtle text-danger border border-danger-subtle rounded-pill px-2 py-1 small">
              Out of Stock
            </span>
          ) : (
            <span className="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-2 py-1 small">
              In Stock ({product.stockQuantity})
            </span>
          )}
        </div>

        <h5 className="card-title fw-bold text-dark mt-2 mb-2">{product.name}</h5>
        <p className="card-text text-muted small flex-grow-1">{product.description}</p>

        {errorMsg && (
          <div className="alert alert-danger py-1 px-2 small mb-2 d-flex align-items-center gap-1">
            <AlertCircle size={14} /> {errorMsg}
          </div>
        )}

        <div className="d-flex justify-content-between align-items-center mt-3 pt-3 border-top">
          <div>
            <span className="text-muted small d-block">Price</span>
            <h4 className="fw-bold text-dark mb-0">${product.price?.toFixed(2)}</h4>
          </div>

          <button
            onClick={handleAdd}
            disabled={isOutOfStock || adding}
            className={`btn ${
              added ? 'btn-success' : 'btn-primary'
            } px-3 py-2 rounded-3 d-flex align-items-center gap-1`}
          >
            {added ? (
              <>
                <Check size={18} /> Added
              </>
            ) : adding ? (
              <span className="spinner-border spinner-border-sm" role="status"></span>
            ) : (
              <>
                <ShoppingCart size={18} /> Add
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ProductCard;
