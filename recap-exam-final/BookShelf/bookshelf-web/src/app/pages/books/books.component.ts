import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { BooksApi, BookDto, BookInput } from '../../core/api/books.api';
import { GenresApi, GenreDto } from '../../core/api/genres.api';
import { AuthorsApi, AuthorDto } from '../../core/api/authors.api';
import { AuthService } from '../../core/auth/auth.service';
import { ModalComponent } from '../../shared/ui/modal/modal.component';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ModalComponent],
  templateUrl: './books.component.html'
})
export class BooksComponent {
  private readonly api = inject(BooksApi);
  private readonly genresApi = inject(GenresApi);
  private readonly authorsApi = inject(AuthorsApi);
  private readonly auth = inject(AuthService);

  readonly books = signal<BookDto[]>([]);
  readonly genres = signal<GenreDto[]>([]);
  readonly authors = signal<AuthorDto[]>([]);
  readonly error = signal<string | null>(null);
  readonly busy = signal(true);
  readonly isAdmin = computed(() => this.auth.isAdmin());

  readonly modalOpen = signal(false);
  readonly modalBusy = signal(false);
  readonly modalError = signal<string | null>(null);
  readonly editingId = signal<number | null>(null);
  readonly modalTitle = computed(() => (this.editingId() == null ? 'Carte nouă' : 'Editează cartea'));
  form: BookInput = { title: '', description: '', genreId: 0, authorId: 0 };

  constructor() {
    this.load();
    this.genresApi.getAll().subscribe({ next: (g) => this.genres.set(g) });
    this.authorsApi.getAll().subscribe({ next: (a) => this.authors.set(a) });
  }

  load() {
    this.busy.set(true);
    this.error.set(null);
    this.api.getAll().subscribe({
      next: (b) => this.books.set(b),
      error: () => this.error.set('Nu am putut încărca cărțile (ești logat?).'),
      complete: () => this.busy.set(false)
    });
  }

  openCreate() {
    this.editingId.set(null);
    this.form = { title: '', description: '', genreId: this.genres()[0]?.id ?? 0, authorId: this.authors()[0]?.id ?? 0 };
    this.modalError.set(null);
    this.modalOpen.set(true);
  }

  openEdit(b: BookDto) {
    this.editingId.set(b.id);
    this.form = { title: b.title, description: b.description, genreId: b.genreId, authorId: b.authorId };
    this.modalError.set(null);
    this.modalOpen.set(true);
  }

  closeModal() {
    if (!this.modalBusy()) this.modalOpen.set(false);
  }

  save() {
    this.modalError.set(null);
    if (this.form.title.trim().length < 2) { this.modalError.set('Titlul e obligatoriu.'); return; }
    if (this.form.description.trim().length < 10) { this.modalError.set('Descrierea: minim 10 caractere.'); return; }
    if (!this.form.genreId || !this.form.authorId) { this.modalError.set('Alege genul și autorul.'); return; }

    const input: BookInput = { ...this.form, title: this.form.title.trim(), description: this.form.description.trim() };
    const id = this.editingId();
    this.modalBusy.set(true);

    if (id == null) {
      this.api.create(input).subscribe({
        next: (b) => { this.books.update((l) => [b, ...l]); this.modalOpen.set(false); },
        error: () => this.modalError.set('Nu am putut crea cartea.'),
        complete: () => this.modalBusy.set(false)
      });
    } else {
      this.api.update(id, input).subscribe({
        next: () => { this.load(); this.modalOpen.set(false); },
        error: () => this.modalError.set('Nu am putut salva modificările.'),
        complete: () => this.modalBusy.set(false)
      });
    }
  }

  delete(b: BookDto) {
    if (!confirm(`Ștergi cartea "${b.title}"?`)) return;
    this.api.delete(b.id).subscribe({
      next: () => this.books.update((l) => l.filter((x) => x.id !== b.id)),
      error: () => this.error.set('Nu am putut șterge (doar Admin poate).')
    });
  }
}
