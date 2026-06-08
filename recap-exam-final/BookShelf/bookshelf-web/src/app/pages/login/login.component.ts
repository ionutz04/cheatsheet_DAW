import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly error = signal<string | null>(null);
  readonly busy = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  submit() {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/';
    this.run(this.auth.login(this.form.getRawValue()), 'Autentificare eșuată. Verifică email/parola.', returnUrl);
  }

  register() {
    this.run(this.auth.register(this.form.getRawValue()), 'Înregistrare eșuată. Poate există deja contul.', '/');
  }

  private run(request$: Observable<unknown>, failureMessage: string, redirectTo: string) {
    this.error.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.busy.set(true);
    request$.subscribe({
      next: async () => { await this.router.navigateByUrl(redirectTo); },
      error: () => { this.error.set(failureMessage); this.busy.set(false); },
      complete: () => this.busy.set(false)
    });
  }
}
