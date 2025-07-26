import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  id: string;
  token?: string;
  message?: string;
}
export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}
export interface RegisterResponse {
  success: boolean;
  id?: string;
  message?: string;
}
@Injectable({
  providedIn: 'root'
})
export class SplitzService {
  private readonly BASE_URL = environment.apiBaseUrl;
  private readonly ENDPOINTS = environment.endpoints;

  private userIdSubject = new BehaviorSubject<string | null>(null);
  public userId$ = this.userIdSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

  login(loginData: LoginRequest): Observable<LoginResponse> {
    const headers = new HttpHeaders({
      'ngrok-skip-browser-warning': 'true'
    });
    return this.http.post<LoginResponse>(`${this.BASE_URL}${this.ENDPOINTS.LOGIN}`, loginData, { headers });
  }

  register(registerData: RegisterRequest): Observable<RegisterResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });
    return this.http.post<RegisterResponse>(`${this.BASE_URL}${this.ENDPOINTS.REGISTER}`, registerData, { headers });
  }

  setUserId(userId: string): void {
    sessionStorage.setItem('userId', userId);
    
    this.userIdSubject.next(userId);
  }

  getUserId(): string | null {
    return this.userIdSubject.value || sessionStorage.getItem('userId');
  }

  logout(): void {
    sessionStorage.removeItem('userId');
    sessionStorage.removeItem('token');
    
    this.userIdSubject.next(null);
    
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!this.getUserId();
  }

  redirectToDashboard(userId: string): void {
    this.router.navigate(['/dashboard', userId]);
  }
  onFetchDashboardData(id: any) {
    const url = `${this.BASE_URL}${this.ENDPOINTS.DASHBOARD}/${id}`;
    console.log(`Fetching dashboard data from ${url}`);
    const headers = new HttpHeaders({
      'ngrok-skip-browser-warning': 'true'
    });
    return this.http.get<any>(url, { headers });
  }
  onFetchGroupData(userId: any, groupId: any) {
    const url = `${this.BASE_URL}${this.ENDPOINTS.GROUP}/${userId}/${groupId}`;
    console.log(`Fetching group data from ${url}`);
    const headers = new HttpHeaders({
      'ngrok-skip-browser-warning': 'true'
    });
    return this.http.get<any>(url, { headers });
  }
  onSaveExpense(expense: any) {
    const url = `${this.BASE_URL}${this.ENDPOINTS.EXPENSE}`;
    console.log(`Saving expense to ${url}`);
    const headers = new HttpHeaders({
      'ngrok-skip-browser-warning': 'true',
      'Content-Type': 'application/json'
    });
    return this.http.post<any>(url, expense, { headers });
  }
  getRecentActivity(userId: any) {
    const url = `${this.BASE_URL}${this.ENDPOINTS.RECENT}/${userId}`;
    console.log(`Fetching recent activity from ${url}`);
    const headers = new HttpHeaders({
      'ngrok-skip-browser-warning': 'true'
    });
    return this.http.get<any[]>(url, { headers });
  }
}
