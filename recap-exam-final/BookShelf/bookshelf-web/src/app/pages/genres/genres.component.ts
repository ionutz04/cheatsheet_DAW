import { Component, inject, signal } from '@angular/core';
import { GenresApi, GenreDto } from '../../core/api/genres.api';

@Component({
  selector: 'app-genres',
  standalone: true,
  imports: [],
  templateUrl: './genres.component.html'
})
export class GenresComponent {
  private readonly api = inject(GenresApi);
  readonly genres = signal<GenreDto[]>([]);
  readonly error = signal<string | null>(null);

  constructor() {
    this.api.getAll().subscribe({
      next: (g) => this.genres.set(g),
      error: () => this.error.set('Nu am putut încărca genurile.')
    });
  }
}
