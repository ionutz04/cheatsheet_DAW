import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface GenreDto {
  id: number;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class GenresApi {
  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<GenreDto[]> {
    return this.http.get<GenreDto[]>('/api/genres');
  }
}
