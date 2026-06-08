import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BookDto {
  id: number;
  title: string;
  description: string;
  publishedAt: string;
  genreId: number;
  genreName: string;
  authorId: number;
  authorName: string;
}

export interface BookInput {
  title: string;
  description: string;
  genreId: number;
  authorId: number;
}

@Injectable({ providedIn: 'root' })
export class BooksApi {
  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<BookDto[]> {
    return this.http.get<BookDto[]>('/api/books');
  }

  getById(id: number): Observable<BookDto> {
    return this.http.get<BookDto>(`/api/books/${id}`);
  }

  create(input: BookInput): Observable<BookDto> {
    return this.http.post<BookDto>('/api/books', input);
  }

  update(id: number, input: BookInput): Observable<void> {
    return this.http.put<void>(`/api/books/${id}`, input);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/books/${id}`);
  }
}
