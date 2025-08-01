import { Component, OnInit, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SplitzService } from '../splitz.service';
import { ActivatedRoute } from '@angular/router';
import { firstValueFrom } from 'rxjs';

interface ActivityItem {
  actor: string;
  action: string;
  expenseName: string;
  groupName: string;
  createdAt: string;
  impact: {
    type: 'get_back' | 'owe';
    amount: number;
  };
}
@Component({
  selector: 'app-recentactivity',
  imports: [CommonModule, FormsModule],
  templateUrl: './recentactivity.component.html',
  styleUrl: './recentactivity.component.css'
})
export class RecentactivityComponent implements OnInit {
  activityData: ActivityItem[] = [];
  userId: number | null = null;
  constructor(private splitzService: SplitzService, private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.getDataFromRouteParam();
  }
  private getDataFromRouteParam(): void {
    this.route.params.subscribe(params => {
      this.userId = params['userId'];
      this.loadActivityData();
    });
  }
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - date.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays === 1) return 'Today';
    if (diffDays === 2) return 'Yesterday';
    if (diffDays <= 7) return `${diffDays - 1} days ago`;
    return date.toLocaleDateString();
  }

  getExpenseIcon(expenseName: string): string {
    const name = expenseName.toLowerCase();
    if (name.includes('dinner') || name.includes('food') || name.includes('lunch')) {
      return 'fas fa-utensils';
    }
    if (name.includes('hotel') || name.includes('booking')) {
      return 'fas fa-bed';
    }
    if (name.includes('ticket')) {
      return 'fas fa-ticket-alt';
    }
    return 'fas fa-receipt';
  }
  trackByIndex(index: number, item: ActivityItem): number {
    return index;
  }

  async loadActivityData(): Promise<any> {
    if (!this.userId) return;
    try {
      this.activityData = await firstValueFrom(this.splitzService.getRecentActivity(this.userId));
    } catch (error) {
      console.error('Error loading activity data:', error);
    }
  }
}
