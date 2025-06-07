import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule, Location } from '@angular/common';

export interface Group {
  id: number;
  name: string;
  balance: number;
  description?: string;
  memberCount?: number;
  createdDate?: Date;
}

@Component({
  selector: 'app-groups',
  imports: [
    CommonModule,
  ],
  templateUrl: './groups.component.html',
  styleUrls: ['./groups.component.css']
})
export class GroupsComponent implements OnInit {
  groupId: number = 0;
  groupData: Group | null = null;
  activeTab: string = 'expenses'; // Default active tab

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.getDataFromRouteParams();
  }

  // Method 3: Getting data from route parameters (ID only)
  private getDataFromRouteParams(): void {
    this.route.params.subscribe(params => {
      this.groupId = +params['id']; // Convert string to number
      console.log('Group ID from route:', this.groupId);
      
      // If no data from state/query, fetch from service/API
      if (!this.groupData) {
        this.fetchGroupData(this.groupId);
      }
    });
  }

  // Method 4: Fetch data from service/API if not available
  private fetchGroupData(groupId: number): void {
    // Replace with actual service call
    // this.groupService.getGroup(groupId).subscribe(group => {
    //   this.groupData = group;
    // });
    
    // Mock data for demonstration
    const mockGroups: Group[] = [
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
      }
    ];
    
    this.groupData = mockGroups.find(g => g.id === groupId) || null;
    console.log('Group data fetched:', this.groupData);
  }

  // Navigation helper methods
  goBack(): void {
    this.location.back();
  }

  navigateToExpenses(): void {
    if (this.groupData) {
      this.router.navigate(['/group', this.groupData.id, 'expenses']);
    }
  }

  navigateToMembers(): void {
    if (this.groupData) {
      this.router.navigate(['/group', this.groupData.id, 'members']);
    }
  }

  // Tab management
  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  // Add expense functionality
  addExpense(): void {
    if (this.groupData) {
      this.router.navigate(['/group', this.groupData.id, 'add-expense']);
    }
  }

  // Add member functionality
  addMember(): void {
    if (this.groupData) {
      this.router.navigate(['/group', this.groupData.id, 'add-member']);
    }
  }
}