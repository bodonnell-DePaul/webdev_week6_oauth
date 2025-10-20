# TodoList Full-Stack Application Integration Guide

The TodoList application now features a complete full-stack architecture with a React TypeScript frontend communicating with a .NET 8 Minimal API backend.

## 🏗️ Architecture Overview

### Frontend (React + TypeScript)
- **Framework**: React 18 with TypeScript
- **State Management**: React Context API
- **Routing**: React Router v6
- **API Communication**: Fetch API with custom service layer
- **UI**: Modern CSS with responsive design

### Backend (.NET 8 Minimal API)
- **Framework**: .NET 8 with Minimal APIs
- **Data Storage**: In-memory storage (development)
- **Documentation**: Swagger/OpenAPI
- **CORS**: Configured for frontend integration

## 🚀 Getting Started

### Prerequisites
- Node.js 18+ and npm
- .NET 8 SDK
- Modern web browser

### Quick Start

1. **Start the Backend API**
```bash
cd backend/TodoListApi
dotnet run --urls "http://localhost:5137"
```

2. **Start the Frontend** (in a new terminal)
```bash
cd TodoList/frontend
npm install
npm run dev
```

3. **Access the Applications**
- Frontend: http://localhost:5173
- Backend API: http://localhost:5137
- Swagger UI: http://localhost:5137 (API documentation)

## 🔐 OAuth Authentication

This application implements OAuth 2.0 authentication with **Google** and **Microsoft** providers. See detailed documentation:

- **[OAuth Setup Guide](OAUTH_SETUP_GUIDE.md)** - Step-by-step provider configuration
- **[OAuth Implementation Guide](OAUTH_IMPLEMENTATION_GUIDE.md)** - Complete code walkthrough
- **[OAuth Quick Reference](OAUTH_QUICK_REFERENCE.md)** - Quick reference card

### Key Configuration Notes

⚠️ **Critical Setup Requirements:**

1. **Microsoft OAuth Redirect URI**: Must be `http://localhost:5173/login` (not just root path)
2. **Azure Platform**: Must be set to "Web" (not "Single-page application")
3. **API Permissions**: Require `User.Read`, `openid`, `profile`, `email` with admin consent
4. **React Strict Mode**: Use sessionStorage to prevent duplicate code exchanges
5. **Provider Scoping**: TodoProvider should only wrap protected routes, not LoginPage

See the troubleshooting sections in the OAuth documentation for detailed solutions to common issues.

## 🔗 API Integration

### Frontend Service Layer (`frontend/src/services/api.ts`)

The frontend communicates with the backend through a dedicated service layer:

```typescript
// Todo operations
await todoApi.getAllTodos()      // GET /api/todos
await todoApi.createTodo(todo)   // POST /api/todos
await todoApi.updateTodo(id, updates) // PUT /api/todos/{id}
await todoApi.deleteTodo(id)     // DELETE /api/todos/{id}
await todoApi.toggleTodo(id)     // PATCH /api/todos/{id}/toggle

// Category operations
await categoryApi.getAllCategories() // GET /api/categories
await categoryApi.createCategory(category) // POST /api/categories
await categoryApi.updateCategory(id, updates) // PUT /api/categories/{id}
await categoryApi.deleteCategory(id) // DELETE /api/categories/{id}

// Statistics
await todoApi.getStats()         // GET /api/stats
```

### Data Flow

1. **Application Startup**:
   - Frontend loads with empty state
   - TodoContext fetches initial data from API
   - Loading states display during data fetch
   - Error handling for failed requests

2. **User Interactions**:
   - All CRUD operations go through API calls
   - Optimistic UI updates with error rollback
   - Real-time statistics refresh after changes
   - Form submissions handle async operations

3. **State Management**:
   - Context API manages global state
   - API responses update local state
   - No localStorage dependency (server is source of truth)

## 📡 API Endpoints

### Todos API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/todos` | Get all todos |
| GET | `/api/todos/{id}` | Get specific todo |
| POST | `/api/todos` | Create new todo |
| PUT | `/api/todos/{id}` | Update todo |
| DELETE | `/api/todos/{id}` | Delete todo |
| PATCH | `/api/todos/{id}/toggle` | Toggle completion |

### Categories API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | Get all categories |
| GET | `/api/categories/{id}` | Get specific category |
| POST | `/api/categories` | Create new category |
| PUT | `/api/categories/{id}` | Update category |
| DELETE | `/api/categories/{id}` | Delete category |

