import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LoginRequest, SplitzService } from '../splitz.service';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LoaderComponent } from '../loader/loader.component';
import { SocialAuthService, GoogleLoginProvider, SocialUser } from '@abacritt/angularx-social-login';


@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, CommonModule, LoaderComponent],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.css'
})
export class LoginPageComponent {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  showPassword = false;
  showLoader = false;
  
  constructor(
    private authService: SocialAuthService,
    private fb: FormBuilder,
    private splitzService: SplitzService

  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    // Redirect if already logged in
    if (this.splitzService.isLoggedIn()) {
      const userId = this.splitzService.getUserId();
      if (userId) {
        this.splitzService.redirectToDashboard(userId);
      }
    }
  }

  get email() {
    return this.loginForm.get('email');
  }

  get password() {
    return this.loginForm.get('password');
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      const loginData: LoginRequest = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password
      };

      this.splitzService.login(loginData).subscribe({
        next: (response: any) => {
          this.showLoader = true;
          this.isLoading = false;

          if (response.success && response.data.id) {
            // Store user ID in service and session
            this.showLoader = false;
            this.splitzService.setUserId(response.data.id);

            // Store token if provided
            if (response.data.token) {
              sessionStorage.setItem('token', response.data.token);
            }

            // Redirect to dashboard with userId in URL
            this.splitzService.redirectToDashboard(response.data.id);
          } else {
            this.showLoader = false;
            this.errorMessage = response.message || 'Login failed. Please try again.';
          }
        },
        error: (error: any) => {
          this.isLoading = false;
          this.showLoader = false;
          console.error('Login error:', error);

          if (error.status === 401) {
            this.errorMessage = 'Invalid email or password.';
          } else if (error.status === 500) {
            this.errorMessage = 'Server error. Please try again later.';
          } else {
            this.errorMessage = error.error?.message || 'An error occurred. Please try again.';
          }
        }
      });
    } else {
      // Mark all fields as touched to show validation errors
      this.loginForm.markAllAsTouched();
    }
  }

  // Helper method to get error message for form fields
  getErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);

    if (field?.hasError('required')) {
      return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} is required`;
    }

    if (field?.hasError('email')) {
      return 'Please enter a valid email address';
    }

    if (field?.hasError('minlength')) {
      return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} must be at least 6 characters`;
    }

    return '';
  }
  ssoLogin(): void {
    this.authService.signIn(GoogleLoginProvider.PROVIDER_ID).then((user: SocialUser) => {
      if (user) {
        // Handle successful login with Google
        console.log('Google login successful:', user);
        this.splitzService.setUserId(user.id);
        sessionStorage.setItem('token', user.authToken || '');
        this.splitzService.redirectToDashboard(user.id);
      } else {
        console.error('Google login failed');
      }
    }).catch((error: any) => {
      console.error('Error during Google login:', error);
      this.errorMessage = 'Google login failed. Please try again.';
    });
  }
}
