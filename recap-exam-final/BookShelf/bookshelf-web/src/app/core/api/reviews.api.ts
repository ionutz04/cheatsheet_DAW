import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// Endpoint-ul /api/reviews exista DOAR dupa Exercitiul 4 (entitatea Review).
// Pana atunci, apelurile dau 404 si componenta de detaliu trateaza eroarea.
export interface ReviewDto {
  id: number;
  bookId: number;
  bookTitle: string;
  reviewer: string;
  rating: number;
  comment: string | null;
  postedAt: string;
}

export interface ReviewInput {
  bookId: number;
  reviewer: string;
  rating: number;
  comment: string | null;
}

@Injectable({ providedIn: 'root' })
export class ReviewsApi {
  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<ReviewDto[]> {
    return this.http.get<ReviewDto[]>('/api/reviews');
  }

  create(input: ReviewInput): Observable<ReviewDto> {
    return this.http.post<ReviewDto>('/api/reviews', input);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/reviews/${id}`);
  }
}
