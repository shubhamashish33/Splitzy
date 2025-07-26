import { Routes } from '@angular/router';
import { DashboardComponent } from './splitz/dashboard/dashboard.component';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { RecentactivityComponent } from './splitz/recentactivity/recentactivity.component';
import { GroupsComponent } from './splitz/dashboard/groups/groups.component';
import { LoginPageComponent } from './splitz/login-page/login-page.component';
import { RegisterPageComponent } from './splitz/register-page/register-page.component';


export const routes: Routes = [
    { path: 'login', component: LoginPageComponent },
    { path: 'register', component: RegisterPageComponent },
    {
        path: '',
        component: MainLayoutComponent,
        children: [
            // { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            { path: 'dashboard/:userId', component: DashboardComponent },
            { path: 'recent-activity/:userId', component: RecentactivityComponent },
            { path: 'group/:userId/:groupId', component: GroupsComponent }
            // Add more child routes here
        ]
    }
];
