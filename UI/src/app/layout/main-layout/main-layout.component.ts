import { Component, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ModalComponent } from '../../splitz/modal/modal.component';
import { CommonModule } from '@angular/common';
import { SplitzService } from '../../splitz/splitz.service';
import { LoginResponse } from '../../splitz/splitz.model';
import { LoaderComponent } from '../../splitz/loader/loader.component';

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterModule,
    ModalComponent,
    CommonModule,
    LoaderComponent
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent implements OnInit {
  isModalOpen: boolean = false;
  modalType: 'expense' | 'settle' = 'expense';
  userId: string | null = null;
  token: string | null = null;
  showLoader: boolean = true;

  constructor(private router: Router, private spltizService: SplitzService) {
  }
  
  ngOnInit(): void {
    this.userId = localStorage.getItem('userId');
    this.token = localStorage.getItem('token');
    this.checkAuthStatus();
  }
  checkAuthStatus() {
    if (this.userId && this.token) {
      this.showLoader = false;
      this.router.navigate(['/dashboard', this.userId]);
      return;
    }

    this.spltizService.onFetchSecureLogin().subscribe({
      next: (response: LoginResponse) => {
        this.showLoader = false;
        if (response.success && response.data) {
          this.spltizService.setUserId(response.data.id);
          this.spltizService.setToken(response.data.token);
          this.userId = response.data.id.toString();
          this.token = response.data.token;
          this.router.navigate(['/dashboard', this.userId]);
        }
        else {
          this.router.navigate(['/login']);
        }
      },
      error: (error) => {
        console.error('Auth check failed', error);
        this.showLoader = false;
        this.router.navigate(['/login']);
      }
    })
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