### Statistics & Health
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/stats` | Get comprehensive statistics |
| GET | `/health` | API health check |

## 🔄 State Management Changes

### Before (LocalStorage)
```typescript
const [todos, setTodos] = useLocalStorage<Todo[]>('todos', initialTodos);
```

### After (API Integration)
```typescript
const [todos, setTodos] = useState<Todo[]>([]);

useEffect(() => {
  const loadTodos = async () => {
    const data = await todoApi.getAllTodos();
    setTodos(data);
  };
  loadTodos();
}, []);
```

## 🛠️ Development Features

### Error Handling
- API request failures are handled gracefully
- Error states display in UI with retry options
- Console logging for debugging
- Network error resilience

### Loading States
- Loading indicators during API calls
- Skeleton screens for better UX
- Async operation feedback
- Optimistic UI updates

### CORS Configuration
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

## 🧪 Testing the Integration

### API Connection Test
The application includes an API connection test utility:

```typescript
import { testApiConnection } from './utils/apiTest';

// Test all API endpoints
const success = await testApiConnection();
```

### Manual Testing
1. **Health Check**: `curl http://localhost:5137/health`
2. **Get Todos**: `curl http://localhost:5137/api/todos`
3. **Create Todo**: Use the frontend form or Swagger UI
4. **Frontend**: Navigate through all pages to verify data loading

## 📊 Data Models

### Todo Model
```typescript
interface Todo {
  id: string;
  title: string;
  description: string;
  priority: 'low' | 'medium' | 'high' | 'urgent';
  category: string;
  isCompleted: boolean;
  createdDate: Date;
  dueDate?: Date;
  tags: string[];
}
```

### Category Model
```typescript
interface Category {
  id: string;
  name: string;
  description: string;
  color: string;
  todoCount: number;
}
```

## 🔧 Configuration

### Frontend Configuration
- API base URL: `http://localhost:5137/api`
- Development server: `http://localhost:5173`
- Hot reload enabled
- TypeScript strict mode

### Backend Configuration
- HTTP port: 5137
- HTTPS port: 7153 (with dev certificate)
- Swagger UI at root path
- CORS enabled for frontend origins

## 🚀 Deployment Considerations

### Frontend
- Build: `npm run build`
- Serve: `npm run preview`
- Environment variables for API URL
- Static file hosting ready

### Backend
- Build: `dotnet build`
- Run: `dotnet run`
- Docker support included
- Production configuration ready

## 📈 Performance Features

### Frontend Optimizations
- Memoized filtering with `useTodoFilter`
- Efficient re-renders with proper dependencies
- Lazy loading potential for large datasets
- Optimistic UI updates

### Backend Optimizations
- Minimal API overhead
- In-memory storage for fast development
- Swagger UI only in development
- Efficient CORS handling

## 🔮 Future Enhancements

### Database Integration
- ~~Replace in-memory storage with Entity Framework~~ ✅ **IMPLEMENTED!**
- ~~SQL Server or PostgreSQL support~~ ✅ **IMPLEMENTED!** (SQLite)
- ~~Database migrations~~ ✅ **IMPLEMENTED!**
- ~~Connection string configuration~~ ✅ **IMPLEMENTED!**

### Authentication
- ~~JWT token-based authentication~~ ✅ **IMPLEMENTED!**
- ~~User registration and login~~ ✅ **IMPLEMENTED!** (via Google & Microsoft OAuth)
- ~~Protected routes~~ ✅ **IMPLEMENTED!**
- ~~User-specific todos~~ ✅ **IMPLEMENTED!**
- ~~Refresh token rotation~~ ✅ **IMPLEMENTED!**

### Real-time Updates
- SignalR for real-time synchronization
- WebSocket connections
- Live collaboration features
- Push notifications

### Caching & Performance
- Redis caching layer
- API response caching
- Frontend state persistence
- Service worker for offline support

---

## ✅ Integration Checklist

- [x] Backend API running on port 5001
- [x] Frontend connecting to backend API
- [x] CORS configured properly
- [x] All CRUD operations working
- [x] Error handling implemented
- [x] Loading states added
- [x] Statistics API integration
- [x] Category management via API
- [x] Todo filtering through API data
- [x] Health check endpoint working

The TodoList application now demonstrates a complete full-stack architecture with professional-grade API integration, error handling, and state management patterns.
