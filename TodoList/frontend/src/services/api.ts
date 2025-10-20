import axios from 'axios';
import { Todo, Category, TodoStats, Priority, WeatherInfo, WeatherForecast5Day } from '../types';

const API_BASE_URL = 'http://localhost:5001/api';

// Response interfaces matching the backend DTOs
interface TodoResponse {
  id: string;
  title: string;
  description: string;
  priority: string;
  category: string;
  isCompleted: boolean;
  createdDate: string;
  dueDate?: string;
  tags: string[];
}

interface CategoryResponse {
  id: string;
  name: string;
  description: string;
  color: string;
  todoCount: number;
}

// Request interfaces for creating/updating
interface CreateTodoRequest {
  title: string;
  description: string;
  priority: Priority;
  category: string;
  dueDate?: string;
  tags: string[];
}

interface UpdateTodoRequest {
  title?: string;
  description?: string;
  priority?: Priority;
  category?: string;
  isCompleted?: boolean;
  dueDate?: string;
  tags?: string[];
}

interface CreateCategoryRequest {
  name: string;
  description: string;
  color: string;
}

interface UpdateCategoryRequest {
  name?: string;
  description?: string;
  color?: string;
}

// Create axios instance with default configuration
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10 second timeout
});

// Request interceptor to add auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Unauthorized - clear auth data and redirect to login (only if not already there)
      localStorage.removeItem('authToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      
      // Only redirect if not already on login page to avoid infinite loops
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    } else if (error.response) {
      // Server responded with error status
      console.error(`API Error: ${error.response.status} ${error.response.statusText}`);
      throw new Error(`API Error: ${error.response.status} ${error.response.statusText}`);
    } else if (error.request) {
      // Network error
      console.error('Network Error:', error.message);
      throw new Error('Network Error: Unable to connect to server');
    } else {
      // Other error
      console.error('Request Error:', error.message);
      throw error;
    }
  }
);

// Helper function to convert backend response to frontend Todo type
const mapTodoResponse = (response: TodoResponse): Todo => ({
  id: response.id,
  title: response.title,
  description: response.description,
  priority: response.priority as Priority,
  category: response.category,
  isCompleted: response.isCompleted,
  createdDate: new Date(response.createdDate),
  dueDate: response.dueDate ? new Date(response.dueDate) : undefined,
  tags: response.tags
});

// Helper function to convert frontend Todo to backend request
const mapTodoToRequest = (todo: Omit<Todo, 'id' | 'createdDate'>): CreateTodoRequest => ({
  title: todo.title,
  description: todo.description,
  priority: todo.priority,
  category: todo.category,
  dueDate: todo.dueDate?.toISOString(),
  tags: todo.tags
});

// Helper function to convert frontend Category to backend response
const mapCategoryResponse = (response: CategoryResponse): Category => ({
  id: response.id,
  name: response.name,
  description: response.description,
  color: response.color,
  todoCount: response.todoCount
});

// Generic API request helper (using axios)
async function apiRequest<T>(endpoint: string, method: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE' = 'GET', data?: any): Promise<T> {
  try {
    const response = await apiClient.request<T>({
      url: endpoint,
      method,
      data,
    });

    // Handle 204 No Content responses
    if (response.status === 204) {
      return {} as T;
    }

    return response.data;
  } catch (error) {
    console.error(`API request failed for ${endpoint}:`, error);
    throw error;
  }
}

