import { test, expect } from '../fixtures/authenticated.fixture';
import { Page } from '@playwright/test';
import { mockStsLogin, mockStsRefresh, mockAdminApi, mockUserApi } from '../mocks/api-mocks';
import { injectMockAuth } from '../helpers/auth.helper';
import { ADMIN_USER } from '../helpers/test-data';

/**
 * Admin Assistant Tests
 *
 * These tests verify the chat assistant UI behavior in various states.
 * The default mock in api-mocks.ts returns 503 for SignalR negotiate,
 * so the hub connection fails fast and the component settles into
 * a disconnected/unavailable state by default.
 */

const ADMIN_API = 'http://localhost:55106/api';

test.describe('Admin Assistant', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/assistant', { waitUntil: 'domcontentloaded' });
    // Wait for the chat component to render
    await expect(adminPage.locator('lib-chat')).toBeVisible({ timeout: 10000 });
  });

  test('assistant page loads without crash', async ({ adminPage }) => {
    const errors: string[] = [];
    adminPage.on('pageerror', (error) => {
      errors.push(error.message);
    });

    // Wait for change detection cycles to run
    await adminPage.waitForTimeout(2000);

    // Should not have TypeError from ViewChild/querySelector
    const viewChildErrors = errors.filter(
      (e) => e.includes('querySelector') || e.includes('nativeElement'),
    );
    expect(viewChildErrors).toHaveLength(0);
  });

  test('never shows raw error text', async ({ adminPage }) => {
    // Wait for connection attempt to settle (default mock returns 503 fast)
    await adminPage.waitForTimeout(2000);

    const pageText = (await adminPage.locator('lib-chat').textContent()) ?? '';

    // Should never contain raw exception details
    expect(pageText).not.toMatch(/HttpRequestException/i);
    expect(pageText).not.toMatch(/AmazonServiceException/i);
    expect(pageText).not.toMatch(/System\./);
    expect(pageText).not.toMatch(/stack\s*trace/i);
    expect(pageText).not.toMatch(/at\s+Bhbk\./);
    expect(pageText).not.toMatch(/NullReferenceException/i);
  });
});

test.describe('Assistant Status States', () => {
  test('shows "Connecting" initially while hub negotiates', async ({ adminPage }) => {
    // Block the SignalR negotiate so the component stays in "connecting" state
    // This overrides the default 503 mock (Playwright LIFO route matching)
    await adminPage.route(`${ADMIN_API}/hubs/chat/negotiate**`, (route) => {
      // Never fulfill — keeps the component in "Connecting to assistant..." state
      // (Playwright auto-aborts on navigation/close)
    });

    await adminPage.goto('/assistant', { waitUntil: 'domcontentloaded' });
    await expect(adminPage.locator('lib-chat')).toBeVisible({ timeout: 10000 });

    // Should show the connecting state with a loader
    const connectingText = adminPage.locator('lib-chat').getByText('Connecting to assistant...');
    await expect(connectingText).toBeVisible({ timeout: 5000 });
  });

  test('shows "unavailable" after connection failure', async ({ adminPage }) => {
    // Default mock already returns 503 for hub negotiate, but we can be explicit
    await adminPage.route(`${ADMIN_API}/hubs/chat/negotiate**`, (route) => {
      route.fulfill({ status: 500, body: 'Internal Server Error' });
    });

    await adminPage.goto('/assistant', { waitUntil: 'domcontentloaded' });
    await expect(adminPage.locator('lib-chat')).toBeVisible({ timeout: 10000 });

    // Should show unavailable state — scope search within lib-chat
    const unavailableText = adminPage.locator('lib-chat').getByText(/unavailable/i);
    await expect(unavailableText).toBeVisible({ timeout: 10000 });
  });

  test('sidebar is hidden when not fully connected', async ({ adminPage }) => {
    // Fail SignalR quickly
    await adminPage.route(`${ADMIN_API}/hubs/chat/negotiate**`, (route) => {
      route.fulfill({ status: 500, body: 'Internal Server Error' });
    });

    await adminPage.goto('/assistant', { waitUntil: 'domcontentloaded' });
    await expect(adminPage.locator('lib-chat')).toBeVisible({ timeout: 10000 });

    // Wait for connection attempt to settle
    await adminPage.waitForTimeout(2000);

    // Sidebar should NOT be visible when not fully connected
    const sidebar = adminPage.locator('lib-chat >> text=Favorites');
    await expect(sidebar).not.toBeVisible();

    // No sidebar toggle buttons in error states
    const sidebarToggle = adminPage.locator('lib-chat button[title="Open sidebar"]');
    await expect(sidebarToggle).not.toBeVisible();
  });

  test('header status dot reflects connection state', async ({ adminPage }) => {
    await adminPage.goto('/dashboard', { waitUntil: 'domcontentloaded' });

    // The assistant link with status dot should be visible
    const assistantLink = adminPage.locator('a[routerLink="/assistant"]');
    await expect(assistantLink).toBeVisible({ timeout: 10000 });

    // Status dot should exist
    const statusDot = assistantLink.locator('.rounded-full.inline-block');
    await expect(statusDot).toBeVisible();

    // Dot should have a color class (gray for disconnected, or pulsing yellow for connecting)
    const dotClasses = await statusDot.getAttribute('class');
    expect(dotClasses).toBeTruthy();
    // At minimum, it should have one of the status color classes
    expect(dotClasses).toMatch(/bg-(gray|yellow|red|orange|green)-400/);
  });
});

