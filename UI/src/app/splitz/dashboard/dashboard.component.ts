import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

export interface Group {
  id: number;
  name: string;
  balance: number;
  description?: string;
  memberCount?: number;
  createdDate?: Date;
}

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    RouterModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {

  public val: number = 14;
  groups: Group[] = [
    { 
      id: 1, 
      name: 'Tin Factory', 
      balance: 1250, 
      description: 'Office expenses and team outings',
      memberCount: 5,
      createdDate: new Date('2024-01-15')
    },
    { 
      id: 2, 
      name: 'Weekend Trip', 
      balance: -500, 
      description: 'Goa vacation expenses',
      memberCount: 8,
      createdDate: new Date('2024-02-20')
    },
    { 
      id: 3, 
      name: 'Office Lunch', 
      balance: 200, 
      description: 'Daily lunch expenses',
      memberCount: 12,
      createdDate: new Date('2024-03-01')
    }
  ];
  constructor(private router: Router) {
    // Initialization logic can go here
  }
  navigateToGroup(groupId: number): void {
    this.router.navigate(['/group', groupId]);
  }


}