// Todo API functions
export const todoApi = {
  // Get all todos
  async getAllTodos(): Promise<Todo[]> {
    const response = await apiRequest<TodoResponse[]>('/todos');
    return response.map(mapTodoResponse);
  },

  // Get todo by ID
  async getTodoById(id: string): Promise<Todo> {
    const response = await apiRequest<TodoResponse>(`/todos/${id}`);
    return mapTodoResponse(response);
  },

  // Create new todo
  async createTodo(todo: Omit<Todo, 'id' | 'createdDate'>): Promise<Todo> {
    const requestData = mapTodoToRequest(todo);
    const response = await apiRequest<TodoResponse>('/todos', 'POST', requestData);
    return mapTodoResponse(response);
  },

  // Update todo
  async updateTodo(id: string, updates: Partial<Todo>): Promise<Todo> {
    const requestData: UpdateTodoRequest = {};
    if (updates.title !== undefined) requestData.title = updates.title;
    if (updates.description !== undefined) requestData.description = updates.description;
    if (updates.priority !== undefined) requestData.priority = updates.priority;
    if (updates.category !== undefined) requestData.category = updates.category;
    if (updates.isCompleted !== undefined) requestData.isCompleted = updates.isCompleted;
    if (updates.dueDate !== undefined) requestData.dueDate = updates.dueDate?.toISOString();
    if (updates.tags !== undefined) requestData.tags = updates.tags;

    const response = await apiRequest<TodoResponse>(`/todos/${id}`, 'PUT', requestData);
    return mapTodoResponse(response);
  },

  // Delete todo
  async deleteTodo(id: string): Promise<void> {
    await apiRequest(`/todos/${id}`, 'DELETE');
  },

  // Toggle todo completion
  async toggleTodo(id: string): Promise<Todo> {
    const response = await apiRequest<TodoResponse>(`/todos/${id}/toggle`, 'PATCH');
    return mapTodoResponse(response);
  },

  // Get statistics
  async getStats(): Promise<TodoStats> {
    return await apiRequest<TodoStats>('/stats');
  }
};

// Category API functions
export const categoryApi = {
  // Get all categories
  async getAllCategories(): Promise<Category[]> {
    const response = await apiRequest<CategoryResponse[]>('/categories');
    return response.map(mapCategoryResponse);
  },

  // Get category by ID
  async getCategoryById(id: string): Promise<Category> {
    const response = await apiRequest<CategoryResponse>(`/categories/${id}`);
    return mapCategoryResponse(response);
  },

  // Create new category
  async createCategory(category: Omit<Category, 'id' | 'todoCount'>): Promise<Category> {
    const requestData: CreateCategoryRequest = {
      name: category.name,
      description: category.description,
      color: category.color
    };
    const response = await apiRequest<CategoryResponse>('/categories', 'POST', requestData);
    return mapCategoryResponse(response);
  },

  // Update category
  async updateCategory(id: string, updates: Partial<Category>): Promise<Category> {
    const requestData: UpdateCategoryRequest = {};
    if (updates.name !== undefined) requestData.name = updates.name;
    if (updates.description !== undefined) requestData.description = updates.description;
    if (updates.color !== undefined) requestData.color = updates.color;

    const response = await apiRequest<CategoryResponse>(`/categories/${id}`, 'PUT', requestData);
    return mapCategoryResponse(response);
  },

  // Delete category
  async deleteCategory(id: string): Promise<void> {
    await apiRequest(`/categories/${id}`, 'DELETE');
  }
};

// Weather API functions
export const weatherApi = {
  // Get current weather for a city
  async getCurrentWeather(city: string): Promise<WeatherInfo> {
    const response = await apiRequest<WeatherInfo>(`/weather/current/${encodeURIComponent(city)}`);
    return response;
  },

  // Get 5-day weather forecast for a city
  async getForecast(city: string): Promise<WeatherForecast5Day> {
    const response = await apiRequest<WeatherForecast5Day>(`/weather/forecast/${encodeURIComponent(city)}`);
    return response;
  },

  // Get weather-sensitive todos
  async getWeatherSensitiveTodos(): Promise<Todo[]> {
    const response = await apiRequest<TodoResponse[]>('/weather/todos');
    return response.map(mapTodoResponse);
  }
};

// Health check - using the proper TodoList API health endpoint
export const healthApi = {
  async checkHealth(): Promise<{ status: string; timestamp: string }> {
    try {
      // Health endpoint is at /health, not /api/health, so we call it directly
      const response = await axios.get<{ status: string; timestamp: string }>('http://localhost:5001/health');
      return response.data;
    } catch (error) {
      console.error('Health check failed:', error);
      throw new Error('Health check failed');
    }
  }
};