test.describe('Admin Chat Creation', () => {
  test('chat conversations REST endpoint returns data on page load', async ({ adminPage }) => {
    // Listen for the GET conversations request that fires on page load
    const getConversationsPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/chat/conversations') && req.method() === 'GET',
    );

    await adminPage.goto('/assistant', { waitUntil: 'domcontentloaded' });
    await expect(adminPage.locator('lib-chat')).toBeVisible({ timeout: 10000 });

    const req = await getConversationsPromise;
    expect(req.method()).toBe('GET');
  });

  test('creating a conversation via REST API returns valid response', async ({ adminPage }) => {
    await adminPage.goto('/assistant', { waitUntil: 'domcontentloaded' });
    await expect(adminPage.locator('lib-chat')).toBeVisible({ timeout: 10000 });

    // Call the conversations REST endpoint directly through the page's route mocks
    // This validates the mock returns the expected shape (same endpoint the chatStore uses)
    const response = await adminPage.evaluate(async () => {
      const res = await fetch('http://localhost:55106/api/chat/conversations', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: 'Test Chat' }),
      });
      return { status: res.status, data: await res.json() };
    });

    expect(response.status).toBe(200);
    expect(response.data).toHaveProperty('id');
    expect(response.data).toHaveProperty('title');
    expect(response.data).toHaveProperty('created');
  });

  test('chat input is not available when service is disconnected', async ({ adminPage }) => {
    await adminPage.goto('/assistant', { waitUntil: 'domcontentloaded' });
    await expect(adminPage.locator('lib-chat')).toBeVisible({ timeout: 10000 });

    // Wait for SignalR connection attempt to settle (503 from mock)
    await adminPage.waitForTimeout(2000);

    // The "unavailable" overlay should be showing (no textarea visible)
    const unavailableText = adminPage.locator('lib-chat').getByText(/unavailable/i);
    await expect(unavailableText).toBeVisible({ timeout: 5000 });

    // No text input should be visible in the disconnected state
    const textarea = adminPage.locator('lib-chat kendo-textarea');
    await expect(textarea).not.toBeVisible();
  });
});

