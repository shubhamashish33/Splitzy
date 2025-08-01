import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { LoginPageComponent } from './splitz/login-page/login-page.component';
import { RegisterPageComponent } from './splitz/register-page/register-page.component';

export const routes: Routes = [
    { path: 'login', component: LoginPageComponent },
    { path: 'register', component: RegisterPageComponent },
    {
        path: '',
        component: MainLayoutComponent,
        children: [
            {
                path: 'dashboard/:userId',
                loadComponent: () =>
                    import('./splitz/dashboard/dashboard.component').then(m => m.DashboardComponent),
            },
            {
                path: 'recent-activity/:userId',
                loadComponent: () =>
                    import('./splitz/recentactivity/recentactivity.component').then(m => m.RecentactivityComponent),
            },
            {
                path: 'group/:userId/:groupId',
                loadComponent: () =>
                    import('./splitz/dashboard/groups/groups.component').then(m => m.GroupsComponent),
            },
        ],
    },
];
