import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { adminGuard } from './core/auth/admin.guard';
import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './pages/login/login.component';
import { BooksComponent } from './pages/books/books.component';
import { BookDetailComponent } from './pages/books/book-detail.component';
import { AuthorsComponent } from './pages/authors/authors.component';
import { GenresComponent } from './pages/genres/genres.component';
import { UnauthorizedComponent } from './pages/unauthorized/unauthorized.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'genres', component: GenresComponent },
  { path: 'books', component: BooksComponent, canActivate: [authGuard] },
  { path: 'books/:id', component: BookDetailComponent, canActivate: [authGuard] },
  { path: 'authors', component: AuthorsComponent, canActivate: [adminGuard] },
  { path: 'unauthorized', component: UnauthorizedComponent },
  { path: '**', redirectTo: '' }
];
