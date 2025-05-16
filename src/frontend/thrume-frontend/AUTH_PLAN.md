# Authentication Implementation Plan

**Goal:** Implement Login and Registration functionality on the `/auth` route using Vue 3, TypeScript, Pinia, and the provided API specification.

**Core Technologies:**

*   Vue 3 (Composition API with `<script setup>`)
*   TypeScript
*   Pinia (for state management)
*   Vue Router
*   Native `fetch` API (for HTTP requests)
*   Standard HTML/CSS (no specific UI library)

**API Details:**

*   **Base URL:** `http://localhost:8080/api`
*   **Endpoints:**
    *   `POST /auth/register` (Body: `RegisterRequest { email, password }`)
    *   `POST /auth/login?useCookies=false` (Body: `LoginRequest { email, password }`)
*   **Responses:**
    *   Register: `200 OK` on success, `400 Bad Request` (with `HttpValidationProblemDetails`) on failure.
    *   Login: `200 OK` (with `AccessTokenResponse { accessToken, tokenType, expiresIn }`) on success, `400 Bad Request` on failure.

**Implementation Steps:**

1.  **Create Pinia Store (`src/stores/auth.ts`):**
    *   **Purpose:** Manage authentication state (token, user status), handle API calls, persist token.
    *   **State:** `accessToken`, `isAuthenticated`, `isLoading`, `error`, `registrationSuccess`.
    *   **Actions:** `login`, `register`, `logout`, `initializeAuth`, `clearError`, `resetRegistrationSuccess`.
    *   **Details:**
        *   `login`: Calls API, stores token in `localStorage` & state, handles errors.
        *   `register`: Calls API, sets success flag, handles errors.
        *   `logout`: Clears token from `localStorage` & state.
        *   `initializeAuth`: Checks `localStorage` on app load to restore session.

2.  **Create API Service (`src/services/authApi.ts`):**
    *   **Purpose:** Centralize API call logic using `fetch`.
    *   Define `API_BASE_URL`.
    *   Implement `apiLogin(credentials)` and `apiRegister(credentials)`.
    *   Handle request/response details (headers, JSON, error checking).

3.  **Update Auth Component (`src/components/Auth.vue`):**
    *   **Purpose:** Render Login/Register forms with tabs, handle input, interact with Pinia store.
    *   Use `<script setup>`, `ref` for form data and active tab (`login`/`register`).
    *   Import `useAuthStore` and `useRouter`.
    *   **Template:** Tabs, forms (`v-if` based on active tab), input binding (`v-model`), submit handlers (`@submit.prevent`), error display (`authStore.error`), loading state (`authStore.isLoading`), registration success message (`authStore.registrationSuccess`).
    *   **Logic:**
        *   `handleLogin`: Calls `authStore.login`, redirects to `/` on success.
        *   `handleRegister`: Calls `authStore.register`, switches to login tab and shows message on success.
        *   Use `watch` or lifecycle hooks to clear errors/flags.

4.  **Initialize Auth State (`src/App.vue`):**
    *   Import `useAuthStore`.
    *   Call `authStore.initializeAuth()` in `onMounted`.

5.  **Update Router (`src/router/index.ts`):**
    *   Ensure `/auth` route uses `Auth.vue`.
    *   Add a `/` route (e.g., `Home.vue`) for post-login redirect. Create `src/views/Home.vue` if needed.

**UI/UX:**

*   Login/Register forms on `/auth` page, switchable via tabs.
*   Successful Login: Store token (Pinia/localStorage), redirect to `/`.
*   Successful Registration: Show success message, redirect to `/auth` (login tab active).
*   Errors: Display below relevant form fields.

**Token Handling:**

*   Use JWT from `AccessTokenResponse`.
*   Store token in `localStorage` for persistence.
*   Manage active session state in Pinia store.

**Flow Diagram:**

```mermaid
graph LR
    subgraph Browser
        A[User Navigates to /auth] --> B(Auth.vue Component);
        B -- Shows Tabs --> C{Login Form};
        B -- Shows Tabs --> D{Register Form};
        C -- Input & Submit --> E[handleLogin];
        D -- Input & Submit --> F[handleRegister];
    end

    subgraph Pinia Store (auth.ts)
        G[State: token, error, loading, isAuthenticated];
        H[Action: login];
        I[Action: register];
        J[Action: initializeAuth];
        K[Action: logout];
    end

    subgraph API Service (authApi.ts)
        L[apiLogin];
        M[apiRegister];
    end

    subgraph Backend API
        N[POST /auth/login];
        O[POST /auth/register];
    end

    subgraph App Lifecycle
        P[App Mounts] --> Q(App.vue onMounted);
    end

    E --> H;
    F --> I;
    H --> L;
    I --> M;
    L --> N;
    M --> O;

    N -- Response --> L;
    O -- Response --> M;
    L -- Updates --> G;
    M -- Updates --> G;

    G -- Updates --> B; subgraph Updates UI

    Q --> J;
    J -- Checks localStorage --> G; subgraph Initializes State

    B -- Successful Login --> R(router.push('/'));
    B -- Successful Register --> C; subgraph Switch Tab

    style B fill:#f9f,stroke:#333,stroke-width:2px
    style G fill:#ccf,stroke:#333,stroke-width:2px