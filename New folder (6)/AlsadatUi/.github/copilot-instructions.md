## Copilot / AI agent instructions for AlsadatUi

Short, actionable guidance so an AI coding assistant can be productive immediately.

- **Quick commands**
  - Dev server: `npm start` (alias for `ng serve`).
  - Build: `npm run build` (standard Angular build).
  - Tests: `npm run test` (runs Karma/Jasmine).
  - SSR runtime (expects built output): `npm run serve:ssr:AlsadatUi` which runs `node dist/alsadat-ui/server/server.mjs` (see `package.json`).

- **High-level architecture (quick)**
  - This is an Angular 17 app using the standalone bootstrap API: see `src/main.ts` (client) and `src/main.server.ts` (SSR bootstrap).
  - SSR entry + Express server: `server.ts` uses `@angular/ssr`'s `CommonEngine` and expects `index.server.html` inside the `dist/*/browser` output.
  - App wiring (routing, HTTP, locale, hydration) is in `src/app/app.config.ts` and `src/app/app.config.server.ts`.

- **Routing & components**
  - Central route table: `src/app/app.routes.ts`. Many routes use `canActivate: [authGuard]` and some use `loadComponent(...)` lazy loading — follow that pattern when adding pages.
  - Main authenticated layout is `SideBarComponent`; login uses `AuthLayout`.

- **HTTP, auth and conventions**
  - HTTP client is provided with `provideHttpClient(withFetch())` (see `app.config.ts`) — prefer using `HttpClient` as other services do.
  - Base API URL comes from `src/environments/environment.development.ts` (`environment.apiUrl`) and includes a trailing slash. Existing services concatenate endpoints like `${this.apiUrl}Auth/login`.
  - Auth token storage and keys: `accessToken`, `refreshToken`, `userName`, `userEmail` in `localStorage` (see `src/app/Services/auth-service.ts`).
  - `AuthInterceptor` (`src/app/interceptors/auth-interceptor.ts`) attaches `Authorization: Bearer <token>`, is SSR-safe (`typeof window === 'undefined'`), and implements a refresh-on-401 flow. Preserve this behavior when modifying http/auth code.

- **Localization & client hydration**
  - Arabic locale is registered in `src/app/app.config.ts` (`LOCALE_ID` = `ar-EG`, `registerLocaleData(localeAr)`). Keep numeric/date formatting and translation expectations in mind.
  - Client hydration is enabled via `provideClientHydration()` — avoid changes that break pre-rendered markup or hydration keys.

- **File / naming conventions to follow**
  - Major folders are capitalized: `Components/`, `Services/`, `Guards/`, `Layouts/`.
  - Services follow `*-service.ts` naming and are provided in `root` (see examples in `src/app/Services`).
  - Use Angular standalone components and `loadComponent` lazy pattern where similar routes already use it.

- **SSR-specific notes**
  - Avoid direct use of `window`, `localStorage`, or DOM APIs without guarding (`typeof window === 'undefined'`). See `AuthInterceptor` for the SSR-safe check.
  - The Express server entry is `server.ts` — when debugging SSR rendering, inspect the server-side `CommonEngine.render(...)` usage and `index.server.html` in the dist output.

- **When editing or adding APIs / services**
  - Match existing style: use `this.http.get<Result<T>>(
    `${this.apiUrl}Resource/endpoint`
  )` and keep `Result<T>` wrapper where used.
  - Preserve localStorage keys and refresh token flow. When adding new auth-related behavior, follow `AuthService` + `AuthInterceptor` pattern.

- **Where to look for examples**
  - Auth and tokens: `src/app/Services/auth-service.ts`
  - Interceptor and 401 refresh: `src/app/interceptors/auth-interceptor.ts`
  - App wiring: `src/app/app.config.ts`, `src/app/app.config.server.ts`
  - Routing: `src/app/app.routes.ts`
  - SSR entry: `server.ts`, `src/main.server.ts`

If anything above is unclear or you want the instructions to include more examples (component scaffolding, unit-test patterns, or a recommended commit message format), tell me which area to expand.
