# BookShelf - frontend Angular

Frontend SPA (Angular 18) pentru API-ul **BookShelf**, în același stil ca exemplul profesorului
(TaskFlow). E un **scaffold gata făcut** - la recapitulare **nu îl modificăm**, doar îl rulăm ca să
vedem API-ul funcționând cu o interfață reală (ca la NewsPortal / TaskFlow).

## Cum îl rulezi

Frontend-ul e **deja build-uit în `../wwwroot/`** și servit de aplicația .NET - deci o singură
comandă, din folderul `BookShelf/`:

```sh
dotnet run
```

Apoi deschizi **`https://localhost:7080/`** (sau `http://localhost:5080/`) și vezi aplicația Angular.
API-ul e la `/api`, Swagger la `/swagger`. Cont admin: **admin@bookshelf.com / Admin@123**.

(Spre deosebire de TaskFlow-ul lui Mike, care rula `npm start` separat pe `:4200`, aici e **un
singur proces, o singură comandă**.)

### Dacă modifici frontend-ul (sursa e în acest folder)

```sh
npm install            # o singura data
npm run build          # -> dist/bookshelf-web/browser/
# apoi copiezi continutul din dist/bookshelf-web/browser/ in ../wwwroot/
```

## Ce conține (structura, ca la TaskFlow)

- `core/auth/` - `auth.service` (login/register, JWT în `localStorage`), `auth.interceptor`
  (adaugă `Authorization: Bearer ...`), `auth.guard` + `admin.guard`.
- `core/api/` - servicii tipate: `books.api`, `authors.api`, `genres.api`, `reviews.api`.
- `pages/` - `home`, `login`, `books` (listă + detaliu + creare/editare/ștergere),
  `authors` (admin), `genres` (public), `unauthorized`.

## De arătat studenților (mentalitatea MVC -> SPA)

- **Backend-ul nu mai e o fabrică de pagini**: controllerele întorc **JSON**, browser-ul randează UI-ul.
- **Autentificarea = atașezi o credențială la apeluri**: login e `POST /api/auth/login`, primești un
  token JWT, îl stochezi și interceptorul îl pune pe fiecare request.
- **Autorizarea are două straturi**: `[Authorize]` pe backend e gardianul real; `authGuard`/`adminGuard`
  în frontend sunt doar UX (un user tot poate apela API-ul direct).
- **Reviews**: pe pagina de detaliu a unei cărți, secțiunea Reviews apare **după Exercițiul 4**
  (când adăugăm entitatea `Review` și endpoint-ul `/api/reviews`). Înainte, frontend-ul tratează 404-ul.
