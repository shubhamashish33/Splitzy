export const environment = {
    production: false,
    apiBaseUrl: 'https://a9e0c23e0e53.ngrok-free.app',
    // apiBaseUrl: 'https://02f1b6b840d.ngrok-free.app',
    endpoints: {
        SSOLOGIN: '/api/Auth/ssologin',
        SECURE: '/api/Auth/secure',
        LOGIN: '/api/Auth/login',
        LOGOUT: '/api/Auth/logout',
        REGISTER: '/api/Auth/signup',
        DASHBOARD: '/api/Dashboard/dashboard',
        GROUP: '/api/Group/GetGroupOverview',
        EXPENSE: '/api/Expense/AddExpense',
        RECENT: '/api/Dashboard/recent'
    }
};