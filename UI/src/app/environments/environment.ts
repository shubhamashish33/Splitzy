export const environment = {
    production: false,
    apiBaseUrl: 'https://42761f8c7efd.ngrok-free.app',
    // apiBaseUrl: 'https://02f1b6b840d.ngrok-free.app',
    endpoints: {
        SECURE: '/api/Auth/secure',
        LOGIN: '/api/Auth/login',
        REGISTER: '/api/Auth/signup',
        DASHBOARD: '/api/Dashboard/dashboard',
        GROUP: '/api/Group/GetGroupOverview',
        EXPENSE: '/api/Expense/AddExpense',
        RECENT: '/api/Dashboard/recent'
    }
};