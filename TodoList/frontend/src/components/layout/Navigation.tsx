import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const Navigation = () => {
  const location = useLocation();
  const { user, logout, isAuthenticated } = useAuth();

  const navItems = [
    { path: '/', label: 'Dashboard', icon: '🏠' },
    { path: '/todos', label: 'Todos', icon: '📝' },
    { path: '/categories', label: 'Categories', icon: '📁' },
    { path: '/about', label: 'About', icon: 'ℹ️' },
  ];

  return (
    <nav className="navbar">
      <div className="nav-container">
        <Link to="/" className="nav-logo">
          <span className="logo-icon">✅</span>
          <span className="logo-text">TodoList</span>
        </Link>
        
        <ul className="nav-menu">
          {navItems.map((item) => (
            <li key={item.path} className="nav-item">
              <Link
                to={item.path}
                className={`nav-link ${location.pathname === item.path ? 'active' : ''}`}
              >
                <span className="nav-icon">{item.icon}</span>
                <span className="nav-text">{item.label}</span>
              </Link>
            </li>
          ))}
        </ul>

        {/* User Profile Section */}
        {isAuthenticated && user && (
          <div className="user-profile">
            <div className="user-info">
              {user.picture && (
                <img
                  src={user.picture}
                  alt={user.name}
                  className="user-avatar"
                />
              )}
              <span className="user-name">{user.name}</span>
            </div>
            <button
              onClick={logout}
              className="logout-button"
              title="Logout"
            >
              Logout
            </button>
          </div>
        )}
      </div>
    </nav>
  );
};

export default Navigation;
