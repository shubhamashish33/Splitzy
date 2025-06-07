import { Routes } from '@angular/router';
import { DashboardComponent } from './splitz/dashboard/dashboard.component';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { RecentactivityComponent } from './splitz/recentactivity/recentactivity.component';
import { GroupsComponent } from './splitz/dashboard/groups/groups.component';


export const routes: Routes = [
    {
        path: '',
        component: MainLayoutComponent,
        children: [
            // { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            { path: 'dashboard', component: DashboardComponent },
            { path: 'recent-activity', component: RecentactivityComponent },
            { path: 'group/:id', component: GroupsComponent }
            // Add more child routes here
        ]
    }
];