test.describe('Admin Display Name', () => {
  test('user display name appears in hamburger menu', async ({ adminPage }) => {
    await adminPage.goto('/dashboard', { waitUntil: 'domcontentloaded' });

    // Click the hamburger menu button
    const menuButton = adminPage.locator('[data-menu-container] button');
    await expect(menuButton).toBeVisible({ timeout: 10000 });
    await menuButton.click();

    // The menu should show the user's display name
    // Mock JWT sets name: 'Admin User' for admin@local
    const displayNameEl = adminPage.locator('[data-menu-container] .text-gray-900');
    await expect(displayNameEl).toBeVisible();
    await expect(displayNameEl).toHaveText('Admin User');
  });

  test('display name works with .NET Microsoft-style claim names', async ({ page }) => {
    // The real .NET backend uses ClaimTypes.GivenName / ClaimTypes.Surname
    // which serialize as long XML namespace URIs, NOT standard JWT short names.
    // This test simulates a real .NET-issued JWT.
    await mockStsLogin(page);
    await mockAdminApi(page);
    await mockUserApi(page);

    // Block STS refresh so it can't replace our Microsoft-style JWT
    const STS_API = 'http://localhost:55114/api';
    await page.route(`${STS_API}/oauth2/v2/ropg-rt`, (route) =>
      route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'invalid_grant' }),
      }),
    );

    const header = { alg: 'none', typ: 'JWT' };
    const now = Math.floor(Date.now() / 1000);
    const payload = {
      sub: '44444444-4444-4444-4444-111111111111',
      iss: 'Bhbk',
      aud: ['identity-admin', 'identity-user'],
      exp: now + 3600,
      iat: now,
      nbf: now,
      jti: crypto.randomUUID(),
      // .NET ClaimTypes.Email → long URI
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'admin@local',
      // .NET ClaimTypes.GivenName → long URI (no short 'given_name')
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname': 'Admin',
      // .NET ClaimTypes.Surname → long URI (no short 'family_name')
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname': 'User',
      // .NET ClaimTypes.Role → long URI
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': ['Identity.Admins', 'Identity.Users'],
    };

    const encode = (obj: unknown) =>
      Buffer.from(JSON.stringify(obj)).toString('base64url');
    const msClaimsJwt = `${encode(header)}.${encode(payload)}.mock-signature`;

    // Inject the Microsoft-style JWT into localStorage
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.evaluate(
      ({ key, value }) => localStorage.setItem(key, value),
      {
        key: 'identity_access_token',
        value: JSON.stringify({
          accessToken: msClaimsJwt,
          expiresIn: 3600,
          savedAt: new Date().toISOString(),
        }),
      },
    );

    // Navigate — initFromStorage() should parse the JWT and extract the name
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForURL('**/dashboard', { timeout: 15000 });

    // Open the hamburger menu
    const menuButton = page.locator('[data-menu-container] button');
    await expect(menuButton).toBeVisible({ timeout: 10000 });
    await menuButton.click();

    // Should show "Admin User" even with Microsoft-style claim names
    const displayNameEl = page.locator('[data-menu-container] .text-gray-900');
    await expect(displayNameEl).toBeVisible();
    await expect(displayNameEl).toHaveText('Admin User');
  });
});

// ── Chat Scroll Behavior ─────────────────────────────────────────────

const SCROLL_CONV_ID = '22222222-2222-2222-2222-222222222222';
const RS = '\x1e'; // SignalR record separator

const SCROLL_CONVERSATION = {
  id: SCROLL_CONV_ID,
  title: 'Scroll Test Chat',
  started: '2026-01-01T00:00:00Z',
  ended: null,
  created: '2026-01-01T00:00:00Z',
};

function generateMessages(count: number) {
  return Array.from({ length: count }, (_, i) => ({
    id: `msg-${String(i).padStart(4, '0')}`,
    role: i % 2 === 0 ? 'user' : 'assistant',
    content:
      i % 2 === 0
        ? `User question number ${i + 1} about the identity system`
        : `This is assistant response number ${i + 1}. It contains some information about the identity system configuration and settings that the administrator asked about.`,
    inputTokens: i % 2 === 1 ? 100 : null,
    outputTokens: i % 2 === 1 ? 50 : null,
    created: new Date(Date.now() - (count - i) * 60000).toISOString(),
  }));
}

