import React, { createContext, useContext, useState, useEffect } from 'react';
import api from '../api/client';

const CartContext = createContext();

export const CartProvider = ({ children }) => {
  const [cart, setCart] = useState({ items: [], totalAmount: 0, totalItems: 0 });
  const [loading, setLoading] = useState(false);

  const refreshCart = async () => {
    try {
      const response = await api.get('/cart');
      if (response.data && response.data.data) {
        setCart(response.data.data);
      }
    } catch (err) {
      console.warn('Failed to fetch cart', err);
    }
  };

  useEffect(() => {
    refreshCart();
  }, []);

  const addToCart = async (productId, quantity = 1) => {
    setLoading(true);
    try {
      const response = await api.post('/cart/items', { productId, quantity });
      if (response.data && response.data.data) {
        setCart(response.data.data);
        return { success: true };
      }
      return { success: false, message: response.data.message };
    } catch (err) {
      const msg = err.response?.data?.message || 'Could not add item to cart';
      return { success: false, message: msg };
    } finally {
      setLoading(false);
    }
  };

  const updateQuantity = async (productId, quantity) => {
    try {
      const response = await api.put('/cart/items', { productId, quantity });
      if (response.data && response.data.data) {
        setCart(response.data.data);
        return { success: true };
      }
      return { success: false };
    } catch (err) {
      const msg = err.response?.data?.message || 'Could not update item quantity';
      return { success: false, message: msg };
    }
  };

  const removeItem = async (productId) => {
    try {
      const response = await api.delete(`/cart/items/${productId}`);
      if (response.data && response.data.data) {
        setCart(response.data.data);
        return { success: true };
      }
      return { success: false };
    } catch (err) {
      return { success: false };
    }
  };

  const clearCart = async () => {
    try {
      await api.delete('/cart');
      setCart({ items: [], totalAmount: 0, totalItems: 0 });
      return { success: true };
    } catch (err) {
      return { success: false };
    }
  };

  return (
    <CartContext.Provider
      value={{
        cart,
        loading,
        addToCart,
        updateQuantity,
        removeItem,
        clearCart,
        refreshCart,
      }}
    >
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => useContext(CartContext);
