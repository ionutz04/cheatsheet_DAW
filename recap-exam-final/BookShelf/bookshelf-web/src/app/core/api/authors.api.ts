import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AuthorDto {
  id: number;
  name: string;
  bio: string | null;
  bookCount: number;
  createdAt: string;
}

export interface AuthorInput {
  name: string;
  bio: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthorsApi {
  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<AuthorDto[]> {
    return this.http.get<AuthorDto[]>('/api/authors');
  }

  create(input: AuthorInput): Observable<AuthorDto> {
    return this.http.post<AuthorDto>('/api/authors', input);
  }
}
