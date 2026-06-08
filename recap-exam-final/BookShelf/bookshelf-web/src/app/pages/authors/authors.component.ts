import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthorsApi, AuthorDto } from '../../core/api/authors.api';
import { ModalComponent } from '../../shared/ui/modal/modal.component';

@Component({
  selector: 'app-authors',
  standalone: true,
  imports: [CommonModule, FormsModule, ModalComponent],
  templateUrl: './authors.component.html'
})
export class AuthorsComponent {
  private readonly api = inject(AuthorsApi);

  readonly authors = signal<AuthorDto[]>([]);
  readonly error = signal<string | null>(null);
  readonly busy = signal(true);

  readonly modalOpen = signal(false);
  readonly modalBusy = signal(false);
  readonly modalError = signal<string | null>(null);
  name = '';
  bio = '';

  constructor() {
    this.load();
  }

  load() {
    this.busy.set(true);
    this.error.set(null);
    this.api.getAll().subscribe({
      next: (a) => this.authors.set(a),
      error: () => this.error.set('Nu am putut încărca autorii.'),
      complete: () => this.busy.set(false)
    });
  }

  openCreate() {
    this.name = '';
    this.bio = '';
    this.modalError.set(null);
    this.modalOpen.set(true);
  }

  closeModal() {
    if (!this.modalBusy()) this.modalOpen.set(false);
  }

  save() {
    this.modalError.set(null);
    if (this.name.trim().length < 2) {
      this.modalError.set('Numele e obligatoriu (minim 2 caractere).');
      return;
    }
    this.modalBusy.set(true);
    this.api.create({ name: this.name.trim(), bio: this.bio.trim() || null }).subscribe({
      next: (a) => { this.authors.update((list) => [a, ...list]); this.modalOpen.set(false); },
      error: () => this.modalError.set('Nu am putut crea autorul.'),
      complete: () => this.modalBusy.set(false)
    });
  }
}