/** Captured WebSocket from routeWebSocket so tests can push server messages */
type MockWebSocket = { send: (msg: string) => void };

/**
 * Set up all mocks needed for a fully-connected chat state, including
 * a working SignalR negotiate + WebSocket mock. Must be called BEFORE
 * any navigation (so the MainLayoutComponent connects via our mock
 * rather than the default 503 stub).
 *
 * Returns an object with a `getWs()` method that waits for the WebSocket
 * to be established (it connects lazily when the app navigates).
 */
async function setupConnectedChat(page: Page): Promise<{ getWs: () => Promise<MockWebSocket> }> {
  // Promise that resolves when the WebSocket callback fires
  let resolveWs: (ws: MockWebSocket) => void;
  const wsPromise = new Promise<MockWebSocket>((r) => { resolveWs = r; });

  // 1. Standard API mocks
  await mockStsLogin(page);
  await mockStsRefresh(page);
  await mockAdminApi(page); // registers hubs/** → 503 and chat/** → []
  await mockUserApi(page);

  // 2. Override SignalR negotiate (LIFO: wins over hubs/**)
  await page.route(`${ADMIN_API}/hubs/chat/negotiate**`, (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        negotiateVersion: 1,
        connectionId: 'test-conn-id',
        connectionToken: 'test-conn-token',
        availableTransports: [
          { transport: 'WebSockets', transferFormats: ['Text', 'Binary'] },
        ],
      }),
    }),
  );

  // 3. Mock WebSocket (separate from page.route — no conflict with hubs/**)
  await page.routeWebSocket(`**/hubs/chat**`, (ws) => {
    resolveWs(ws);
    ws.onMessage((msg) => {
      const str = typeof msg === 'string' ? msg : '';
      for (const part of str.split(RS).filter(Boolean)) {
        // SignalR handshake (no "type" field)
        if (part.includes('"protocol"') && part.includes('"json"')) {
          ws.send(`{}${RS}`);
          continue;
        }
        try {
          const parsed = JSON.parse(part);
          // Ping → Pong
          if (parsed.type === 6) {
            ws.send(`{"type":6}${RS}`);
          }
          // Invocation
          if (parsed.type === 1) {
            if (parsed.target === 'CheckStatus') {
              ws.send(
                JSON.stringify({
                  type: 1,
                  target: 'Status',
                  arguments: [{ llmAvailable: true, providerName: 'Mock' }],
                }) + RS,
              );
            }
            // Send Completion for any invocation so invoke() resolves
            if (parsed.invocationId) {
              ws.send(
                JSON.stringify({ type: 3, invocationId: parsed.invocationId }) +
                  RS,
              );
            }
          }
        } catch {
          /* ignore malformed */
        }
      }
    });
  });

  // 4. Override chat REST API with our conversation + messages (LIFO: wins over chat/**)
  await page.route(`${ADMIN_API}/chat/**`, (route) => {
    const url = route.request().url();
    const method = route.request().method();

    if (method === 'GET') {
      if (url.includes(`${SCROLL_CONV_ID}/messages`)) {
        return route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(generateMessages(30)),
        });
      }
      if (url.includes(SCROLL_CONV_ID)) {
        return route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(SCROLL_CONVERSATION),
        });
      }
      // Conversation list
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([SCROLL_CONVERSATION]),
      });
    }
    if (method === 'POST') {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: crypto.randomUUID(),
          title: 'New Chat',
          created: new Date().toISOString(),
        }),
      });
    }
    if (method === 'DELETE') {
      return route.fulfill({ status: 200 });
    }
    return route.fallback();
  });

  // 5. Inject auth and navigate to dashboard (MainLayoutComponent.ngOnInit connects)
  await page.goto('/login', { waitUntil: 'domcontentloaded' });
  await injectMockAuth(page, ADMIN_USER.email);
  await page.goto('/login', { waitUntil: 'domcontentloaded' });
  await page.waitForURL('**/dashboard', { timeout: 15000 });

  return { getWs: () => wsPromise };
}

