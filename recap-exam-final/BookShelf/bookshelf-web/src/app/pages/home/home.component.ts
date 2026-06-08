import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './home.component.html'
})
export class HomeComponent {
  private readonly auth = inject(AuthService);
  readonly isAuthenticated = computed(() => this.auth.isAuthenticated());
  readonly isAdmin = computed(() => this.auth.isAdmin());
}
