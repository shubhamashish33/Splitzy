import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
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
export class MainLayoutComponent {
  isModalOpen: boolean = false;
  modalType: 'expense' | 'settle' = 'expense';
  openModal(type: 'expense' | 'settle') {
    this.modalType = type;
    this.isModalOpen = true;
    }

}