/** Navigate to assistant via SPA link (no page reload, preserves SignalR) */
async function navigateToAssistant(page: Page) {
  const assistantLink = page.locator('a[routerLink="/assistant"]');
  await expect(assistantLink).toBeVisible({ timeout: 10000 });
  await assistantLink.click();
  await page.waitForURL('**/assistant', { timeout: 10000 });
  await expect(page.locator('lib-chat')).toBeVisible({ timeout: 10000 });
}

/** Wait for the welcome screen (proves the component is in "connected and ready" state) */
async function waitForWelcomeScreen(page: Page) {
  await expect(page.getByText('How can I help you today?')).toBeVisible({
    timeout: 15000,
  });
}

/** Select a conversation and wait for its messages to render */
async function selectScrollConversation(page: Page) {
  const convItem = page.getByText('Scroll Test Chat');
  await expect(convItem).toBeVisible({ timeout: 5000 });
  await convItem.click();
  // Wait for the LAST message to be in the DOM (ensures all 30 messages rendered)
  await expect(
    page.locator('.hide-scrollbar').getByText('assistant response number 30'),
  ).toBeVisible({ timeout: 10000 });
  // Wait for Angular's scrollToBottom microtask to fire and layout to settle
  await page.waitForTimeout(1000);
}

test.describe('Chat Scroll Behavior', () => {
  test('selecting a conversation scrolls messages to the bottom', async ({
    page,
  }) => {
    await setupConnectedChat(page);
    await navigateToAssistant(page);
    await waitForWelcomeScreen(page);
    await selectScrollConversation(page);

    const isAtBottom = await page.evaluate(() => {
      const el = document.querySelector('.hide-scrollbar');
      if (!el) return false;
      return el.scrollHeight - el.scrollTop - el.clientHeight < 100;
    });
    expect(isAtBottom).toBe(true);
  });

  test('scroll-to-bottom button appears when user scrolls up', async ({
    page,
  }) => {
    await setupConnectedChat(page);
    await navigateToAssistant(page);
    await waitForWelcomeScreen(page);
    await selectScrollConversation(page);

    // Scroll to the top using scrollTo which fires a native scroll event
    await page.evaluate(() => {
      const el = document.querySelector('.hide-scrollbar');
      if (el) el.scrollTo({ top: 0 });
    });
    // Give Angular time to process the scroll event and run change detection
    await page.waitForTimeout(500);

    const scrollBtn = page.locator('button[title="Scroll to bottom"]');
    await expect(scrollBtn).toBeVisible({ timeout: 5000 });
  });

  test('clicking scroll-to-bottom button snaps back to bottom', async ({
    page,
  }) => {
    await setupConnectedChat(page);
    await navigateToAssistant(page);
    await waitForWelcomeScreen(page);
    await selectScrollConversation(page);

    // Scroll to the top using scrollTo which fires a native scroll event
    await page.evaluate(() => {
      const el = document.querySelector('.hide-scrollbar');
      if (el) el.scrollTo({ top: 0 });
    });
    await page.waitForTimeout(500);

    const scrollBtn = page.locator('button[title="Scroll to bottom"]');
    await expect(scrollBtn).toBeVisible({ timeout: 5000 });
    await scrollBtn.click();
    await page.waitForTimeout(500);

    // Should be back at bottom
    const isAtBottom = await page.evaluate(() => {
      const el = document.querySelector('.hide-scrollbar');
      if (!el) return false;
      return el.scrollHeight - el.scrollTop - el.clientHeight < 100;
    });
    expect(isAtBottom).toBe(true);

    // Button should be hidden
    await expect(scrollBtn).not.toBeVisible();
  });

  test('returning to assistant still shows messages at the bottom', async ({
    page,
  }) => {
    await setupConnectedChat(page);
    await navigateToAssistant(page);
    await waitForWelcomeScreen(page);
    await selectScrollConversation(page);

    // Navigate away to dashboard
    const dashboardLink = page.locator('a[routerLink="/dashboard"]');
    await dashboardLink.click();
    await page.waitForURL('**/dashboard', { timeout: 10000 });

    // Navigate back to assistant (SPA nav preserves store state)
    await navigateToAssistant(page);

    // The conversation should still be selected and messages visible
    await expect(
      page.locator('.hide-scrollbar').getByText('assistant response number 30'),
    ).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(1000);

    // Should be scrolled to the bottom
    const isAtBottom = await page.evaluate(() => {
      const el = document.querySelector('.hide-scrollbar');
      if (!el) return false;
      return el.scrollHeight - el.scrollTop - el.clientHeight < 100;
    });
    expect(isAtBottom).toBe(true);
  });

  test('streaming response keeps scroll pinned to the bottom', async ({
    page,
  }) => {
    const { getWs } = await setupConnectedChat(page);
    await navigateToAssistant(page);
    await waitForWelcomeScreen(page);
    // getWs() resolves once the WebSocket is established (during setupConnectedChat navigation)
    const serverWs = await getWs();
    await selectScrollConversation(page);

    // Type a message and send it
    const textarea = page.locator(
      'kendo-textarea[placeholder*="Ask me anything"]',
    );
    await textarea.click();
    await page.keyboard.type('Tell me about users in the system');
    await page.keyboard.press('Enter');

    // Wait for the user message to appear in the chat
    await expect(
      page.locator('.hide-scrollbar').getByText('Tell me about users'),
    ).toBeVisible({ timeout: 5000 });
    await page.waitForTimeout(300);

    // Simulate a multi-chunk streaming response via the SignalR WebSocket.
    // Each chunk is a SignalR Invocation (type 1) targeting "ReceiveChunk".
    const chunks = [
      'The identity system currently has several users configured. ',
      'Each user has associated roles, claims, and login providers. ',
      'Here is a detailed breakdown of the user accounts, their permissions, ',
      'and the various authentication methods configured for each one. ',
      'The system supports multiple OAuth2 grant types including resource owner, ',
      'client credentials, authorization code, and device code flows. ',
      'Additionally, each user can have multiple audience associations ',
      'that determine which applications they can access. ',
      'The role-based access control system provides fine-grained permissions ',
      'across all identity management operations in the platform.',
    ];

    for (const chunk of chunks) {
      serverWs.send(
        JSON.stringify({
          type: 1,
          target: 'ReceiveChunk',
          arguments: [{ type: 'content', content: chunk }],
        }) + RS,
      );
      // Small delay between chunks to simulate real streaming
      await page.waitForTimeout(100);
    }

    // Wait for streaming content to render
    await page.waitForTimeout(500);

    // Should still be at the bottom after all chunks
    const isAtBottomDuringStream = await page.evaluate(() => {
      const el = document.querySelector('.hide-scrollbar');
      if (!el) return false;
      return el.scrollHeight - el.scrollTop - el.clientHeight < 100;
    });
    expect(isAtBottomDuringStream).toBe(true);

    // Send the "complete" chunk to finish streaming
    serverWs.send(
      JSON.stringify({
        type: 1,
        target: 'ReceiveChunk',
        arguments: [
          { type: 'complete', content: '', inputTokens: 200, outputTokens: 150 },
        ],
      }) + RS,
    );
    await page.waitForTimeout(500);

    // Should still be at the bottom after streaming completes
    const isAtBottomAfterComplete = await page.evaluate(() => {
      const el = document.querySelector('.hide-scrollbar');
      if (!el) return false;
      return el.scrollHeight - el.scrollTop - el.clientHeight < 100;
    });
    expect(isAtBottomAfterComplete).toBe(true);
  });
});
