import { Component, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ModalComponent } from '../../splitz/modal/modal.component';
import { CommonModule } from '@angular/common';
import { SplitzService } from '../../splitz/splitz.service';

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterModule,
    ModalComponent,
    CommonModule
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent implements OnInit {
  isModalOpen: boolean = false;
  modalType: 'expense' | 'settle' = 'expense';
  userId: string | null = null;
  token: string | null = null;

  constructor(private router: Router, private spltizService: SplitzService) {
    this.userId = localStorage.getItem('userId');
    this.token = localStorage.getItem('token');
    if (!this.userId && !this.token) {
      this.router.navigate(['/login']);
      this.spltizService.onFetchSecureLogin().subscribe((response: any) => {
        console.log('SSO Login URL:', response);
        //this need to be changed to a proper redirect.
      });
    }
    else {
    }
  }

  ngOnInit(): void {
    // Check userId on component init
    this.userId = localStorage.getItem('userId');
  }

  openModal(type: 'expense' | 'settle') {
    this.modalType = type;
    this.isModalOpen = true;
  }

  navigateToDashboard() {
    const userId = localStorage.getItem('userId');
    if (userId) {
      this.router.navigate(['/dashboard', userId]);
    }
  }

  navigateToRecentActivity() {
    const userId = localStorage.getItem('userId');
    if (userId) {
      this.router.navigate(['/recent-activity', userId]);
    }
  }
}
