import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html'
})
export class AppComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly isAuthenticated = computed(() => this.auth.isAuthenticated());
  readonly isAdmin = computed(() => this.auth.isAdmin());

  async logout() {
    this.auth.logout();
    await this.router.navigateByUrl('/login');
  }
}
