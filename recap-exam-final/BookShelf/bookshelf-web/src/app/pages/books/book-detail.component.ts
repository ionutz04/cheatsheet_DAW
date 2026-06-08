import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BooksApi, BookDto } from '../../core/api/books.api';
import { ReviewsApi, ReviewDto } from '../../core/api/reviews.api';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-book-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './book-detail.component.html'
})
export class BookDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(BooksApi);
  private readonly reviewsApi = inject(ReviewsApi);
  private readonly auth = inject(AuthService);

  readonly book = signal<BookDto | null>(null);
  readonly error = signal<string | null>(null);
  readonly reviews = signal<ReviewDto[]>([]);
  readonly reviewsMissing = signal(false);
  readonly isAdmin = computed(() => this.auth.isAdmin());

  // Formularul "adauga recenzie" - functioneaza odata ce exista /api/reviews (dupa Exercitiul 4).
  reviewer = '';
  rating = 5;
  comment = '';
  readonly adding = signal(false);
  readonly addError = signal<string | null>(null);

  private bookId = 0;

  constructor() {
    this.bookId = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getById(this.bookId).subscribe({
      next: (b) => this.book.set(b),
      error: () => this.error.set('Cartea nu a fost găsită.')
    });
    this.loadReviews();
  }

  private loadReviews() {
    this.reviewsApi.getAll().subscribe({
      next: (rs) => this.reviews.set(rs.filter((r) => r.bookId === this.bookId)),
      error: () => this.reviewsMissing.set(true) // /api/reviews nu exista inca (Exercitiul 4)
    });
  }

  addReview() {
    this.addError.set(null);
    if (this.reviewer.trim().length < 2) { this.addError.set('Reviewer-ul e obligatoriu.'); return; }
    if (this.rating < 1 || this.rating > 5) { this.addError.set('Rating între 1 și 5.'); return; }

    this.adding.set(true);
    this.reviewsApi
      .create({ bookId: this.bookId, reviewer: this.reviewer.trim(), rating: this.rating, comment: this.comment.trim() || null })
      .subscribe({
        next: (r) => { this.reviews.update((l) => [...l, r]); this.reviewer = ''; this.comment = ''; this.rating = 5; },
        error: () => this.addError.set('Nu am putut adăuga recenzia.'),
        complete: () => this.adding.set(false)
      });
  }

  deleteReview(r: ReviewDto) {
    if (!confirm('Ștergi recenzia?')) return;
    this.reviewsApi.delete(r.id).subscribe({
      next: () => this.reviews.update((l) => l.filter((x) => x.id !== r.id)),
      error: () => this.addError.set('Nu am putut șterge (doar Admin).')
    });
  }
}
