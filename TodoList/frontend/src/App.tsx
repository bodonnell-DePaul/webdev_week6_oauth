import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Layout from './components/layout/Layout';
import { TodoProvider } from './context/TodoContext';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import HomePage from './pages/HomePage';
import TodosPage from './pages/TodosPage';
import CategoriesPage from './pages/CategoriesPage';
import AboutPage from './pages/AboutPage';
import LoginPage from './pages/LoginPage';
import NotFoundPage from './pages/NotFoundPage';
import { testApiConnection } from './utils/apiTest';
import './index.css';

// Test API connection on app startup
testApiConnection();

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          
          {/* Protected routes - wrapped with TodoProvider */}
          <Route path="/" element={
            <ProtectedRoute>
              <TodoProvider>
                <Layout>
                  <HomePage />
                </Layout>
              </TodoProvider>
            </ProtectedRoute>
          } />
          
          <Route path="/todos" element={
            <ProtectedRoute>
              <TodoProvider>
                <Layout>
                  <TodosPage />
                </Layout>
              </TodoProvider>
            </ProtectedRoute>
          } />
          
          <Route path="/categories" element={
            <ProtectedRoute>
              <TodoProvider>
                <Layout>
                  <CategoriesPage />
                </Layout>
              </TodoProvider>
            </ProtectedRoute>
          } />
          
          <Route path="/about" element={
            <ProtectedRoute>
              <TodoProvider>
                <Layout>
                  <AboutPage />
                </Layout>
              </TodoProvider>
            </ProtectedRoute>
          } />
          
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;
