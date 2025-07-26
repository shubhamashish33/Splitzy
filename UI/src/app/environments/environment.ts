export const environment = {
    production: false,
    // apiBaseUrl: 'https://f02f1b6b840d.ngrok-free.app',
    apiBaseUrl: 'https://02f1b6b840d.ngrok-free.app',
    endpoints: {
        LOGIN: '/api/Auth/login',
        REGISTER: '/api/Auth/signup',
        DASHBOARD: '/api/Dashboard/dashboard',
        GROUP: '/api/Group/GetGroupOverview',
        EXPENSE: '/api/Expense/AddExpense',
        RECENT: '/api/Dashboard/recent'
    }
};