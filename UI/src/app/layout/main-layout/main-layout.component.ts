import { Component, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ModalComponent } from '../../splitz/modal/modal.component';
import { CommonModule } from '@angular/common';

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

  constructor(private router: Router) {
    this.userId = sessionStorage.getItem('userId');
    if (!this.userId) {
      this.router.navigate(['/login']);
    }
  }

  ngOnInit(): void {
    // Check userId on component init
    this.userId = sessionStorage.getItem('userId');
  }

  openModal(type: 'expense' | 'settle') {
    this.modalType = type;
    this.isModalOpen = true;
  }

  navigateToDashboard() {
    const userId = sessionStorage.getItem('userId');
    if (userId) {
      this.router.navigate(['/dashboard', userId]);
    }
  }

  navigateToRecentActivity() {
    const userId = sessionStorage.getItem('userId');
    if (userId) {
      this.router.navigate(['/recent-activity', userId]);
    }
  }
}
