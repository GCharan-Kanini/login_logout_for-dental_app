import { expect, test } from '@playwright/test';

// The state every logout scenario ends in: a signed-out visitor sees the sign-in view, and the protected
// endpoint refuses requests with no session. Both go through the dev proxy, as a real browser would.
test.describe('signed-out baseline', () => {
  test('a signed-out visitor is shown the sign-in view and no logout control', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Log out' })).toHaveCount(0);
  });

  test('the protected endpoint answers unauthenticated through the proxy', async ({ request }) => {
    const response = await request.get('/api/staff/me');

    expect(response.status()).toBe(401);
    expect(response.headers()['content-type']).toContain('application/problem+json');
    expect(await response.text()).not.toContain('user_id');
  });
});
