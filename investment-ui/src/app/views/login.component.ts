import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email = '';
  password = '';
  isLoading = false;
  errorMessage = '';
  isRegisterMode = false;

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.errorMessage = 'Email i hasło są wymagane';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const authCall = this.isRegisterMode
      ? this.authService.register(this.email, this.password)
      : this.authService.login(this.email, this.password);

    authCall.subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/']);
        } else {
          this.errorMessage = response.message || 'Błąd podczas logowania';
        }
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Auth error:', err);
        if (err.status === 0) {
          this.errorMessage = 'Nie można połączyć się z serwerem. Upewnij się, że backend jest uruchomiony na http://localhost:5247';
        } else if (err.status === 401) {
          this.errorMessage = 'Email lub hasło są nieprawidłowe';
        } else if (err.error?.message) {
          this.errorMessage = err.error.message;
        } else {
          this.errorMessage = 'Błąd połączenia z serwerem';
        }
        this.isLoading = false;
      }
    });
  }

  toggleMode(): void {
    this.isRegisterMode = !this.isRegisterMode;
    this.errorMessage = '';
  }
}
