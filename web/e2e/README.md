# End-to-end tests

Browser tests for the logout story (US-008), run with Playwright against the real API and the Angular dev server.

## What runs today

- `signed-out-baseline.spec.ts`: a signed-out visitor sees the sign-in view with no logout control, and
  `GET /api/staff/me` through the dev proxy answers 401 `application/problem+json` without `user_id`.

## Planned scenarios (not yet implemented)

Each scenario starts from a signed-in staff member. The system has no way to create one yet (see below).

- **AC-1, successful logout:** sign in, use the logout control, expect the server to answer 204, then expect
  `/api/staff/me` with the same token to answer 401 and the sign-in view to be shown.
- **AC-2, replay after logout:** after logout, send the pre-logout token to `/api/staff/me` through the Playwright
  request context. Expect 401 and no staff data.
- **AC-3, failed termination and retry:** sign in, intercept `/api/auth/logout` to return 500, expect the inline
  "Logout failed" message and a working authenticated request. Remove the interception, press Retry, expect the
  session to end and the token to be rejected.

## Blocker

No sign-in endpoint or sign-in form exists, and the browser client holds its session only in memory. Nothing in the
running system can start a signed-in session, so these scenarios cannot be written against the real system yet.
Options, in the order we would pick them:

1. Build the sign-in story first. The scenarios then sign in through the UI.
2. Add a seed endpoint, mapped only in a dedicated `E2E` environment, plus a way to hand the token to the browser.
   This adds a test-only path to the production API and needs a decision.

## Running locally

From `web/`:

```sh
npx playwright install chromium          # first time only
npx playwright install-deps chromium     # needs root, once per machine
npm run e2e
```

`playwright.config.ts` starts the API (`dotnet run --launch-profile http` in `api/`, port 5080) and the dev server
(`ng serve`, port 4200). Locally it reuses servers that are already running. In CI (`CI` set) it starts fresh ones.

Results: the list reporter prints to the console, and the HTML report goes to `web/playwright-report/`. Traces for
failed tests are kept in `web/test-results/`.

## Running in CI

No CI step has been added. The repository rules forbid changing CI files in this task. The step to add to the
GitHub Actions workflow, after `npm ci` in `web/` and the .NET setup:

```yaml
- run: npx playwright install --with-deps chromium
  working-directory: web
- run: npm run e2e
  working-directory: web
- uses: actions/upload-artifact@v4
  if: always()
  with:
    name: playwright-report
    path: web/playwright-report/
```

Observability: nothing is wired. Sending results to a dashboard needs a decision on which tool, and it belongs in
the pipeline change.

## How these tests check server-side invalidation

A scenario passes only when the server, not the browser, refuses the old token. The replay checks send the
pre-logout token directly to the API, so a client that keeps local state cannot pass them.
